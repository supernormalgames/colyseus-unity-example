using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class MusicPlayer : GenericSingleton<MusicPlayer>
{
  public AudioClip menuTrack;
  public AudioClip gameTrack;

  private AudioSource audioSource;
  private AudioClip currentTrack;

  public bool MusicPref
  {
    get { return PlayerPrefs.GetInt("music_enabled", 1) == 1; }
    set { PlayerPrefs.SetInt("music_enabled", value ? 1 : 0); }
  }

  public float Volume
  {
    get 
    { 
      if (MusicPref) 
      {
        return audioSource.volume; 
      }
      else
      {
        return 0;
      }
    }
    set 
    { 
      audioSource.volume = value;
      PlayerPrefs.SetFloat("musicvolume", value);

      if (value == 0)
      {
        MusicPref = false;
        Stop();
      }
      else if (MusicPref == false)
      {
        MusicPref = true;
        Resume();
      }
    }
  }

  public override void Awake()
  {
    base.Awake();
    DontDestroyOnLoad(gameObject);
    audioSource = GetComponent<AudioSource>();
    audioSource.volume = PlayerPrefs.GetFloat("musicvolume", 0.25f);
  }

  public async void InterruptMusic(int delayMs)
  {
    audioSource.mute = true;
    await Task.Delay(delayMs);
    audioSource.mute = false;
  }

  public void PlayMenuTheme()
  {
    PlayTrack(menuTrack);
  }

  public void PlayGame()
  {
    PlayTrack(gameTrack);
  }

  public void Stop()
  {
    audioSource.Stop();
  }

  public void Resume()
  {
    if (currentTrack != null)
    {
      PlayTrack(currentTrack);
    }
  }

  public void Toggle()
  {
    if (MusicPref)
    {
      MusicPref = false;
      Stop();
    }
    else
    {
      MusicPref = true;
      Resume();

      Volume = 0.25f;
    }
  }

  private void PlayTrack(AudioClip track)
  {
    currentTrack = track;

    if (!MusicPref || (audioSource.isPlaying && audioSource.clip == track)) return;

    audioSource.clip = track;
    audioSource.Play();
  }
}
