namespace NetworkStatusMonitor
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Owin;
    using Microsoft.Owin.Hosting;

    using Owin;

    public static class Program
    {
        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;

            Log.Application.Info("Application started.");

            var address = new IPAddress(0x08080808);
            var buffer = new byte[0];
            var pingOptions = new PingOptions(32, true);

            using (WebApp.Start("http://localhost:8080/", Startup))
            {
                TimerCallback callback =
                    state =>
                    {
                        var moment = DateTimeOffset.Now;

                        using (var ping = new Ping())
                        {
                            PingReply pingReply;

                            try
                            {
                                pingReply = ping.Send(address, 100, buffer, pingOptions);
                            }
                            catch (PingException)
                            {
                                Log.Status.Info("{0:yyyy-MM-dd HH:mm:ss} Error 0", moment);

                                return;
                            }

                            Log.Status.Info("{0:yyyy-MM-dd HH:mm:ss} {1:F} {2:D}", moment, pingReply.Status, pingReply.RoundtripTime);
                        }
                    };

                using (var timer = new Timer(callback, null, 0, 1000))
                {
                    Console.ReadKey(true);

                    timer.Change(-1, -1);
                }
            }

            Log.Application.Info("Application stopped.");
        }

        private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var id = Guid.NewGuid().ToString("D");

            Log.Application.Fatal("Unhandled exception ({0}).", id);

            var exception = (Exception) e.ExceptionObject;
            var message = FormatException(exception, id);

            Log.Exceptions.Fatal(message);
        }

        private static string FormatException(Exception exception, string id)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(id);

            FormatException(stringBuilder, exception);

            return stringBuilder.ToString();
        }

        private static void FormatException(StringBuilder stringBuilder, Exception exception)
        {
            var type = exception.GetType();

            stringBuilder.AppendLine(type.FullName);
            stringBuilder.AppendLine(exception.Message);
            stringBuilder.AppendLine(exception.StackTrace);

            if (exception.InnerException != null)
            {
                FormatException(stringBuilder, exception.InnerException);
            }
        }

        private static void Startup(IAppBuilder appBuilder)
        {
            Log.Application.Debug("Web server startup started.");

            appBuilder.Run(HandlerAsync);

            Log.Application.Debug("Web server startup completed.");
        }

        private static async Task HandlerAsync(IOwinContext context)
        {
            Log.Application.Debug("Web request handler started.");
            Log.Application.Debug("Request path: {0}", context.Request.Path.Value);

            context.Response.ContentType = "text/plain";

            if (context.Request.Path.Value.Equals("/", StringComparison.OrdinalIgnoreCase))
            {
                await HomePageHandlerAsync(context.Response);
            }
            else
            {
                await ResourceNotFoundHandlerAsync(context.Response);
            }

            Log.Application.Debug("Web request handler completed.");
        }

        private static async Task HomePageHandlerAsync(IOwinResponse response)
        {
            Log.Application.Debug("Home page handler started.");

            using (var streamWriter = new StreamWriter(response.Body))
            {
                await streamWriter.WriteLineAsync("TODO");
            }

            Log.Application.Debug("Home page handler completed.");
        }

        private static async Task ResourceNotFoundHandlerAsync(IOwinResponse response)
        {
            Log.Application.Debug("Resource not found handler started.");

            response.StatusCode = 404;

            using (var streamWriter = new StreamWriter(response.Body))
            {
                await streamWriter.WriteLineAsync("There is nothing here.");
            }

            Log.Application.Debug("Resource not found handler completed.");
        }
    }
}
