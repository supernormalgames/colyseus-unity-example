using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Text;
using System.Collections.Specialized;
using Colyseus;
using state;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class GameClient : GenericSingleton<GameClient>
{
  public EventHandler<string> OnLeave;
  public EventHandler<string> OnPlayStateChange;
  public EventHandler<string> OnMessage;
  public EventHandler<string> OnJoinCode;
  public EventHandler<int> OnTurnChange;
  public EventHandler<bool> OnResolvingChange;
  public EventHandler<ChatPayload> OnChat;
  public EventHandler<state.Player> OnPlayerAdd;
  public EventHandler<state.Player> OnPlayerRemove;
  public EventHandler<GameState> OnInitialState;
  public EventHandler<GameState> OnStateChange;

  public Player thisPlayer;
  public string joinCode;
  public bool createdPrivate = false;

  protected Client client;
  protected Room<GameState> room;

  string serverUri = "ws://127.0.0.1:2567";

  string roomName = "game";
  string playState = "waiting";
  bool hasInitialState = false;

  public string SavedName
  {
    get { return PlayerPrefs.GetString("savedname", ""); }
    set { PlayerPrefs.SetString("savedname", value); }
  }

  public bool Joined
  {
    get { return room != null; }
  }

  public bool HasInitialState
  {
    get { return hasInitialState; }
  }

  public Room<GameState> Room
  {
    get { return this.room; }
  }

  public string PlayState
  {
    get { return playState; }
  }

  public override void Awake()
  {
    base.Awake();

    QualitySettings.vSyncCount = 0;
    Application.targetFrameRate = 30;

    thisPlayer = null;
    client = ColyseusManager.Instance.CreateClient(serverUri);
  }

  // COMMANDS

  public async Task<bool> FindOrCreateGame()
  {
    if (room != null) return false;

    // server will assign random name if none is provided
    createdPrivate = false;
    room = await client.JoinOrCreate<GameState>(roomName, new Dictionary<string, object> { { "name", SavedName } });

    AttachMessageHandlers();

    return true;
  }

  public async Task<bool> CreateGame()
  {
    if (room != null) return false;

    room = await client.Create<GameState>(roomName, new Dictionary<string, object> { { "private", true }, { "name", SavedName } });

    createdPrivate = true;

    AttachMessageHandlers();

    return true;
  }

  public async Task<bool> JoinGame(string joinCode)
  {
    if (room != null) return false;

    createdPrivate = false;

    NameValueCollection query = new NameValueCollection()
    {
      { "joinCode", joinCode }
    };

    RoomQueryResult roomQueryResult = await Request<RoomQueryResult>("GET", "/roomhelper", query);
    
    if (roomQueryResult.roomId != null && roomQueryResult.roomId != "")
    {
      room = await client.JoinById<GameState>(roomQueryResult.roomId, new Dictionary<string, object> { { "name", SavedName } });
    }
    else
    {
      throw new Exception("couldn't find that game");
    }

    AttachMessageHandlers();

    return true;
  }

  public async Task<bool> Leave()
  {
    if (!Joined) return false;

    await room.Leave();

    return true;
  }

  // GAME MESSAGES

  public async Task<bool> SendTurn(Vector2Int position, int tokenType)
  {
    return await Send(GameProtocol.PLACE_TOKEN, new Turn() { tokenType = tokenType, x = position.x, y = position.y });
  }

  public async Task<bool> SendPass()
  {
    return await Send(GameProtocol.PASS);
  }

  public async Task<bool> SendResign()
  {
    return await Send(GameProtocol.RESIGN);
  }

  public async Task<bool> SendRematch()
  {
    return await Send(GameProtocol.REMATCH);
  }

  public async Task<bool> SendChat(string message)
  {
    return await Send(GameProtocol.CHAT, message);
  }

  // PRIVATE

  async Task<bool> Send(byte type)
  {
    if (room != null && Joined)
    {
      await this.room.Send(type);
      return true;
    }
    else
    {
      return false;
    }
  }

  async Task<bool> Send(byte type, object message)
  {
    if (room != null && Joined)
    {
      await this.room.Send(type, message);
      return true;
    }
    else
    {
      return false;
    }
  }

  void OnDestroy()
  {
    if (room == null) return;

    Room.State.players.OnAdd -= OnPlayerAddHandler;
    Room.State.players.OnRemove -= OnPlayerRemoveHandler;
    Room.OnLeave -= OnLeaveHandler;
    Room.OnStateChange -= OnRoomStateChangeHandler;
  }

  void OnRoomStateChangeDelta(List<Colyseus.Schema.DataChange> changes)
  {
    if (!hasInitialState) return;

    foreach (var change in changes)
    {
      switch (change.Field)
      {
        case "playState":
          OnPlayStateChange?.Invoke(this, (string)change.Value);
          break;
        case "teamTurn":
          OnTurnChange?.Invoke(this, Convert.ToInt16(change.Value));
          break;
        case "resolving":
          OnResolvingChange?.Invoke(this, (bool)change.Value);
          break;
      }
    }
  }

  void OnRoomStateChangeHandler(GameState state, bool isFirstState)
  {
    if (isFirstState)
    {
      thisPlayer = state.players[room.SessionId];
      hasInitialState = true;

      OnInitialState?.Invoke(this, state);

      Room.State.players.OnAdd += OnPlayerAddHandler;
      Room.State.players.OnRemove += OnPlayerRemoveHandler;
    }
    else
    {
      OnStateChange?.Invoke(this, state);
    }
  }

  void OnPlayerAddHandler(state.Player player, string key)
  {
    OnPlayerAdd?.Invoke(this, player);
  }

  private void OnPlayerRemoveHandler(Player player, string key)
  {
    OnPlayerRemove?.Invoke(this, player);
  }

  void OnLeaveHandler(NativeWebSocket.WebSocketCloseCode code)
  {
    room = null;
    hasInitialState = false;

    Debug.Log("Disconnected with code " + code);

    OnLeave?.Invoke(this, code.ToString());

    SceneFader fader = SceneFader.Instance;

    if (fader != null)
    {
      SceneFader.Instance.FadeToScene("Home");
    }
    else
    {
      SceneManager.LoadScene("Home");
    }
  }

  void AttachMessageHandlers()
  {
    Room.OnLeave += OnLeaveHandler;
    Room.OnStateChange += OnRoomStateChangeHandler;
    Room.State.OnChange += OnRoomStateChangeDelta;

    this.room.OnMessage<string>(GameProtocol.MESSAGE, (value) =>
    {
      OnMessage?.Invoke(this, value);
    });

    this.room.OnMessage<ChatPayload>(GameProtocol.CHAT, (value) =>
    {
      OnChat?.Invoke(this, value);
    });

    this.room.OnMessage<int>(GameProtocol.CAPTURE, (value) =>
    {
      SoundEffects.Instance.Capture();
    });

    this.room.OnMessage<string>(GameProtocol.JOIN_CODE, (value) => {
      joinCode = value;
      OnJoinCode?.Invoke(this, value);
    });
  }

  async Task<T> Request<T>(string method, string path, NameValueCollection query = null, object jsonObject = null)
  {
    if (query == null)
    {
      query = HttpUtility.ParseQueryString(string.Empty);
    }

    string endpoint = serverUri.Replace("ws://", "http://").Replace("wss://", "https://");
    var uriBuilder = new UriBuilder(endpoint);
    uriBuilder.Path = path;
    uriBuilder.Query = query.ToQueryString();

    var req = new UnityWebRequest();
    req.method = method;
    req.timeout = 10;
    req.url = uriBuilder.Uri.ToString();

    // Send JSON on request body
    if (jsonObject != null)
    {
      var bodyString = JsonUtility.ToJson(jsonObject);
      req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyString));
      req.SetRequestHeader("Content-Type", "application/json");
    }

    // Request headers
    req.SetRequestHeader("Accept", "application/json");

    req.downloadHandler = new DownloadHandlerBuffer();
    await req.SendWebRequest();

    if (req.result == UnityWebRequest.Result.ConnectionError)
    {
      throw new Exception("connection problem");
    }

    if (req.result == UnityWebRequest.Result.ProtocolError)
    {
      throw new Exception("server error");
    }

    if (req.downloadHandler.text == null)
    {
      return JsonUtility.FromJson<T>("{}");
    }
    else
    {
      var json = req.downloadHandler.text;
      return JsonUtility.FromJson<T>(json);
    }
  }

  [Serializable]
  private class RoomQueryResult
  {
    public string roomId;
  }
}
