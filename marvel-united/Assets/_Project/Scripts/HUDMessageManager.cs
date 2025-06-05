using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDMessageManager : MonoBehaviour
{
    public static HUDMessageManager Instance { get; private set; }

    [Header("HUD Elements")]
    public GameObject container;
    public TextMeshProUGUI messageText;

    [Header("Settings")]
    public float displayDuration = 2f;

    private readonly Queue<string> messageQueue = new();
    private Coroutine displayRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Enqueue(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        messageQueue.Enqueue(message);
        if (displayRoutine == null)
            displayRoutine = StartCoroutine(DisplayMessages());
    }

    private IEnumerator DisplayMessages()
    {
        while (messageQueue.Count > 0)
        {
            string msg = messageQueue.Dequeue();
            if (container != null)
                container.SetActive(true);
            if (messageText != null)
            {
                messageText.text = msg;
                messageText.gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(displayDuration);
            if (messageText != null)
                messageText.gameObject.SetActive(false);
            if (container != null)
                container.SetActive(false);
        }
        displayRoutine = null;
    }

    public IEnumerator ShowAndWait(string message)
    {
        if (string.IsNullOrEmpty(message)) yield break;
        if (container != null)
            container.SetActive(true);
        if (messageText != null)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(displayDuration);
        if (messageText != null)
            messageText.gameObject.SetActive(false);
        if (container != null)
            container.SetActive(false);
    }
}