using Abletech.Arxivar.Client;
using Poker.Lib.Util;
using System;

namespace Poker.Lib.Arxivar.Services
{
    public sealed class WcfClient
    {
        private static WcfClient _instance;
        private static readonly Object _sync = new Object();
        public WCFConnectorManager ConnectionManager;

        private WcfClient()
        {
            string host = Config.Instance.Retrieve("arxivar", "host");
            string port = Config.Instance.Retrieve("arxivar", "port");
            string username = Config.Instance.Retrieve("arxivar", "username");
            string password = Config.Instance.Retrieve("arxivar", "password");
            string connectionString = string.Format("net.tcp://{0}:{1}/Arxivar/Push", host, port);
            ConnectionManager = new WCFConnectorManager(username, password, connectionString, "arxPkRestProfiler");
        }

        public static WcfClient Instance
        {
            get
            {
                if (_instance == null)
                    lock (_sync)
                        if (_instance == null)
                            _instance = new WcfClient();
                return _instance;
            }
        }
    }
}
