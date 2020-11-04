using Microsoft.Extensions.DependencyInjection;

using System;

namespace BUTR.NexusModsUploader.CommandsImplementation
{
    /// <summary>
    /// Implementation of ICommandsFactory.
    /// </summary>
    /// <typeparam name="TCommand">The type of command being requested.</typeparam>
    public class CommandsFactory<TCommand> : ICommandsFactory<TCommand> where TCommand : IAsyncCommand
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandsFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TCommand Create() => ActivatorUtilities.CreateInstance<TCommand>(_serviceProvider);
    }
}