using System.Net.Sockets;
using System.Text;

namespace MafiaGameServer;

public class ClientSession(TcpClient tcp)
{
    private readonly NetworkStream _stream = tcp.GetStream();
    private readonly List<byte> _pending = new();
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public async Task HandleAsync()
    {
        var buffer = new byte[1024];
        try
        {
            while (true)
            {
                int read = await _stream.ReadAsync(buffer);
                if (read == 0) break;
                
                _pending.AddRange(buffer[..read]);

                while (TryReadText(out string text))
                {
                    Console.WriteLine($"Got: {text}");
                    
                    await SendAsync(text);

                    Console.WriteLine($"Sent: {text}");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
        finally
        {
            tcp.Close();
        }
    }

    public bool TryReadText(out string text)
    {
        int newLine = _pending.IndexOf((byte)'\n');
        if (newLine < 0)
        {
            text = "";
            return false;
        }
        
        var bytes = _pending.ToArray();
        string raw = Encoding.UTF8.GetString(bytes, 0, newLine);
        _pending.RemoveRange(0, newLine + 1);

        text = raw.EndsWith('\r') ? raw[..^1] : raw;
        return true;
    }

    public async Task SendAsync(string text)
    {
        byte[] data = Encoding.UTF8.GetBytes(text + "\n");
        
        await _writeLock.WaitAsync();

        try
        {
            await _stream.WriteAsync(data);
        }
        finally
        {
            _writeLock.Release();
        }
    }
}