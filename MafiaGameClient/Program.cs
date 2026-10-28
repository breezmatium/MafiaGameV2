namespace MafiaGameClient;

class Program
{
    static async Task Main(string[] args)
    {
        var client = new Client();
        await client.Connect();

        var receiver = Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    string message = await client.ReadAsync();
                    if (message.Length == 0) break;
                    Console.WriteLine(message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Receive stopped: " + e.Message);
            }
        });
        
        while (true)
        {
            string? line = Console.ReadLine();
            if (line == null) break;
            await client.SendAsync(line);
        }

        await client.Disconnect();
        await receiver;
    }
}