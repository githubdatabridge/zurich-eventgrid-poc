using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Lib.Services.FileStorage
{
    public static class ServiceExtensions
    {
        private const string STORAGE_NAME = "AccoutStorageUri";
        public static IServiceCollection AddStoreServices(
                   this IServiceCollection services,
                   IConfiguration configuration)
        {
         
            services.AddAzureClients(clientBuilder =>
            {
                var cred = new DefaultAzureCredential();
                clientBuilder.UseCredential(cred);

                var conS = configuration.GetSection(STORAGE_NAME).Value
                    ?? throw new ArgumentNullException(nameof(STORAGE_NAME));

                clientBuilder.AddBlobServiceClient(new Uri(conS.Replace("__service__", "blob")));
                clientBuilder.AddQueueServiceClient(new Uri(conS.Replace("__service__", "queue")));
            });

            services.AddSingleton<InputStorageService>();
            services.AddSingleton<OutputStorageService>();

            return services;
        }
        public static IServiceCollection AddStoreServicesConnS(
                 this IServiceCollection services, IConfiguration configuration)
        {

            services.AddAzureClients(clientBuilder =>
            {
                var conS = configuration["StorageConString"];

                clientBuilder.AddBlobServiceClient(conS);
                clientBuilder.AddQueueServiceClient(conS);
            });

            services.AddSingleton<InputStorageService>();
            services.AddSingleton<OutputStorageService>();

            return services;
        }
    }
}


