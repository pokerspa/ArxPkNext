using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Poker.Lib.Util;
using System.Text.RegularExpressions;

namespace Poker.Lib.Http
{
    public delegate void RouteAction(HttpListenerContext context, GroupCollection RouteVars, MultipartParser multipart);

    public class WebServer
    {
        private Thread thread;
        private volatile bool threadActive;

        private HttpListener listener;
        private readonly string ip;
        private readonly int port;

        private Logger logger;
        private Router router;

        public WebServer(string host, int port)
        {
            this.ip = host;
            this.port = port;
        }

        public WebServer(int port)
        {
            this.ip = IPAddress.Loopback.ToString();
            this.port = port;
        }

        public void Start()
        {
            router = new Router();
            logger = new Logger(Config.Instance.LogPath, "HTTP", Config.Instance.Version);
            logger.WriteLine("Startup");

            // Run webserver event loop in another thread
            if (thread != null) throw new Exception("WebServer already active. (Call stop first)");
            thread = new Thread(Listen);
            thread.Start();

            logger.WriteLine("Started");
        }

        public void Stop()
        {
            logger.WriteLine("Stopping");

            // Stop thread and listener
            threadActive = false;
            if (listener != null && listener.IsListening) listener.Stop();

            // Wait for thread to finish
            if (thread != null)
            {
                thread.Join();
                thread = null;
            }

            // Finish closing listener
            if (listener != null)
            {
                listener.Close();
                listener = null;
            }

            logger.WriteLine("Stopped");
        }

        private void Listen()
        {
            logger.WriteLine("Listening");

            threadActive = true;

            // Start listener
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add(string.Format("http://{0}:{1}/", ip, port));
                listener.Start();
            }
            catch (Exception e)
            {
                logger.WriteLine("ERROR", e.Message);
                threadActive = false;
                return;
            }

            // Wait for requests
            while (threadActive)
            {
                try
                {
                    var context = listener.GetContext();
                    if (!threadActive) break;
                    ProcessContext(context);
                }
                catch (HttpListenerException e)
                {
                    if (e.ErrorCode != 995) logger.WriteLine("ERROR", e.Message);
                    threadActive = false;
                }
                catch (Exception e)
                {
                    logger.WriteLine("ERROR", e.Message);
                    threadActive = false;
                }
            }
        }

        private void ProcessContext(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;

            MultipartParser multipart = null;

            if (context.Request.HttpMethod == "POST")
            {
                try
                {
                    multipart = new MultipartParser(context.Request);
                }
                catch (Exception e)
                {
                    logger.WriteLine("Failed to parse multipart");
                    logger.WriteLine("ERROR", e.Message);
                }
            }

            string programFilesX86 = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");

            // Get clean path
            string path = context.Request.Url.AbsolutePath.TrimEnd('/');

            // Resolve path and invoke function
            if (router.Resolve(path, out RouteAction fn, out GroupCollection RouteVars))
            {
                try
                {
                    fn(context, RouteVars, multipart);
                }
                catch (Exception e)
                {
                    Response res = new Response(ref context);
                    res.SetResponse(e.Message);
                    res.SetStatus(false);
                    res.Send(HttpStatusCode.NotFound);
                }
            }

            logger.WriteLine(string.Format("{0} {1} {2}", context.Request.HttpMethod, path, context.Response.StatusCode));
        }

        public void RegisterRoute(string path, RouteAction handler) => router.Add(path, handler);
    }

    internal class Router
    {
        protected Dictionary<string, RouteAction> routes = new Dictionary<string, RouteAction>();

        public void Add(string route, RouteAction handler)
        {
            routes.Add(route, handler);
        }

        public bool Resolve(string route, out RouteAction action, out GroupCollection RouteVars)
        {
            RouteVars = null;

            if (routes.TryGetValue(route, out action))
                return true;

            foreach (var testingRoute in routes)
            {
                Match match = Regex.Match(route, testingRoute.Key);

                if (match.Success)
                {
                    // match.Groups["SYSTEMID"].Value
                    action = testingRoute.Value;
                    RouteVars = match.Groups;
                    return true;
                }
            }

            return false;
        }
    }
}
