public class GameProtocol
{
    public const int VERSION = 2;

	public const byte PLACE_TOKEN = 1;
    public const byte MESSAGE = 2;
	public const byte PASS = 3;
	public const byte RESIGN = 4;
	public const byte REMATCH = 5;
	public const byte CHAT = 6;
    public const byte CAPTURE = 7;
    public const byte JOIN_CODE = 8;
}

public class PlayState
{
    public const string WAITING = "waiting";
    public const string PLAYING = "playing";
    public const string ENDGAME = "endgame";
}

public class Turn
{
    public int tokenType;
    public int x;
    public int y;
}

public class ChatPayload
{
    public string from;
    public string message;
}
