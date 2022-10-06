using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
[ExecuteInEditMode]
public class PixelPerfectCanvasScaler : MonoBehaviour
{
  public static PixelPerfectCanvasScaler instance;

  public Vector2 landscapeReferenceResolution;
  public Vector2 portraitReferenceResolution;

  private CanvasScaler canvasScaler;
  private int lastScreenWidth = 0;
  private int lastScreenHeight = 0;
  private int scaleFactor = 1;

  public int ScaleFactor
  {
    get { return scaleFactor; }
  }

  void Awake()
  {
    instance = this;
    canvasScaler = GetComponent<CanvasScaler>();
  }

  void Update()
  {
    if (lastScreenWidth != Screen.width || lastScreenHeight != Screen.height)
    {
      lastScreenWidth = Screen.width;
      lastScreenHeight = Screen.height;
    }
    else
    {
      return;
    }

    UpdateScale();
  }

  public void UpdateScale()
  {
    if (lastScreenWidth == 0 || lastScreenHeight == 0) return;

    Vector2 referenceResolution = Screen.width > Screen.height ? landscapeReferenceResolution : portraitReferenceResolution;

    int maxScaleWidth = Mathf.FloorToInt(lastScreenWidth / referenceResolution.x);
    int maxScaleHeight = Mathf.FloorToInt(lastScreenHeight / referenceResolution.y);
    int maxScale = Mathf.Min(maxScaleWidth, maxScaleHeight);

    canvasScaler.scaleFactor = scaleFactor = maxScale;
  }
}
