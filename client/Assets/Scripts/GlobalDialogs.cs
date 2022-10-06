using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class GlobalDialogs : MonoBehaviour
{
  public GameObject alertDialogPrefab;
  public GameObject alertPortraitDialogPrefab;
  public GameObject howToDialogPrefab;
  public GameObject howToPortraitDialogPrefab;

  public static GlobalDialogs Instance
  {
    get { return GameObject.FindObjectOfType<GlobalDialogs>(); }
  }

  void Start()
  {

  }

  public void Alert(string message, Action<string> closed = null)
  {
    AnimatedDialog dialog;

    if (Application.isMobilePlatform)
    {
      dialog = PrepareDialog(alertPortraitDialogPrefab);
    }
    else
    {
      dialog = PrepareDialog(alertDialogPrefab);
    }

    dialog.Show(() => {
      TextMeshProUGUI headline = dialog.bodyText;
      headline.text = message;
    }, closed);
  }

  public void HowToPlay()
  {
    AnimatedDialog dialog;

    if (Application.isMobilePlatform)
    {
      dialog = PrepareDialog(howToPortraitDialogPrefab);
    }
    else
    {
      dialog = PrepareDialog(howToDialogPrefab);
    }

    dialog.Show();
  }

  private AnimatedDialog PrepareDialog(GameObject prefab)
  {
    GameObject attachAfter = GameObject.FindGameObjectWithTag("Modal");
    GameObject dialogObject = Instantiate(prefab, attachAfter.transform.parent);
    dialogObject.transform.SetSiblingIndex(attachAfter.transform.GetSiblingIndex() + 1);

    AnimatedDialog dialog = dialogObject.GetComponent<AnimatedDialog>();
    dialog.allowClose = true;
    dialog.autoHide = false;
    dialog.shaded = true;
    dialog.destroyOnClose = true;

    return dialog;
  }
}
