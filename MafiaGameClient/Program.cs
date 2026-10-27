using System.Net.Sockets;
using System.Text;

namespace MafiaGameClient;

class Program
{
    static async Task Main(string[] args)
    {
        var client = new Client();
        await client.Connect();
        
        while (true)
        {
            string? line = Console.ReadLine();
            if (line == null) break;

            await client.SendAsync(line);

            string response = await client.ReadAsync();
            Console.WriteLine($"Server: {response}");
        }
    }
}