namespace WinPortProxy
{
    public class ProxyRule
    {
        public string Direction { get; set; }
        public string ListenAddress { get; set; }
        public int ListenPort { get; set; }
        public string ConnectAddress { get; set; }
        public int ConnectPort { get; set; }

        public ProxyRule(string direction, string listenAddress, int listenPort, string connectAddress, int connectPort)
        {
            Direction = direction;
            ListenAddress = listenAddress;
            ListenPort = listenPort;
            ConnectAddress = connectAddress;
            ConnectPort = connectPort;
        }

        public override string ToString()
        {
            return string.Format("{0} listenaddress={1} listenport={2} connectaddress={3}  connectport={4}",
                Direction, ListenAddress, ListenPort, ConnectAddress, ConnectPort);
        }
        public string ToShortString()
        {
            return string.Format("{0} listenaddress={1} listenport={2}",
                Direction, ListenAddress, ListenPort);
        }
    }
}
