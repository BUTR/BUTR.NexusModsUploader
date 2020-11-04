using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BUTR.NexusModsUploader.Models
{
    public class NexusFilesResponse
    {
        [JsonPropertyName("files")]
        public List<NexusFile> Files { get; set; } = default!;

        [JsonPropertyName("file_updated")]
        public List<object> Updates { get; set; } = default!;
    }
}