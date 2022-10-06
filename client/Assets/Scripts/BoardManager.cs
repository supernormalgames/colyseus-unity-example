using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using state;
public enum BoardState
{
  Wait = 0,
  Turn = 1
}

public class BoardManager : MonoBehaviour, IPointerClickHandler
{
  public RectTransform backgroundLayer;
  public RectTransform tokenLayer;
  public GameObject backgroundTilePrefab;
  public GameObject tokenPrefab;
  public int tileSize = 16;

  public UnityEvent<Vector2Int> cellSelected;

  BoardState boardState;
  RectTransform rectTransform;
  Vector2Int boardSize;
  int thisPlayerTeam;
  bool mobile = false;

  Dictionary<string, BoardToken> tokens;
  List<GameObject> backgroundTiles = new List<GameObject>();

  void Start()
  {
    rectTransform = GetComponent<RectTransform>();
    SetBoardStateWait();
    mobile = Application.isMobilePlatform;
  }

  public void SetBoardStateTurn()
  {
    boardState = BoardState.Turn;
  }

  public void SetBoardStateWait()
  {
    boardState = BoardState.Wait;
  }

  public void Init(int width, int height, int thisPlayerTeam)
  {
    this.thisPlayerTeam = thisPlayerTeam;
    boardSize = new Vector2Int(width, height);
    Vector2 size = new Vector2(width * tileSize, height * tileSize);
    rectTransform.sizeDelta = size;

    if (mobile)
    {
      rectTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
      rectTransform.anchoredPosition = new Vector2(-size.x * 1.5f / 2, -size.y * 1.5f / 2);
    }
    else
    {
      rectTransform.localScale = new Vector3(1f, 1f, 1f);
      rectTransform.anchoredPosition = new Vector2(-size.x / 2, -size.y / 2);
    }
    

    RenderBackground();
    ClearTokens();
  }

  public void CreateToken(Token token)
  {
    GameObject obj = Instantiate(tokenPrefab, tokenLayer);
    BoardToken boardToken = obj.GetComponent<BoardToken>();

    boardToken.Init(token, tileSize, thisPlayerTeam);
    tokens.Add(boardToken.Id, boardToken);
  }

  public void RemoveToken(string id)
  {
    BoardToken token = tokens[id];

    if (token != null)
    {
      token.Capture();
      tokens.Remove(id);
    }
  }

  private void ClearTokens()
  {
    if (tokens != null)
    {
      foreach (BoardToken token in tokens.Values)
      {
        Destroy(token.gameObject);
      }
    }

    tokens = new Dictionary<string, BoardToken>();
  }

  private void RenderBackground()
  {
    foreach (GameObject tile in backgroundTiles)
    {
      Destroy(tile);
    }
  
    backgroundTiles.Clear();

    for (int x = 0; x < boardSize.x; x++)
    {
      for (int y = 0; y < boardSize.y; y++)
      {
        GameObject bgTile = Instantiate(backgroundTilePrefab, backgroundLayer);
        bgTile.GetComponent<RectTransform>().anchoredPosition = new Vector2(x * tileSize, y * tileSize);
        backgroundTiles.Add(bgTile);
      }
    }
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    if (boardState != BoardState.Turn) return;

    Vector2 localPositon;

    if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPositon))
      return;

    Vector2Int gridPosition = GridPos(localPositon);

    // Debug.Log(localPositon + " : " + gridPosition);

    cellSelected.Invoke(gridPosition);
  }

  private Vector2Int GridPos(Vector2 localPosition)
  {
    return new Vector2Int(Mathf.FloorToInt(localPosition.x / tileSize), Mathf.FloorToInt(localPosition.y / tileSize));
  }
}

