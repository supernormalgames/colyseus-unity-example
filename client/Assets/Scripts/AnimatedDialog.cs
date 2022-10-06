using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using TMPro;

public class AnimatedDialog : MonoBehaviour
{
  public Vector2 size = new Vector2(200, 160);
  public bool autoHide = true;
  public bool shaded = false;
  public bool allowClose = false;
  public bool destroyOnClose = false;
  public string closeValue = null;
  public TextMeshProUGUI bodyText = null;
  public AudioClip openSound;

  GameObject contents;
  RectTransform rectTransform;
  Image borderImage;
  Action<string> onClose;

  void Awake()
  {
    contents = transform.Find("Contents").gameObject;
    borderImage = GetComponent<Image>();
    rectTransform = GetComponent<RectTransform>();

    if (autoHide)
    {
      Hide();
    }
  }

  public void HideWithCloseValue()
  {
    Hide(closeValue);
  }

  public void Hide(string value = null)
  {
    borderImage.enabled = false;
    contents.SetActive(false);

    if (shaded)
    {
      SetShadeEnabled(false);
    }

    if (onClose != null)
    {
      onClose(value);
    }

    if (destroyOnClose)
    {
      Destroy(this.gameObject);
    }
  }

  public void ProtectedHide()
  {
    if (allowClose) Hide();
  }

  public void Show(Action opened = null, Action<string> closed = null)
  {
    SoundEffects.Instance.PlaySound(openSound);

    onClose = closed;
    rectTransform.sizeDelta = new Vector2(10, 10);
    borderImage.enabled = true;
    contents.SetActive(false);

    if (shaded)
    {
      SetShadeEnabled(true);
    }

    rectTransform.DOSizeDelta(size, 0.4f).OnComplete(() => {
      contents.SetActive(true);

      if (opened != null) {
        opened();
      }
    });
  }

  private void SetShadeEnabled(bool isEnabled)
  {
    GameObject shade = GameObject.FindGameObjectWithTag("Modal");

    if (shade != null)
    {
      shade.GetComponent<Image>().enabled = isEnabled;
    }
  }
}
