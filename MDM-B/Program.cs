using Shared.Lib.Services.FileStorage;

namespace MDM.B
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((builder,services) =>
                {
                    services.AddStoreServices(builder.Configuration);
                    services.AddHostedService<Worker>();
                })
                .Build();

            host.Run();
        }
    }
}