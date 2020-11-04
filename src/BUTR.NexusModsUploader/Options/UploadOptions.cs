namespace BUTR.NexusModsUploader.Options
{
    public class UploadOptions
    {
        public string GameId { get; set; } = default!;
        public int ModId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Version { get; set; } = default!;
        public bool IsLatest { get; set; } = default!;
        public bool IsNewOfExisting { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string FilePath { get; set; } = default!;
    }
}