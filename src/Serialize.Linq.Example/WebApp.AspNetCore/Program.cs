using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace WebApp.AspNetCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            // initialize repo
            var persons = PersonRepository.Current;

            host.Run();
        }
    }
}
