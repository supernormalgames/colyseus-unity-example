using UnityEngine;
using UnityEngine.EventSystems;

public class ModalShade : MonoBehaviour, IPointerDownHandler
{
  void Start()
  {

  }

  void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
  {
    GameObject[] dialogs = GameObject.FindGameObjectsWithTag("Dialog");

    foreach(GameObject dialog in dialogs)
    {
      AnimatedDialog animatedDialog = dialog.GetComponent<AnimatedDialog>();
      if (animatedDialog.allowClose) animatedDialog.Hide();
    }
  }
}
