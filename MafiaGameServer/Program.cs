namespace MafiaGameServer;

public class Program
{
    static async Task Main(string[] args)
    {
        var server = new Server(5555);
        await server.StartAsync();
    }
}