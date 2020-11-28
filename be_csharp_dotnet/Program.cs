using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TodoList
{
    public class Program
    {
        public static async Task Main( String[] args )
        {
            await CreateHostBuilder( args )
                  .Build()
                  .RunAsync();
        }

        private static IHostBuilder CreateHostBuilder( String[] args ) =>
            Host
                .CreateDefaultBuilder( args )
                .ConfigureLogging( logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                } )
                .ConfigureWebHostDefaults( webBuilder => webBuilder.UseStartup<Startup>() );
    }
}