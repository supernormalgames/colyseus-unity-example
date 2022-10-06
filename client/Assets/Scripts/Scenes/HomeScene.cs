using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HomeScene : MonoBehaviour
{
  public Button[] buttons;
  public GameObject mainOptions;
  public GameObject joinFriend;
  public GameObject pickName;
  public TMP_InputField joinCodeField;
  public TMP_InputField nameField;

  private GameClient client;
  private MusicPlayer musicPlayer;
  private string gameSceneName;

  void Start()
  {
    client = GameClient.Instance;
    Screen.fullScreen = false;
    gameSceneName = Application.isMobilePlatform ? "GamePortrait" : "Game";

    if (client.SavedName != "")
    {
      SetMainOptionVisible(true);
    }
    else
    {
      SetNameVisible(true);
    }
  }

  public void VisitSite()
  {
    Application.OpenURL("https://supernormalgames.com");
  }

  public void SetMainOptionVisible(bool enabled)
  {
    mainOptions.SetActive(enabled);
    joinFriend.SetActive(!enabled);
    pickName.SetActive(false);
  }

  public void SetNameVisible(bool enabled)
  {
    if (enabled)
    {
      pickName.SetActive(true);
      mainOptions.SetActive(false);
      joinFriend.SetActive(false);
    }
    else
    {
      SetMainOptionVisible(true);
    }
  }

  public void SaveName()
  {
    if (nameField.text.Length > 0)
    {
      client.SavedName = nameField.text;
      SetMainOptionVisible(true);
    }
    else
    {
      GlobalDialogs.Instance.Alert("Please pick a name first");
    }
  }

  public async void PlayGame()
  {
    try
    {
      SetLoading(true);

      await client.FindOrCreateGame();

      SceneFader.Instance.FadeToScene(gameSceneName);
    }
    catch(Exception e)
    {
      SetLoading(false);
      GlobalDialogs.Instance.Alert(e.Message);
    }
  }

  public async void CreateGame()
  {
    try
    {
      SetLoading(true);

      await client.CreateGame();

      SceneFader.Instance.FadeToScene(gameSceneName);
    }
    catch(Exception e)
    {
      SetLoading(false);
      GlobalDialogs.Instance.Alert(e.Message);
    }
  }

  public async void JoinGame()
  {
    try
    {
      SetLoading(true);

      await client.JoinGame(joinCodeField.text);

      SceneFader.Instance.FadeToScene(gameSceneName);
    }
    catch(Exception e)
    {
      SetLoading(false);
      GlobalDialogs.Instance.Alert(e.Message);
    }
  }

  public void ShowHowToPlay()
  {
    GlobalDialogs.Instance.HowToPlay();
  }

  void SetLoading(bool isLoading)
  {
    foreach (Button button in buttons)
    {
      button.interactable = !isLoading;
    }
  }
}
