using BUTR.NexusModsUploader.CommandsImplementation;
using BUTR.NexusModsUploader.Http;
using BUTR.NexusModsUploader.Options;

using Microsoft.Extensions.Options;

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.NexusModsUploader.Commands
{
    public class UploadCommand : IAsyncCommand
    {
        private readonly UploadOptions _options;
        private readonly NexusModsApiClient _nexusModsApiClient;
        private readonly NexusModsUploadClient _nexusModsUploadClient;
        private readonly NexusModsClient _nexusModsClient;

        public UploadCommand(IOptions<UploadOptions> options, NexusModsApiClient nexusModsApiClient, NexusModsUploadClient nexusModsUploadClient, NexusModsClient nexusModsClient)
        {
            _options = options.Value;
            _nexusModsApiClient = nexusModsApiClient;
            _nexusModsUploadClient = nexusModsUploadClient;
            _nexusModsClient = nexusModsClient;
        }

        public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(_options.FilePath))
                return 1;

            if (!await _nexusModsApiClient.GetApiKeyValidity() || !await _nexusModsClient.GetSessionValidity())
                return 1;

            var gameId = await _nexusModsApiClient.GetGameId(_options.GameId);

            if (false)
            {
                var fileId = await _nexusModsApiClient.GetLatestFileId(_options.GameId, _options.ModId);
                ;
            }

            var upload = await _nexusModsUploadClient.UploadFile(new FileInfo(Path.GetFullPath(_options.FilePath)));
            if (upload == null)
                return 1;
            var available = await _nexusModsUploadClient.CheckStatus(upload);
            var success = await _nexusModsClient.AddFile(gameId, _options.GameId, _options.ModId, upload, _options);

            return 1;
        }
    }
}