using BUTR.NexusModsUploader.CommandsImplementation;
using BUTR.NexusModsUploader.Http;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.NexusModsUploader.Commands
{
    public class CheckCommand : IAsyncCommand
    {
        private readonly NexusModsClient _nexusModsClient;

        public CheckCommand(NexusModsClient nexusModsClient)
        {
            _nexusModsClient = nexusModsClient;
        }

        public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
        {
            return await _nexusModsClient.GetSessionValidity() ? 0 : 1;
        }
    }
}