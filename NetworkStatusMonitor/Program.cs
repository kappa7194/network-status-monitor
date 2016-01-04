namespace NetworkStatusMonitor
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.Owin;
    using Microsoft.Owin.Hosting;

    using Owin;

    public static class Program
    {
        public static void Main()
        {
            using (WebApp.Start("http://localhost:8080/", Startup))
            {
                Console.ReadLine();
            }
        }

        private static void Startup(IAppBuilder appBuilder)
        {
            appBuilder.Run(HandlerAsync);
        }

        private static async Task HandlerAsync(IOwinContext context)
        {
            context.Response.ContentType = "text/plain";

            if (context.Request.Path.Value.Equals("/", StringComparison.OrdinalIgnoreCase))
            {
                await HomePageAsync(context.Response);
            }
            else
            {
                await NotFoundAsync(context.Response);
            }
        }

        private static async Task HomePageAsync(IOwinResponse response)
        {
            using (var streamWriter = new StreamWriter(response.Body))
            {
                await streamWriter.WriteLineAsync("TODO");
            }
        }

        private static async Task NotFoundAsync(IOwinResponse response)
        {
            response.StatusCode = 404;

            using (var streamWriter = new StreamWriter(response.Body))
            {
                await streamWriter.WriteLineAsync("There is nothing here.");
            }
        }
    }
}
