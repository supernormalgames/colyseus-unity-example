using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class MessageTicker : MonoBehaviour 
{
	public float typeDelay = 0.1f;
	public float delayBetweenMessages = 2.0f;
	public float delayBeforeRepeat = 3.0f;
	public bool repeat;
	
	public TextMeshProUGUI text;

	public UnityEvent messagesFinished;

	private string[] messages;
	private bool typing = false;
	private int currentMessage = 0;
	private int position = 0;
	private Coroutine typingRoutine = null;

	public bool IsTyping
	{
		get { return typing; }
	}

	void Awake () 
	{
		Clear();
	}

	public void Clear()
	{
		text.text = "";
	}

	public void ShowMessage(string message, bool repeat = false)
	{
		ShowMessages(new string[] { message }, repeat);
	}

	public void ShowMessages(string[] messages, bool repeat = false)
	{
		this.repeat = repeat;
		this.messages = messages;

		Reset();
		StartCoroutine(Show());
	}

	public void AppendMessage(string message)
	{
		string[] newMessages = new string[messages.Length + 1];
		for (int i = 0; i < messages.Length; i++)
		{
			newMessages[i] = messages[i];
		}

		newMessages[messages.Length] = message;

		messages = newMessages;
	}

	private void Reset()
	{
		typing = false;
		currentMessage = 0;
		position = 0;
		text.text = "";

		if (typingRoutine != null)
		{
			StopCoroutine(typingRoutine);
		}
	}

	private IEnumerator Show()
	{
		yield return new WaitForSeconds(0.1f);

		RevealCurrent();
	}


	private void RevealCurrent()
	{
		if (currentMessage < 0 || currentMessage >= messages.Length) return;

		RevealMessage(messages[currentMessage]);
	}

	private void RevealMessage(string message)
	{
		if (typeDelay > 0)
		{
			typingRoutine = StartCoroutine(RevealText(message));
		}
		else
		{
			text.text = message;
			StartCoroutine(NextAction());
		}
	}

	public IEnumerator NextAction()
	{
		if (currentMessage < messages.Length - 1) {
			currentMessage ++;

			yield return new WaitForSeconds(delayBetweenMessages);

			RevealCurrent();
		} else if (messages.Length > 1 && repeat) {
			currentMessage = 0;

			yield return new WaitForSeconds(delayBeforeRepeat);

			RevealCurrent();
		} else {
			yield return new WaitForSeconds(delayBetweenMessages);
			messagesFinished?.Invoke();
		}
	}

	private IEnumerator RevealText(string message)
	{
		typing = true;
		position = 0;
		text.text = "";

		while (position < message.Length - 1)
		{
			text.text += message[position];
			position++;

			yield return new WaitForSeconds(typeDelay);
		}

		typing = false;
		text.text = message;

		yield return NextAction();
	}
}
