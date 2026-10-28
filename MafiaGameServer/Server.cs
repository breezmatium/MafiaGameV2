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
            var player = new Player(session, $"UnknSlayer_{tcp.GetHashCode()}");

            if (!_lobby.TryAdd(player))
            {
                await session.SendAsync("ERROR lobby_full");
                tcp.Close();
                continue;
            }

            await session.SendAsync("WELCOME");
            await _lobby.BroadcastAsync("LOBBY " + _lobby.Describe());

            _ = Task.Run(async () =>
            {
                try
                {
                    await session.HandleAsync(msg => HandleMessageAsync(player, msg));
                }
                finally
                {
                    _lobby.Remove(player);
                    await _lobby.BroadcastAsync("LOBBY " + _lobby.Describe());

                    Console.WriteLine($"Client disconnected: {player.Name}");
                }
            });
        }
    }

    private async Task HandleMessageAsync(Player player, string line)
    {
        var parts = line.Split(' ', 2);
        string command = parts[0].ToUpperInvariant();
        string argument = parts.Length > 1 ? parts[1].Trim() : string.Empty;

        switch (command)
        {
            case "NAME":
                await HandleNameAsync(player, argument);
                break;
            case "READY":
                await HandleReadyAsync(player);
                break;
            case "CHAT":
                await HandleChatAsync(player, argument);
                break;
            case "START":
                await HandleStartAsync(player);
                break;
            case "":
                break;
            default:
                await player.Session.SendAsync("ERROR unknown_command");
                break;
        }
    }

    private async Task HandleNameAsync(Player player, string name)
    {
        bool valid = name.Length is >= 2 and <= 16 && name.All(c => char.IsLetterOrDigit(c) || c == '_');

        if (!valid)
        {
            await player.Session.SendAsync("ERROR bad_name");
            return;
        }

        if (_lobby.IsNameTaken(name) && !player.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            await player.Session.SendAsync("ERROR name_taken");
            return;
        }
        
        player.Name = name;
        await player.Session.SendAsync("NAME " + name);
        await _lobby.BroadcastAsync("LOBBY " + _lobby.Describe());
    }

    private async Task HandleReadyAsync(Player player)
    {
        player.IsReady = !player.IsReady;
        await _lobby.BroadcastAsync("LOBBY " + _lobby.Describe());
    }

    private async Task HandleChatAsync(Player player, string text)
    {
        if (text.Length == 0) return;
        await _lobby.BroadcastAsync($"CHAT {player.Name}: {text}");
    }

    private async Task HandleStartAsync(Player player)
    {
        if (!_lobby.CanStart)
        {
            await player.Session.SendAsync("ERROR cannot_start");
            return;
        }
        
        await _lobby.BroadcastAsync("STARTED");
    }
}