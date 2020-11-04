namespace BUTR.NexusModsUploader.CommandsImplementation
{
    public interface IAsyncCommands<out TCommand> where TCommand : IAsyncCommand
    {
        /// <summary>
        /// The <typeparamref name="TCommand"/> instance
        /// </summary>
        TCommand Command { get; }
    }
}