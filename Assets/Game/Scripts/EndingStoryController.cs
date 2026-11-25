using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingStoryController : MonoBehaviour
{
    [Header("Ending UI")]
    [SerializeField] private TMP_Text storyText;
    [SerializeField] private Button quitButton;

    [Header("Ending Lines")]
    [TextArea(2, 5)]
    [SerializeField] private string[] storyLines;

    [Header("Typing Settings")]
    [SerializeField] private float characterDelay = 0.04f;
    [SerializeField] private float lineDelay = 1.2f;
    [SerializeField] private int soundEveryCharacters = 2;

    [Header("Audio")]
    [SerializeField] private AudioSource typingAudio;
    [SerializeField] private AudioSource endingMusicAudio;

    private Coroutine storyCoroutine;

    private void Awake()
    {
        if (quitButton != null)
        {
            quitButton.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;

        if (quitButton != null)
        {
            quitButton.gameObject.SetActive(false);
        }

        if (endingMusicAudio != null)
        {
            endingMusicAudio.loop = true;
            endingMusicAudio.Play();
        }

        storyCoroutine =
            StartCoroutine(PlayEndingStory());
    }

    private IEnumerator PlayEndingStory()
    {
        if (storyText == null)
        {
            yield break;
        }

        storyText.text = "";

        foreach (string line in storyLines)
        {
            yield return StartCoroutine(
                TypeLine(line));

            yield return new WaitForSecondsRealtime(
                lineDelay);
        }

        if (quitButton != null)
        {
            quitButton.gameObject.SetActive(true);
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

    public void OpenEndingStory()
    {
        Time.timeScale = 0f;
        gameObject.SetActive(true);
    }

    public void QuitGame()
    {
        if (storyCoroutine != null)
        {
            StopCoroutine(storyCoroutine);
        }

        Time.timeScale = 1f;

        if (endingMusicAudio != null)
        {
            endingMusicAudio.Stop();
        }

        Application.Quit();

#if UNITY_EDITOR
        Debug.Log("Quit button pressed.");
#endif
    }
}