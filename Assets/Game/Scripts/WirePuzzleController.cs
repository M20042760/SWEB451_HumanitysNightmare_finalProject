using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WirePuzzleController : MonoBehaviour
{
    [Header("Puzzle References")]
    [SerializeField] private WireTile[] terminals;
    [SerializeField] private OfflineAI offlineAI;
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private EndingStoryController endingStoryController;

    [Header("Puzzle UI")]
    [SerializeField] private TMP_Text wiresText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private RectTransform wireLayer;
    [SerializeField] private Image wirePrefab;

    [Header("Puzzle Settings")]
    [SerializeField] private int requiredWires = 3;
    [SerializeField] private float wireThickness = 12f;
    [SerializeField] private float puzzleTime = 30f;

    [Header("Puzzle Timer Sound")]
    [SerializeField] private AudioSource puzzleTimerAudio;
    [SerializeField] private float timerWarningSeconds = 10f;

    private WireTile currentStart;
    private WireTile lastTile;

    private bool drawingWire;
    private bool puzzleActive;
    private bool puzzleComplete;

    private float remainingTime;
    private int connectedWires;

    private readonly List<WireTile> currentPath =
        new List<WireTile>();

    private readonly List<Image> currentSegments =
        new List<Image>();

    private readonly HashSet<WireTile> completedTerminals =
        new HashSet<WireTile>();

    private readonly HashSet<WireTile> occupiedTiles =
        new HashSet<WireTile>();

    private void Awake()
    {
        WireTile[] allTiles =
            GetComponentsInChildren<WireTile>(true);

        foreach (WireTile tile in allTiles)
        {
            tile.SetController(this);
        }

        if (wireLayer != null)
        {
            wireLayer.SetAsLastSibling();
        }

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (puzzleTimerAudio != null)
        {
            puzzleTimerAudio.loop = true;
            puzzleTimerAudio.playOnAwake = false;
        }

        ResetPuzzle();
    }

    private void Update()
    {
        if (!puzzleActive || puzzleComplete)
        {
            return;
        }

        // Unscaled time allows the puzzle timer to run
        // while the puzzle pauses the rest of the game.
        remainingTime -= Time.unscaledDeltaTime;

        UpdateTimerText();
        UpdatePuzzleTimerSound();

        if (remainingTime <= 0f)
        {
            TimeExpired();
        }
    }

    public void OpenPuzzle()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true);
        }

        ResetPuzzle();
        Time.timeScale = 0f;
    }

    public void QuitPuzzle()
    {
        ClearTemporaryWire();
        StopPuzzleTimerSound();

        puzzleActive = false;
        Time.timeScale = 1f;

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }
    }

    public void BeginWire(WireTile tile)
    {
        if (!puzzleActive ||
            puzzleComplete ||
            tile == null ||
            !tile.IsTerminal ||
            completedTerminals.Contains(tile) ||
            occupiedTiles.Contains(tile))
        {
            return;
        }

        ClearTemporaryWire();

        currentStart = tile;
        lastTile = tile;
        drawingWire = true;

        currentPath.Clear();
        currentPath.Add(tile);
    }

    public void EnterTile(WireTile tile)
    {
        if (!drawingWire ||
            tile == null ||
            tile == lastTile)
        {
            return;
        }

        if (occupiedTiles.Contains(tile) ||
            currentPath.Contains(tile))
        {
            return;
        }

        DrawSegment(
            lastTile,
            tile,
            GetWireColor(currentStart.Color));

        currentPath.Add(tile);
        lastTile = tile;
    }

    public void EndWire(WireTile tile)
    {
        if (!drawingWire)
        {
            return;
        }

        drawingWire = false;

        WireTile finalTile = lastTile;

        bool correctEnd =
            finalTile != null &&
            finalTile != currentStart &&
            finalTile.IsTerminal &&
            finalTile.Color == currentStart.Color &&
            !completedTerminals.Contains(finalTile);

        if (correctEnd)
        {
            completedTerminals.Add(currentStart);
            completedTerminals.Add(finalTile);

            foreach (WireTile pathTile in currentPath)
            {
                occupiedTiles.Add(pathTile);
            }

            connectedWires++;

            currentStart = null;
            lastTile = null;
            currentPath.Clear();
            currentSegments.Clear();

            UpdateWiresText();

            if (connectedWires >= requiredWires)
            {
                CompletePuzzle();
            }
        }
        else
        {
            ClearTemporaryWire();
        }
    }

    private void DrawSegment(
        WireTile firstTile,
        WireTile secondTile,
        Color wireColor)
    {
        if (wirePrefab == null ||
            wireLayer == null)
        {
            Debug.LogWarning(
                "Wire Prefab or Wire Layer is missing.");

            return;
        }

        Image wire =
            Instantiate(wirePrefab, wireLayer);

        wire.gameObject.SetActive(true);
        wire.color = wireColor;

        RectTransform firstRect =
            firstTile.GetComponent<RectTransform>();

        RectTransform secondRect =
            secondTile.GetComponent<RectTransform>();

        RectTransform wireRect =
            wire.GetComponent<RectTransform>();

        Vector3 firstPosition =
            firstRect.position;

        Vector3 secondPosition =
            secondRect.position;

        Vector3 direction =
            secondPosition - firstPosition;

        float distance =
            direction.magnitude;

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x) *
            Mathf.Rad2Deg;

        wireRect.position =
            (firstPosition + secondPosition) / 2f;

        wireRect.sizeDelta =
            new Vector2(
                distance,
                wireThickness);

        wireRect.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle);

        wire.transform.SetAsLastSibling();

        currentSegments.Add(wire);
    }

    private void ClearTemporaryWire()
    {
        foreach (Image segment in currentSegments)
        {
            if (segment != null)
            {
                Destroy(segment.gameObject);
            }
        }

        currentSegments.Clear();
        currentPath.Clear();

        currentStart = null;
        lastTile = null;
        drawingWire = false;
    }

    private void ClearAllWires()
    {
        if (wireLayer == null)
        {
            return;
        }

        Image[] wires =
            wireLayer.GetComponentsInChildren<Image>(
                true);

        foreach (Image wire in wires)
        {
            if (wire != wirePrefab)
            {
                Destroy(wire.gameObject);
            }
        }
    }

    public void RetryPuzzle()
    {
        ResetPuzzle();
    }

    public void ResetPuzzle()
    {
        ClearAllWires();
        StopPuzzleTimerSound();

        currentStart = null;
        lastTile = null;
        drawingWire = false;
        puzzleComplete = false;
        puzzleActive = true;

        connectedWires = 0;
        remainingTime = puzzleTime;

        completedTerminals.Clear();
        occupiedTiles.Clear();
        currentPath.Clear();
        currentSegments.Clear();

        UpdateWiresText();
        UpdateTimerText();
    }

    private void UpdatePuzzleTimerSound()
    {
        if (puzzleTimerAudio == null ||
            puzzleTimerAudio.clip == null)
        {
            return;
        }

        bool warning =
            puzzleActive &&
            !puzzleComplete &&
            remainingTime <= timerWarningSeconds &&
            remainingTime > 0f;

        if (warning)
        {
            if (!puzzleTimerAudio.isPlaying)
            {
                puzzleTimerAudio.Play();
            }

            float danger =
                1f - Mathf.Clamp01(
                    remainingTime /
                    timerWarningSeconds);

            puzzleTimerAudio.pitch =
                Mathf.Lerp(1f, 1.5f, danger);
        }
        else
        {
            StopPuzzleTimerSound();
        }
    }

    private void StopPuzzleTimerSound()
    {
        if (puzzleTimerAudio != null)
        {
            puzzleTimerAudio.Stop();
            puzzleTimerAudio.pitch = 1f;
        }
    }

    private void TimeExpired()
    {
        puzzleActive = false;
        drawingWire = false;

        ClearTemporaryWire();
        StopPuzzleTimerSound();

        if (timerText != null)
        {
            timerText.text = "TIME OUT!";
        }
    }

    private void CompletePuzzle()
    {
        puzzleComplete = true;
        puzzleActive = false;

        StopPuzzleTimerSound();

        if (timerText != null)
        {
            timerText.text = "COMPLETE!";
        }

        if (offlineAI != null)
        {
            offlineAI.CompleteReprogramming();
        }

        Time.timeScale = 1f;

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (endingStoryController != null)
        {
            endingStoryController.OpenEndingStory();
        }

        Debug.Log("Wire puzzle complete!");
    }

    private void UpdateWiresText()
    {
        if (wiresText != null)
        {
            wiresText.text =
                $"WIRES: {connectedWires}/{requiredWires}";
        }
    }

    private void UpdateTimerText()
    {
        if (timerText != null)
        {
            timerText.text =
                $"TIME: {Mathf.CeilToInt(remainingTime)}";
        }
    }

    private Color GetWireColor(
        WireTile.WireColor wireColor)
    {
        switch (wireColor)
        {
            case WireTile.WireColor.Red:
                return Color.red;

            case WireTile.WireColor.Green:
                return Color.green;

            case WireTile.WireColor.Yellow:
                return Color.yellow;

            default:
                return Color.white;
        }
    }
}