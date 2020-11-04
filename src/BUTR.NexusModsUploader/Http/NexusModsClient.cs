using BUTR.NexusModsUploader.Models;
using BUTR.NexusModsUploader.Options;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace BUTR.NexusModsUploader.Http
{
    public class NexusModsClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NexusModsClient> _logger;

        public NexusModsClient(HttpClient httpClient, ILogger<NexusModsClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> GetSessionValidity()
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, "/Core/Libs/Common/Widgets/MyModerationHistoryTab");
                var resp = await _httpClient.SendAsync(req);
                if (!resp.IsSuccessStatusCode)
                    return false;

                await using var stream = await resp.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);
                string? line;
                var lineOffset = 0;
                while ((line = await reader.ReadLineAsync()) != null && lineOffset < 10)
                {
                    if (line.Contains("og:") || line.Contains("Error"))
                    {
                        return false;
                    }

                    lineOffset++;
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while checking session validity!");
                return false;
            }
        }

        public async Task<bool> AddFile(int gameId, string gameName, int modId, UploadedFile upload, UploadOptions options)
        {
            var uri = "/Core/Libs/Common/Managers/Mods?AddFile";
            var message = new HttpRequestMessage(HttpMethod.Post, uri);
            message.Headers.Add("Referer", $"https://www.nexusmods.com/{gameName}/mods/edit/?step=docs&id={modId}");
            using (var content = new MultipartFormDataContent())
            {
                content.Add(ToContent(gameId), "game_id");
                content.Add(ToContent(options.Name), "name");
                content.Add(ToContent(options.Version), "file-version");
                content.Add(options.IsLatest ? ToContent(1) : ToContent(0), "update-version");
                content.Add(options.IsLatest ? ToContent(1) : ToContent(0), "set_as_main_nmm");
                content.Add(ToContent(1), "category");
                //if (options.PreviousFileId.HasValue)
                //{
                //    content.Add(ToContent(1), "new-existing");
                //    content.Add(ToContent(options.PreviousFileId.Value), "old_file_id");
                //}
                content.Add(ToContent(options.Description), "brief-overview");
                content.Add(ToContent(upload.Id), "file_uuid");
                content.Add(ToContent(upload.FileSize), "file_size");
                content.Add(ToContent(modId), "mod_id");
                content.Add(ToContent(modId), "id");
                content.Add(ToContent("add"), "action");
                content.Add(ToContent(upload.FileName), "uploaded_file");
                content.Add(ToContent(upload.OriginalFile), "original_file");
                message.Content = content;
                var resp = await _httpClient.SendAsync(message);
                if (resp.IsSuccessStatusCode)
                {
                    var strResponse = await resp.Content.ReadAsStringAsync();
                    var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(strResponse);
                    var success = data.ContainsKey("status") && data["status"].ToString() == true.ToString();
                    if (success)
                    {
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("Response received from Nexus Mods: " + data["message"]);
                    }
                }
                return false;
            }
        }

        private static StringContent ToContent(int i) => new StringContent(i.ToString());
        private static StringContent ToContent(string s) => new StringContent(string.IsNullOrWhiteSpace(s) ? string.Empty : s);
        private static StringContent ToContent(long l) => new StringContent(l.ToString());
    }
}