namespace MafiaGameServer.Game;

public class Player(ClientSession session, string name)
{
    public ClientSession Session { get; } = session;
    public string Name { get; } = name;
    public bool IsReady { get; }
}