using System.Net.Sockets;
using System.Text;

namespace MafiaGameClient;

public class Client
{
    private readonly TcpClient _client = new();
    private NetworkStream _stream;
    private readonly List<byte> _pending = new();

    public async Task Connect()
    {
        await _client.ConnectAsync("127.0.0.1", 5555);
        _stream = _client.GetStream();
    }

    public async Task SendAsync(string text)
    {
        byte[] data = Encoding.UTF8.GetBytes(text + "\n");
        await _stream.WriteAsync(data);
    }

    public async Task<string> ReadAsync()
    {
        var buffer =  new byte[1024];
        while (true)
        {
            int newLine = _pending.IndexOf((byte)'\n');
            if (newLine >= 0)
            {
                var bytes = _pending.ToArray();
                string text = Encoding.UTF8.GetString(bytes, 0, newLine);
                _pending.RemoveRange(0, newLine + 1);
                
                return text.EndsWith('\r') ? text[..^1] : text;
            }
            
            int read = await _stream.ReadAsync(buffer);
            if (read == 0)
            {
                Console.WriteLine("Server closed");
                return string.Empty;
            }
            
            _pending.AddRange(buffer[..read]);
        }
    }
    
    public async Task Disconnect()
    {
        await SendAsync("DISCONNECT");
        await _stream.DisposeAsync();
    }
}