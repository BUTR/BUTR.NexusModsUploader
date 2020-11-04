using BUTR.NexusModsUploader.Commands;
using BUTR.NexusModsUploader.CommandsImplementation;
using BUTR.NexusModsUploader.Extensions;
using BUTR.NexusModsUploader.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.NexusModsUploader
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            await BuildCommandLine()
                .UseHost(_ => CreateHostBuilder(args))
                .UseDefaults()
                .Build()
                .InvokeAsync(args);
        }

        private static CommandLineBuilder BuildCommandLine()
        {
            var upload = new Command("upload")
            {
                new Option<string>("--gameId"),
                new Option<string>("--modId"),
                new Option<string>("--name"),
                new Option<string>("--version"),
                new Option<string>("--isLatest"),
                new Option<string>("--isNewOfExisting"),
                new Option<string>("--description"),
                new Option<string>("--filePath")
            };
            upload.Handler = CommandHandler.Create(async (IHost host) =>
            {
                var commandHandler = host.Services.GetRequiredService<IAsyncCommands<UploadCommand>>();
                await commandHandler.Command.ExecuteAsync(CancellationToken.None);
            });

            var check = new Command("check")
            {
                new Option<string>("--cookies")
            };
            check.Handler = CommandHandler.Create(async (IHost host) =>
            {
                var commandHandler = host.Services.GetRequiredService<IAsyncCommands<CheckCommand>>();
                await commandHandler.Command.ExecuteAsync(CancellationToken.None);
            });

            var root = new RootCommand
            {
                upload,
                check
            };
            return new CommandLineBuilder(root);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => new HostBuilder()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureHostConfiguration(config =>
            {
                config.AddEnvironmentVariables("DOTNET_");
                config.AddCommandLine(args);
            })
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddEnvironmentVariables("BUTR_NMU_");
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .ConfigureLogging((context, logging) =>
            {
                var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

                // IMPORTANT: This needs to be added *before* configuration is loaded, this lets
                // the defaults be overridden by the configuration.
                if (isWindows)
                {
                    // Default the EventLogLoggerProvider to warning or above
                    logging.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Warning);
                }

                logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
                logging.AddEventSourceLogger();

                if (isWindows)
                {
                    // Add the EventLogLoggerProvider on windows machines
                    logging.AddEventLog();
                }
            })
            .UseDefaultServiceProvider((context, options) =>
            {
                var isDevelopment = context.HostingEnvironment.IsDevelopment();
                options.ValidateScopes = isDevelopment;
                options.ValidateOnBuild = isDevelopment;
            })
            .ConfigureServices((context, services) =>
            {
                services.AddCommands();

                services.Configure<NexusModsApiOptions>(context.Configuration);
                services.Configure<NexusModsCookiesOptions>(context.Configuration);
                services.Configure<UploadOptions>(context.Configuration);

                services.AddNexusModsApiClient();
                services.AddNexusModsUploadClient();
                services.AddNexusModsClient();
            });
    }
}