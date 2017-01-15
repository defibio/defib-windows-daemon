using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Defib.Scripting
{
    public class Network
    {
        public bool IsServerOnline(string ip)
        {
            Ping ping = new Ping();
            PingReply reply = ping.Send(ip);

            if (reply.Status == IPStatus.Success)
            {
                return true;
            }

            return false;
        }

        public bool IsPortListening(string ip, int port)
        {
            TcpClient client = new TcpClient();

            try
            {
                client.Connect(ip, port);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
