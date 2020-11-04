namespace BUTR.NexusModsUploader.CommandsImplementation
{
    public class CommandsManager<TCommand> : IAsyncCommands<TCommand> where TCommand : IAsyncCommand
    {
        private readonly ICommandsFactory<TCommand> _factory;

        /// <summary>
        /// Initializes a new instance with the specified command.
        /// </summary>
        /// <param name="factory">The factory to use to create command.</param>
        public CommandsManager(ICommandsFactory<TCommand> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// The default <typeparamref name="TCommand"/> instance, equivalent to Get().
        /// </summary>
        public TCommand Command => Get();

        /// <summary>
        /// Returns a configured <typeparamref name="TCommand"/> instance.
        /// </summary>
        public virtual TCommand Get() => _factory.Create();
    }
}