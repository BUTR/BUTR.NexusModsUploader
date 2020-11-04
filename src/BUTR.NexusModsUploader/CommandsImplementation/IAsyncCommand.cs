using System.Threading;
using System.Threading.Tasks;

namespace BUTR.NexusModsUploader.CommandsImplementation
{
    public interface IAsyncCommand : ICommand
    {
        Task<int> ExecuteAsync(CancellationToken cancellationToken);
    }
}