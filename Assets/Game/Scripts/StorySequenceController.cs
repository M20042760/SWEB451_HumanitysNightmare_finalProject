using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StorySequenceController : MonoBehaviour
{
    [Header("Story UI")]
    [SerializeField] private TMP_Text storyText;
    [SerializeField] private Button startButton;

    [Header("Story Lines")]
    [TextArea(2, 5)]
    [SerializeField] private string[] storyLines;

    [Header("Typing Settings")]
    [SerializeField] private float characterDelay = 0.04f;
    [SerializeField] private float lineDelay = 1.2f;
    [SerializeField] private int soundEveryCharacters = 2;

    [Header("Audio")]
    [SerializeField] private AudioSource introMusicAudio;
    [SerializeField] private AudioSource gameplayMusicAudio;
    [SerializeField] private AudioSource typingAudio;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject batteryText;
    [SerializeField] private GameObject statusText;
    [SerializeField] private GameObject detectionBorder;

    private Coroutine storyCoroutine;

    private void Awake()
    {
        Time.timeScale = 0f;

        if (startButton != null)
        {
            startButton.gameObject.SetActive(false);
        }

        if (introMusicAudio != null)
        {
            introMusicAudio.loop = true;
            introMusicAudio.Play();
        }

        if (gameplayMusicAudio != null)
        {
            gameplayMusicAudio.Stop();
        }
    }

    private void OnEnable()
    {
        storyCoroutine = StartCoroutine(PlayStory());
    }

    private IEnumerator PlayStory()
    {
        if (storyText == null)
        {
            yield break;
        }

        storyText.text = "";

        foreach (string line in storyLines)
        {
            yield return StartCoroutine(TypeLine(line));

            yield return new WaitForSecondsRealtime(
                lineDelay);
        }

        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
        }
    }

    private IEnumerator TypeLine(string line)
    {
        storyText.text = "";

        for (int i = 0; i < line.Length; i++)
        {
            storyText.text += line[i];

            if (typingAudio != null &&
                typingAudio.clip != null &&
                !char.IsWhiteSpace(line[i]) &&
                i % soundEveryCharacters == 0)
            {
                typingAudio.PlayOneShot(
                    typingAudio.clip);
            }

            yield return new WaitForSecondsRealtime(
                characterDelay);
        }
    }

    public void StartGame()
    {
        if (storyCoroutine != null)
        {
            StopCoroutine(storyCoroutine);
        }

        Time.timeScale = 1f;

        if (introMusicAudio != null)
        {
            introMusicAudio.Stop();
        }

        if (gameplayMusicAudio != null)
        {
            gameplayMusicAudio.loop = true;
            gameplayMusicAudio.Play();
        }

        if (batteryText != null)
        {
            batteryText.SetActive(true);
        }

        if (statusText != null)
        {
            statusText.SetActive(true);
        }

        if (detectionBorder != null)
        {
            detectionBorder.SetActive(true);
        }

        gameObject.SetActive(false);
    }
}