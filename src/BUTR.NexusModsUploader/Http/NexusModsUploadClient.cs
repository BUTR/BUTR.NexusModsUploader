using BUTR.NexusModsUploader.Models;
using BUTR.NexusModsUploader.Options;

using Flurl;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BUTR.NexusModsUploader.Http
{
    public class NexusModsUploadClient
    {
        private readonly NexusModsApiOptions _options;
        private readonly HttpClient _httpClient;

        public NexusModsUploadClient(IOptions<NexusModsApiOptions> options, HttpClient httpClient)
        {
            _options = options.Value;
            _httpClient = httpClient;
        }

        public async Task<UploadedFile?> UploadFile(FileInfo file)
        {
            var zipContentType = "application/x-zip-compressed";

            var chunkSize = 5242880;
            int GetChunkSize(int i)
            {
                var position = (i * (long) chunkSize);
                var toRead = (int) Math.Min(file.Length - position + 1, chunkSize);
                return toRead;
            }
            string GetIdentifier() => $"{file.Length}{file.Name.Replace(".", "")}";

            var totalChunks = (int)(file.Length / chunkSize);
            if (file.Length % chunkSize != 0)
            {
                totalChunks++;
            }

            using (var str = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                for (var i = 0; i < totalChunks; i++)
                {
                    var toRead = GetChunkSize(i);
                    var path = "/uploads/chunk"
                    .SetQueryParams(new
                    {
                        resumableChunkNumber = totalChunks,
                        resumableChunkSize = chunkSize,
                        resumableCurrentChunkSize = GetChunkSize(i),
                        resumableTotalSize = file.Length,
                        resumableType = zipContentType,
                        resumableIdentifier = GetIdentifier(),
                        resumableFilename = file.Name,
                        resumableRelativePath = file.Name,
                        resumableTotalChunks = totalChunks
                    });
                    var getResp = await _httpClient.GetAsync(path.ToString());
                    if (getResp.StatusCode != HttpStatusCode.NoContent)
                    {
                        throw new Exception("I don't even know what this means");
                    }
                    var buffer = new byte[toRead];
                    await str.ReadAsync(buffer, 0, buffer.Length);
                    using (var form = new MultipartFormDataContent())
                    {
                        form.Add(new StringContent((i + 1).ToString()), "resumableChunkNumber");
                        form.Add(ToContent(chunkSize), "resumableChunkSize");
                        form.Add(ToContent(toRead), "resumableCurrentChunkSize");
                        form.Add(ToContent(file.Length), "resumableTotalSize");
                        form.Add(ToContent(zipContentType), "resumableType");
                        form.Add(ToContent(GetIdentifier()), "resumableIdentifier");
                        form.Add(ToContent(file.Name), "resumableFilename");
                        form.Add(ToContent(file.Name), "resumableRelativePath");
                        form.Add(ToContent(totalChunks), "resumableTotalChunks");
                        form.Add(new ByteArrayContent(buffer), "file", "blob");
                        //  new StreamContent(str, toRead)
                        // form.Add(new ByteArrayContent(buffer), "file", "blob");
                        var response = await _httpClient.PostAsync("/uploads/chunk", form).ConfigureAwait(false);
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception("I don't know what this means either");
                        }
                        if (response.Content.Headers.Contains("Content-Type"))
                        {
                            var resp = System.Text.Json.JsonSerializer.Deserialize<UploadedFile>(await response.Content.ReadAsStringAsync());
                            if (!string.IsNullOrWhiteSpace(resp.Id))
                            {
                                resp.FileSize = (int)file.Length;
                                resp.OriginalFile = file.Name;
                                return resp;
                            }

                        }
                    }
                }
            }

            return null;
        }

        public async Task<bool> CheckStatus(UploadedFile upload)
        {
            var ready = false;
            var attempt = 1;
            do
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
                var json = await _httpClient.GetStringAsync($"/uploads/check_status?id={upload.Id}");
                var resp = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                var assembled = bool.TryParse(resp["file_chunks_reassembled"]?.ToString(), out ready);
            } while (!ready);
            return ready;
        }


        private static StringContent ToContent(int i) => new StringContent(i.ToString());
        private static StringContent ToContent(string s) => new StringContent(string.IsNullOrWhiteSpace(s) ? string.Empty : s);
        private static StringContent ToContent(long l) => new StringContent(l.ToString());
    }
}