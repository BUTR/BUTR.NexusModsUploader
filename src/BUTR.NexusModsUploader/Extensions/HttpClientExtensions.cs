using BUTR.NexusModsUploader.Http;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Net.Http.Headers;
using System.Reflection;

namespace BUTR.NexusModsUploader.Extensions
{
    public static class HttpClientExtensions
    {
        public static IHttpClientBuilder AddNexusModsApiClient(this IServiceCollection services)
        {
            var assemblyName = Assembly.GetEntryAssembly()!.GetName();

            return services.AddHttpClient<NexusModsApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.nexusmods.com/v1/");
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(assemblyName.Name, assemblyName.Version?.ToString(4) ?? "0.0.0.0"));
            });
        }

        public static IHttpClientBuilder AddNexusModsUploadClient(this IServiceCollection services)
        {
            var assemblyName = Assembly.GetEntryAssembly()!.GetName();

            services.AddSingleton<NexusModsClientHandler>();
            return services.AddHttpClient<NexusModsUploadClient>(client =>
            {
                client.BaseAddress = new Uri("https://upload.nexusmods.com");
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(assemblyName.Name, assemblyName.Version?.ToString(4) ?? "0.0.0.0"));
            }).ConfigurePrimaryHttpMessageHandler<NexusModsClientHandler>();
        }

        public static IHttpClientBuilder AddNexusModsClient(this IServiceCollection services)
        {
            var assemblyName = Assembly.GetEntryAssembly()!.GetName();

            services.AddSingleton<NexusModsClientHandler>();
            return services.AddHttpClient<NexusModsClient>(client =>
            {
                client.BaseAddress = new Uri("https://www.nexusmods.com");
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(assemblyName.Name, assemblyName.Version?.ToString(4) ?? "0.0.0.0"));
            }).ConfigurePrimaryHttpMessageHandler<NexusModsClientHandler>();
        }
    }
}