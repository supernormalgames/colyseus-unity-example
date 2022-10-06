using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneFader : MonoBehaviour
{
  public float duration = 0.4f;
  public float featheredEdgeWidth = 32f;

  private RawImage image;
  private Image featheredEdge;
  private Color invisibleColor = new Color(1f, 1f, 1f, 0f);
  private Color visibleColor = new Color(0f, 0f, 0f, 1f);
  private RectTransform rectTransform;
  private static TransitionType transitionType;
  private static Color wipeColor;
  private static string argument = null;

  public string Argument
  {
    get { return argument; }
  }

  public static SceneFader Instance
  {
    get { return GameObject.FindObjectOfType<SceneFader>(); }
  }

  void Awake()
  {
    image = GetComponent<RawImage>();
    featheredEdge = transform.GetChild(0).GetComponent<Image>();
    image.enabled = true;
    rectTransform = GetComponent<RectTransform>();
  }

  void Start()
  {
    if (transitionType == TransitionType.Jump)
    {
      image.enabled = false;
    }
    else if (transitionType == TransitionType.Wipe)
    {
      WipeReveal(0f);
    }
    else
    {
      FadeReveal(0f);
    }
  }

  public void FadeReveal(float delay = 0f)
  {
    rectTransform.anchoredPosition = new Vector2(0, 0);
    image.DOColor(invisibleColor, duration).SetDelay(delay).SetEase(Ease.Linear).OnComplete(() => {
      image.enabled = false;
      featheredEdge.enabled = false;
    });
  }

  public void WipeReveal(float delay = 0f)
  {
    image.color = wipeColor;
    featheredEdge.color = wipeColor;
    image.enabled = true;
    featheredEdge.enabled = true;
    float width = rectTransform.rect.size.x;
    rectTransform.anchoredPosition = new Vector2(0, 0);
    rectTransform.DOAnchorPos(new Vector2(-(width + featheredEdgeWidth), 0), duration * 1.75f).SetDelay(delay).SetEase(Ease.OutQuad).OnComplete(() => {
      image.enabled = false;
      featheredEdge.enabled = false;
    });
  }

  public void JumpToScene(string sceneName)
  {
    transitionType = TransitionType.Jump;
    SceneManager.LoadScene(sceneName);
  }

  public void FadeToScene(string sceneName, string arg = null)
  {
    transitionType = TransitionType.Fade;
    argument = arg;
    image.color = invisibleColor;
    image.enabled = true;
    rectTransform.anchoredPosition = new Vector2(0, 0);
    image.DOColor(visibleColor, duration).SetEase(Ease.OutQuad).OnComplete(() => SceneManager.LoadScene(sceneName));
  }

  public void WipeToScene(string sceneName, Color color, string arg = null)
  {
    transitionType = TransitionType.Wipe;
    argument = arg;
    wipeColor = color;
    image.color = color;
    featheredEdge.color = color;
    image.enabled = true;
    featheredEdge.enabled = true;
    float width = rectTransform.rect.size.x;
    rectTransform.anchoredPosition = new Vector2(width, 0);
    rectTransform.DOAnchorPos(new Vector2(0, 0), duration * 1.5f).SetEase(Ease.Linear).OnComplete(() => SceneManager.LoadScene(sceneName));
  }
}

public enum TransitionType
{
  Fade,
  Wipe,
  Jump
}
