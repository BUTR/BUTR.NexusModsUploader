namespace BUTR.NexusModsUploader.CommandsImplementation
{
    /// <summary>
    /// Used to create <typeparamref name="TCommand"/> instances.
    /// </summary>
    /// <typeparam name="TCommand">The type of command being requested.</typeparam>
    public interface ICommandsFactory<out TCommand> where TCommand : ICommand
    {
        /// <summary>
        /// Returns a configured <typeparamref name="TCommand"/> instance.
        /// </summary>
        TCommand Create();
    }
}