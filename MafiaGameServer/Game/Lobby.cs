namespace MafiaGameServer.Game;

public class Lobby
{
    private readonly List<Player> _players = new();
    
    public IReadOnlyList<Player> Players => _players;
    
    public void AddPlayer(Player player) => _players.Add(player);
    
    public void RemovePlayer(Player player) => _players.Remove(player);
    
    public bool IsFull => _players.Count >= 10;
    public bool CanStart => _players.Count >= 4 && _players.All(p => p.IsReady);
}