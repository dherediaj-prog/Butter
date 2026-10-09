using System.Net;
using System.Net.Sockets;

namespace Engine.Services.Network;

public static class IPService
{
    public static string GetLocalIPAddress()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        socket.Connect("8.8.8.8", 65530);
        if (socket.LocalEndPoint is IPEndPoint endPoint)
        {
            return endPoint.Address.ToString();
        }

        return "127.0.0.1";
    }
}