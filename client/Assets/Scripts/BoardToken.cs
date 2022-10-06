using Colyseus.Schema;
using UnityEngine;
using UnityEngine.UI;
using state;
using System.Collections.Generic;
using DG.Tweening;

public static class TokenTypes
{
  public static int NOTUSED_A = 0;
  public static int NOTUSED_B = 1;
  public static int NOTUSED_C = 2;
  public static int BASE = 3;
  public static int UNKNOWN = 4;
}

public class BoardToken : MonoBehaviour
{
  public Sprite[] tokenSprites;

  private Token token = null;
  private int tileSize = 16;
  private int thisPlayerTeam;
  private Image image;
  private RectTransform rectTransform;

  public string Id
  {
    get { return token.id; }
  }

  public void Init(Token token, int tileSize, int thisPlayerTeam)
  {
    this.token = token;
    this.tileSize = tileSize;
    this.thisPlayerTeam = thisPlayerTeam;

    image = GetComponent<Image>();
    rectTransform = GetComponent<RectTransform>();
    image.color = GameScene.Instance.TeamColors[token.team];

    UpdatePosition();
    UpdateSprite();

    this.token.OnChange += OnChange;
  }

  public void Capture()
  {
    image.sprite = tokenSprites[2]; // TODO: rename these

    rectTransform.DOAnchorPos(rectTransform.anchoredPosition, 0.4f).OnComplete(() => {
      Destroy(this.gameObject);
    });
  }

  void OnDestroy()
  {
    this.token.OnChange -= OnChange;
  }

  private void OnChange(List<DataChange> changes)
  {
    UpdatePosition();
    UpdateSprite();
  }

  private void UpdatePosition(bool initial = true)
  {
    Vector2 position = new Vector2(token.x * tileSize, token.y * tileSize);

    if (initial) {
      Vector2 startPosition = position + new Vector2(0, 40);
      rectTransform.anchoredPosition = startPosition;
      rectTransform.DOAnchorPos(position, 0.2f).SetEase(Ease.Linear).OnComplete(() => {
        SoundEffects.Instance.PlaceToken();
      });
    } else {
      rectTransform.anchoredPosition = position;
    }
  }

  private void UpdateSprite()
  {
    image.sprite = tokenSprites[token.tokenType];
  }
}