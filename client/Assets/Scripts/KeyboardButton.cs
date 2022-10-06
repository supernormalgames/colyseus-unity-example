using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class KeyboardButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
  public AudioClip press;
  public AudioClip unPress;
  public int pressDistance = 2;

  Vector3 initialPosition;
  Vector3 pressedPosition;
  RectTransform textTransform;
  Button button;
  AudioSource audioSource;

  void Start()
  {
    if (transform.childCount > 0) 
    {
      GameObject child = transform.GetChild(0).gameObject;
      textTransform = child.GetComponent<RectTransform>();
      initialPosition = textTransform.localPosition;
    }

    button = GetComponent<Button>();
  }

  public void OnPointerDown(PointerEventData eventData)
  {
    if (!button.interactable) return;
    
    audioSource = SoundEffects.Instance.Source;
    var pressedPosition = new Vector3(initialPosition.x, initialPosition.y - pressDistance, 0);
    if (textTransform != null) textTransform.localPosition = pressedPosition;

    if (press != null && audioSource != null) audioSource.PlayOneShot(press);
  }

  public void OnPointerUp(PointerEventData eventData)
  {
    if (!button.interactable) return;

    if (textTransform != null) textTransform.localPosition = initialPosition;

    if (unPress != null && audioSource != null) audioSource.PlayOneShot(unPress);
  }
}
