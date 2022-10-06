using UnityEngine;

public class SoundEffects : GenericSingleton<SoundEffects>
{
  public AudioClip chatMessage;
  public AudioClip message;
  public AudioClip placeToken;
  public AudioClip capture;
  public AudioClip victory;

  private AudioSource audioSource;

  public AudioSource Source
  {
    get { return audioSource; }
  }

  public float Volume
  {
    get { return Source.volume; }
    set 
    { 
      Source.volume = value;
      PlayerPrefs.SetFloat("effectsvolume", value);
    }
  }

  void Start()
  {
    DontDestroyOnLoad(gameObject);
    audioSource = GetComponent<AudioSource>();
    audioSource.volume = PlayerPrefs.GetFloat("effectsvolume", 0.5f);
  }

  public void ChatMessage()
  {
    PlaySound(chatMessage);
  }

  public void Message()
  {
    PlaySound(message);
  }

  public void PlaceToken()
  {
    PlaySound(placeToken, 0.5f);
  }

  public void Capture()
  {
    PlaySound(capture);
  }
  
  public void Victory()
  {
    PlaySound(victory);
  }

  public void PlaySound(AudioClip sound, float volumeScale = 1.0f)
  {
    audioSource.PlayOneShot(sound, volumeScale);
  }
}
