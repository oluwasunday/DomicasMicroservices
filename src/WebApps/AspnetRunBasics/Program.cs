using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace AspnetRunBasics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /*var host = CreateHostBuilder(args).Build();
            SeedDatabase(host);
            host.Run();*/
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

    }
}
