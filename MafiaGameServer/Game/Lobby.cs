namespace MafiaGameServer.Game;

public class Lobby
{
    public const int MaxPlayers = 10;
    public const int MinPlayers = 4;

    private readonly object _lock = new();
    private readonly List<Player> _players = new();

    public int Count { get { lock (_lock) return _players.Count; } }
    
    public bool TryAdd(Player player)
    {
        lock (_lock)
        {
            if (_players.Count >= MaxPlayers) return false;
            _players.Add(player);
            return true;
        }
    }

    public void Remove(Player player)
    {
        lock (_lock) _players.Remove(player);
    }

    public bool IsNameTaken(string name)
    {
        lock (_lock)
        {
            return _players.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }

    public Player[] Snapshot()
    {
        lock (_lock) return _players.ToArray();
    }

    public string Describe()
    {
        var players = Snapshot();
        string list = string.Join(", ", players.Select(p => p.IsReady ? p.Name + "*" : p.Name));
        return $"{players.Length}/{MaxPlayers}|{list}";
    }

    public async Task BroadcastAsync(string text)
    {
        foreach (var player in Snapshot())
        {
            try
            {
                await player.Session.SendAsync(text);
            }
            catch
            {
                Remove(player);
            }
        }
    }
    
    public bool CanStart { get {lock (_lock) return _players.Count >= MinPlayers && _players.All(p => p.IsReady); } }
}