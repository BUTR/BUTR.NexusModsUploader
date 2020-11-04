using System;
using BUTR.NexusModsUploader.Options;

using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BUTR.NexusModsUploader.Helpers;
using BUTR.NexusModsUploader.Models;

namespace BUTR.NexusModsUploader.Http
{
    public class NexusModsApiClient
    {
        private readonly NexusModsApiOptions _options;
        private readonly HttpClient _httpClient;

        public NexusModsApiClient(IOptions<NexusModsApiOptions> options, HttpClient httpClient)
        {
            _options = options.Value;
            _httpClient = httpClient;
        }

        public async Task<int> GetGameId(string gameName)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, $"games/{gameName}.json");
            req.Headers.Add("apikey", _options.ApiKey);
            var resp = await _httpClient.SendAsync(req);
            var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(await resp.Content.ReadAsStringAsync());
            var latest = dict["id"].ToString();
            return int.Parse(latest ?? "0");
        }

        public async Task<bool> GetApiKeyValidity()
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, "users/validate.json");
            req.Headers.Add("apikey", _options.ApiKey);
            var resp = await _httpClient.SendAsync(req);
            if (resp.IsSuccessStatusCode)
            {
                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(await resp.Content.ReadAsStringAsync());
                return !string.IsNullOrWhiteSpace(dict["name"].ToString()) && dict["key"].ToString() == _options.ApiKey;
            }
            else
            {
                return false;
            }
        }

        public async Task<int?> GetLatestFileId(string gameName, int modId)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"games/{gameName}/mods/{modId}/files.json");
                req.Headers.Add("apikey", _options.ApiKey);
                var resp = await _httpClient.SendAsync(req);
                var dict = System.Text.Json.JsonSerializer.Deserialize<NexusFilesResponse>(await resp.Content.ReadAsStringAsync());
                var mainFiles = dict.Files.Where(f => f.CategoryName != null && f.CategoryName == "MAIN").ToList();
                if (mainFiles.Count == 1)
                {
                    //well that was easy
                    return mainFiles[0].FileId;
                }
                else
                {
                    var ordered = mainFiles.OrderByDescending(mf => mf.FileVersion, new AlphanumComparatorFast());
                    return ordered.Last().FileId;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}