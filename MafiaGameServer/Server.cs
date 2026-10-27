using System.Net;
using System.Net.Sockets;
using MafiaGameServer.Game;

namespace MafiaGameServer;

class Server(int port)
{
    private readonly TcpListener _listener = new(IPAddress.Any, port);
    private readonly Lobby _lobby = new();

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine($"Server started on port {port}");
        
        while (true)
        {
            var tcp = await _listener.AcceptTcpClientAsync();
            Console.WriteLine("Client connected");
            
            var session = new ClientSession(tcp);
            _ = session.HandleAsync();

            var player = new Player(session, $"UnknownSlayer#{tcp.GetHashCode()}");
            _lobby.AddPlayer(player);

            await session.SendAsync("LOBBY_JOINED");
        }
    }
}