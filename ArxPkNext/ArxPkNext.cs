using Poker.Lib.controllers;
using Poker.Lib.Http;
using Poker.Lib.Util;
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace Poker.Arxivar.ServerPlugins
{
    public class NextRest : Abletech.Arxivar.Server.PlugIn.AbstractPlugin
    {
        public override string Name => Config.Instance.Name;
        public override string Version => Config.Instance.Version;
        public string ClientName => Config.Instance.Name;

        private Logger logger;
        private WebServer http;

        public override void Initialize()
        {
            logger = new Logger(Config.Instance.LogPath, Name, Version);
            logger.Truncate();
            logger.WriteLine("Initialize");
        }

        public override void On_Services_Started()
        {
            logger.WriteLine("Start service");

            // Start webserver
            string host = Config.Instance.Retrieve("server", "host");
            int port = Convert.ToInt32(Config.Instance.Retrieve("server", "port"));
            http = new WebServer(host, port);
            logger.WriteLine(string.Format("Listening on port {0} at {1}", port, host));
            http.Start();
            logger.WriteLine("Started http");

            // Status
            http.RegisterRoute("/status", (HttpListenerContext context, GroupCollection RouteVars, MultipartParser multipart) =>
            {
                Response res = new Response(ref context);
                res.SetResponse("It's working!");
                res.SetStatus(true);
                res.Send(HttpStatusCode.OK);
            });

            // Form Submit
            http.RegisterRoute("/apps/bip/create", BipController.Create);

            // Get file list for System ID
            http.RegisterRoute("^/apps/bip/store/(?<SYSTEMID>\\d+-?\\w+)/?$", BipController.List);

            // Get file file list for System ID
            http.RegisterRoute("^/apps/bip/store/(?<SYSTEMID>\\d+-?\\w+)/(?<DOCID>\\d+)/?$", BipController.Download);

            // Get file list for System ID
            http.RegisterRoute("^/apps/bip/seal/(?<SYSTEMID>\\d+-?\\w+)/?$", BipController.Seal);

            // Remove profile by System ID
            http.RegisterRoute("^/apps/bip/destroy/(?<SYSTEMID>\\d+-?\\w+)/?$", BipController.Destroy);
        }

        public override void On_Services_Stopped()
        {
            logger.WriteLine("Stop service");
        }
        public override void Dispose()
        {
            logger.WriteLine("Dispose");
        }
    }
}
