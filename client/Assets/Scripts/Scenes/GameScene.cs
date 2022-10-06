using UnityEngine;
using UnityEngine.UI;
using state;
using System.Collections.Generic;
using TMPro;

public class GameScene : MonoBehaviour
{
  public BoardManager board;
  public MessageTicker messages;
  public GameObject playerItemPrefab;
  public RectTransform playersRectTransform;
  public TextMeshProUGUI chatText;
  public TextMeshProUGUI matchNumberText;
  public TMP_InputField chatInput;

  public Button passButton;
  public Button resignButton;
  public Button rematchButton;
  public Button inviteButton;
  public Button toggleChat;

  public Color[] teamColors;
  public Sprite[] chatToggleSprites;

  private GameClient client;
  private GameState state;
  private Player thisPlayer;
  private int currentTokenToken = 0;
  private List<PlayerItem> playerItems;
  private Queue<ChatMessage> chatMessages;
  private bool hasShownJoinMessage = false;
  private bool chatEnabled = true;

  private static GameScene instance;

  public static GameScene Instance
  {
    get { return instance; }
  }

  public Color[] TeamColors
  {
    get { return teamColors; }
  }

  void Awake()
  {
    instance = this;
  }

  void Start()
  {
    playerItems = new List<PlayerItem>();
    chatMessages = new Queue<ChatMessage>();

    client = GameClient.Instance;

    client.OnInitialState += OnInitialState;
    client.OnLeave += OnLeave;
    client.OnPlayStateChange += OnPlayStateChange;
    client.OnTurnChange += OnTurnChange;
    client.OnResolvingChange += OnResolvingChange;
    client.OnMessage += OnMessage;
    client.OnPlayerAdd += OnPlayerAdd;
    client.OnPlayerRemove += OnPlayerRemove;
    client.OnChat += OnChat;
    client.OnJoinCode += OnJoinCode;

    board.cellSelected.AddListener(CellSelected);
    board.SetBoardStateTurn();

    SetTokenType(TokenTypes.BASE);

    if (client.Room == null)
    {
      _ = client.FindOrCreateGame();
    }
    else if (client.HasInitialState)
    {
      OnInitialState(this, client.Room.State);
    }

    chatText.text = "";
    chatInput.onSubmit.AddListener((value) => {
      if (chatInput.text.Length > 0) _ = client.SendChat(value);
      chatInput.text = "";

      if (!Application.isMobilePlatform)
      {
        chatInput.Select();
        chatInput.ActivateInputField();
      }

      chatInput.placeholder.color = Color.clear;
    });
  }

  void OnDestroy()
  {
    if (client != null)
    {
      client.OnInitialState -= OnInitialState;
      client.OnLeave -= OnLeave;
      client.OnPlayStateChange -= OnPlayStateChange;
      client.OnTurnChange -= OnTurnChange;
      client.OnResolvingChange -= OnResolvingChange;
      client.OnMessage -= OnMessage;
      client.OnPlayerAdd -= OnPlayerAdd;
      client.OnPlayerRemove -= OnPlayerRemove;
    }

    if (state != null)
    {
      this.state.tokens.OnAdd -= OnTokenAddHandler;
      this.state.tokens.OnRemove -= OnTokenRemoveHandler;
    }

    board.cellSelected.RemoveAllListeners();
  } 

  public void Leave()
  {
    _ = client.Leave();
  }

  public void Invite()
  {
    if (Application.isMobilePlatform)
    {
      new NativeShare()
      .SetText("join my nanogo game: " + client.joinCode)
      .SetCallback( ( result, shareTarget ) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
      .Share();
    }
    else
    {
      GUIUtility.systemCopyBuffer = "join my nanogo game: " + client.joinCode;
      GlobalDialogs.Instance.Alert("Invite for match " + client.joinCode + " copied! Send it to a friend.");
    }
  }
  
  public void ShowInviteInstructions()
  {
    GlobalDialogs.Instance.Alert("Use the invite button above to invite a friend to this match.");
  }

  public void ToggleChat()
  {
    chatEnabled = !chatEnabled;

    if (chatEnabled)
    {
      toggleChat.image.sprite = chatToggleSprites[0];
      chatText.GetComponent<CanvasGroup>().alpha = 1;
      chatInput.interactable = true;
    }
    else
    { 
      toggleChat.image.sprite = chatToggleSprites[1];
      chatText.GetComponent<CanvasGroup>().alpha = 0;
      chatInput.interactable = false;
    } 
  }

  public void Pass()
  {
    _ = client.SendPass();
  }

  public void Resign()
  {
    _ = client.SendResign();
  }

  public void Rematch()
  {
    _ = client.SendRematch();
  }

  public void SetTokenType(int tokenType)
  {
    currentTokenToken = tokenType;
  }

  public void Reset()
  {

  }

  bool NagSendInvite
  {
    get { return PlayerPrefs.GetInt("nagsendinvite", 1) == 1; }
    set { PlayerPrefs.SetInt("nagsendinvite", value ? 1 : 0); }
  }

  void SetTurnEnabled(bool enabled)
  {
    passButton.gameObject.SetActive(true);
    resignButton.gameObject.SetActive(true);

    if (enabled)
    {
      messages.ShowMessage("your turn!");
      board.SetBoardStateTurn();
      passButton.interactable = true;
      resignButton.interactable = true;
    }
    else
    {
      passButton.interactable = false;
      resignButton.interactable = false;
      board.SetBoardStateTurn();
    }
  }

  // HANDLERS

  void CellSelected(Vector2Int position)
  {
    _ = client.SendTurn(position, currentTokenToken);
  }

  void OnInitialState(object _, GameState state)
  {
    this.state = state;
    this.thisPlayer = client.thisPlayer;

    board.Init((int)state.boardWidth, (int)state.boardHeight, (int)thisPlayer.team);

    this.state.tokens.ForEach((id, token) => {
      board.CreateToken(token);
    });

    this.state.players.ForEach((id, player) => {
      AddPlayer(player);
    });

    this.state.tokens.OnAdd += OnTokenAddHandler;
    this.state.tokens.OnRemove += OnTokenRemoveHandler;

    UpdateUI();
  }

  void OnPlayStateChange(object _, string playState)
  {
    UpdateUI();
  }

  void OnMessage(object _, string message)
  {
    SoundEffects.Instance.Message();
    AddChatMessage(message);
  }

  void OnTurnChange(object _, int team)
  {
    UpdateUI();
  }

  void OnResolvingChange(object _, bool resolving)
  {
    UpdateUI();
  }

  void OnPlayerAdd(object _, Player player)
  {
    AddPlayer(player);
    AddChatMessage(player.name + " is here!");
  }

  void AddPlayer(Player player)
  {
    GameObject playerItemObject = Instantiate(playerItemPrefab, playersRectTransform);
    PlayerItem playerItem = playerItemObject.GetComponent<PlayerItem>();
    playerItem.Init(player);
    playerItems.Add(playerItem);
  } 

  void OnPlayerRemove(object _, Player player)
  {
    PlayerItem item = playerItems.Find((item) => item.player.sessionId == player.sessionId);

    if (item != null)
    {
      Destroy(item.gameObject);
      playerItems.Remove(item);
    }

    AddChatMessage(player.name + " left the game");
  }

  void OnChat(object _, ChatPayload chat)
  {
    SoundEffects.Instance.ChatMessage();

    Player player = null;
    
    this.state.players.ForEach((key, p) => {
      if (p.sessionId == chat.from) player = p;
    });

    AddChatMessage(chat.message, player);
  }

  void OnJoinCode(object _, string joinCode)
  {
    UpdateUI();
  }

  void OnTokenAddHandler(Token token, string key)
  {
    board.CreateToken(token);
  }

  private void OnTokenRemoveHandler(Token token, string key)
  {
    board.RemoveToken(token.id);
  }

  void OnLeave(object _, string message)
  {
    SceneFader.Instance.FadeToScene("home");
  }

  void AddChatMessage(string message, Player player = null)
  {
    Color color = player != null ? teamColors[player.team] : teamColors[2]; 

    ChatMessage chatMessage = new ChatMessage() {
      message = message,
      color = color
    };

    chatMessages.Enqueue(chatMessage);

    if (chatMessages.Count > 5) chatMessages.Dequeue();

    string allMessages = "";
    ChatMessage[] chatMessageArray = chatMessages.ToArray();

    for (int i = 0; i < chatMessageArray.Length; i++)
    {
      ChatMessage msg = chatMessageArray[i];
      allMessages += "<#" + ColorUtility.ToHtmlStringRGB(msg.color) + ">" + msg.message;

      if (i < chatMessageArray.Length - 1)
      allMessages += "\n";
    }

    chatText.text = allMessages;
  }

  Player OtherPlayer
  {
    get {
      Player other = null;

      this.state.players.ForEach((id, player) => {
        if (player.sessionId != thisPlayer.sessionId)
        {
          other = player;
        }
      });

      return other;
    }
  }

  void UpdateUI()
  {
    matchNumberText.text = "match #" + client.joinCode;

    string playState = state.playState;

    if (!hasShownJoinMessage && client.createdPrivate)
    {
      hasShownJoinMessage = true;

      if (NagSendInvite)
      {
        NagSendInvite = false;
        Invoke("ShowInviteInstructions", 1.1f);
      }
    }

    if (playState == "waiting")
    {
      messages.ShowMessage("waiting for opponent...");
      SetTurnEnabled(false);
      rematchButton.gameObject.SetActive(false);
      inviteButton.interactable = true;
    }
    else if (playState == "playing")
    {
      inviteButton.interactable = false;
      messages.Clear();

      rematchButton.gameObject.SetActive(false);

      if (state.resolving)
      {
        SetTurnEnabled(false);
      }
      else
      {
        SetTurnEnabled(thisPlayer.team == state.teamTurn);

        if (thisPlayer.team != state.teamTurn)
        {
          messages.ShowMessage(OtherPlayer?.name + " is thinking...");
        }
      }
    }
    else if (playState == "endgame")
    {
      SetTurnEnabled(false);

      passButton.gameObject.SetActive(false);
      resignButton.gameObject.SetActive(false);
      rematchButton.gameObject.SetActive(true);
      inviteButton.interactable = false;

      Player winner = null;

      this.state.players.ForEach((id, player) => {
        if (player.winner)
        {
          winner = player;
        }
      });

      if (winner != null)
      {
        SoundEffects.Instance.Victory();
        messages.ShowMessage(winner.name + " wins!");
      }
      else
      {
        messages.ShowMessage("it's a draw.");
      }
    }
  }

  private class ChatMessage
  {
    public string message;
    public Color color;
  }
}
