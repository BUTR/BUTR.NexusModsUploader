using BUTR.NexusModsUploader.Models;
using BUTR.NexusModsUploader.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace BUTR.NexusModsUploader.Http
{
    public class NexusModsClientHandler : HttpClientHandler
    {
        private readonly ILogger<NexusModsClientHandler> _logger;

        public NexusModsClientHandler(IOptions<NexusModsCookiesOptions> options, ILogger<NexusModsClientHandler> logger)
        {
            _logger = logger;

            CookieContainer = new CookieContainer();

            if (options.Value != null && !string.IsNullOrWhiteSpace(options.Value.Cookies))
            {
                try
                {
                    var cookies = JsonSerializer.Deserialize<List<CookieEntry>>(options.Value.Cookies, new JsonSerializerOptions()
                    {
                        IgnoreNullValues = true,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        PropertyNameCaseInsensitive = true,
                        AllowTrailingCommas = true
                    });
                    foreach (var cookie in cookies)
                        CookieContainer.Add(new Cookie(cookie.Id, cookie.Value, cookie.Path, cookie.Domain));
                }
                catch (JsonException e)
                {
                    _logger.LogError(e, "Error while deserializing Cookies!");
                }
            }
        }
    }
}