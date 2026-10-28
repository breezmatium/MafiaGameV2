namespace MafiaGameServer.Game;

public class Player(ClientSession session, string name)
{
    public Guid Id { get; } = Guid.NewGuid();
    public ClientSession Session { get; } = session;
    public string Name { get; set; } = name;
    public bool IsReady { get; set; }
}