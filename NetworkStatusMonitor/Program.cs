namespace NetworkStatusMonitor
{
    using System;
    using System.Collections.Generic;
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
        private readonly static IPAddress address = new IPAddress(0x08080808);
        private readonly static byte[] buffer = new byte[0];
        private readonly static PingOptions pingOptions = new PingOptions(32, true);

        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;

            Log.Application.Info("Application starting.");
            Log.Application.Debug("Web server starting.");

            using (WebApp.Start("http://localhost:8080/", Startup))
            {
                Log.Application.Debug("Web server started.");

                using (var timer = new Timer(Callback, null, 0, 1000))
                {
                    Log.Application.Info("Application started.");

                    Console.ReadKey(true);

                    Log.Application.Info("Application stopping.");
                    Log.Application.Debug("Timer deactivating.");

                    timer.Change(-1, -1);

                    Log.Application.Debug("Timer deactivated.");
                    Log.Application.Debug("Timer stopping.");
                }

                Log.Application.Debug("Timer stopped.");
                Log.Application.Debug("Web server stopping.");
            }

            Log.Application.Debug("Web server stopped.");
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
            Log.Application.Debug("Web server configuration started.");

            appBuilder.Run(HandlerAsync);

            Log.Application.Debug("Web server configuration completed.");
        }

        private static async Task HandlerAsync(IOwinContext context)
        {
            Log.Application.Debug("Web request handler started.");
            Log.Application.Debug("Request path: {0}", context.Request.Path.Value);

            context.Response.ContentType = "text/plain";

            if (context.Request.Path.Value.Equals("/", StringComparison.OrdinalIgnoreCase))
            {
                await EverythingHandlerAsync(context.Response);
            }
            else if (context.Request.Path.Value.Equals("/ERRORS", StringComparison.OrdinalIgnoreCase))
            {
                await ErrorsHandlerAsync(context.Response);
            }
            else
            {
                await ResourceNotFoundHandlerAsync(context.Response);
            }

            Log.Application.Debug("Web request handler completed.");
        }

        private static async Task EverythingHandlerAsync(IOwinResponse response)
        {
            Log.Application.Debug("Everything handler started.");

            var lines = new List<string>();

            using (var stream = new FileStream(@"Logs\Status.log", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream))
                {
                    var line = await reader.ReadLineAsync();

                    while (line != null)
                    {
                        lines.Add(line);

                        line = await reader.ReadLineAsync();
                    }
                }
            }

            lines.Reverse();

            using (var streamWriter = new StreamWriter(response.Body))
            {
                foreach (var line in lines)
                {
                    await streamWriter.WriteLineAsync(line);
                }
            }

            Log.Application.Debug("Everything handler completed.");
        }

        private static async Task ErrorsHandlerAsync(IOwinResponse response)
        {
            Log.Application.Debug("Errors handler started.");

            var lines = new List<string>();

            using (var stream = new FileStream(@"Logs\Status.log", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream))
                {
                    var line = await reader.ReadLineAsync();

                    while (line != null)
                    {
                        if (line.IndexOf("SUCCESS", StringComparison.OrdinalIgnoreCase) == -1)
                        {
                            lines.Add(line);
                        }

                        line = await reader.ReadLineAsync();
                    }
                }
            }

            lines.Reverse();

            using (var streamWriter = new StreamWriter(response.Body))
            {
                foreach (var line in lines)
                {
                    await streamWriter.WriteLineAsync(line);
                }
            }

            Log.Application.Debug("Errors handler completed.");
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

        private static void Callback(object state)
        {
            Log.Application.Debug("Timer callback started.");

            var moment = DateTimeOffset.Now;

            using (var ping = new Ping())
            {
                PingReply pingReply;

                try
                {
                    Log.Application.Info("Ping executing.");

                    pingReply = ping.Send(address, 100, buffer, pingOptions);

                    Log.Application.Info("Ping executed.");
                }
                catch (PingException)
                {
                    Log.Application.Error("Ping failed.");

                    Log.Status.Info("{0:yyyy-MM-dd HH:mm:ss} Error 0", moment);

                    return;
                }

                Log.Status.Info("{0:yyyy-MM-dd HH:mm:ss} {1:F} {2:D}", moment, pingReply.Status, pingReply.RoundtripTime);
            }

            Log.Application.Debug("Timer callback completed.");
        }
    }
}
