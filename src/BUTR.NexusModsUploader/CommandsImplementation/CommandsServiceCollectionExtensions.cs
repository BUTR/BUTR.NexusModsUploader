using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System;

namespace BUTR.NexusModsUploader.CommandsImplementation
{
    public static class CommandsServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services required for using commands.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddCommands(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.TryAdd(ServiceDescriptor.Singleton(typeof(IAsyncCommands<>), typeof(CommandsManager<>)));
            services.TryAdd(ServiceDescriptor.Transient(typeof(ICommandsFactory<>), typeof(CommandsFactory<>)));

            return services;
        }
    }
}