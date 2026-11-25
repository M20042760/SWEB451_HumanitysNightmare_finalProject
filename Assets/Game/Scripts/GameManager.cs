using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Battery")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float batteryDrainPerSecond = 1.5f;

    [Header("Capture")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float captureDelay = 5f;
    [SerializeField] private float captureMessageDuration = 2f;

    [Header("HUD")]
    [SerializeField] private TMP_Text batteryText;
    [SerializeField] private TMP_Text statusText;

    [Header("Detection UI")]
    [SerializeField] private Image detectionBorder;
    [SerializeField] private float detectionFadeSpeed = 5f;
    [SerializeField] private float flickerStartDelay = 3f;
    [SerializeField] private float detectedBorderAlpha = 0.65f;
    [SerializeField] private float detectionLingerDuration = 3f;

    private float battery;
    private float detectionTimer;
    private float captureMessageTimer;
    private float detectionLingerTimer;

    private bool detected;
    private bool recharging;
    private bool hidden;
    private bool rewiring;
    private bool gameOver;
    private bool betaCompleted;

    private string rewiringMessage;

    public bool IsHidden => hidden;

    private void Awake()
    {
        battery = maxBattery;

        if (detectionBorder != null)
        {
            Color borderColor = detectionBorder.color;
            borderColor.a = 0f;
            detectionBorder.color = borderColor;
        }

        RefreshUI();
    }

    private void Update()
    {
        UpdateDetectionBorder();

        if (gameOver)
        {
            if (Keyboard.current != null &&
                Keyboard.current.rKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene(
                    SceneManager.GetActiveScene().name);
            }

            return;
        }

        if (betaCompleted)
        {
            return;
        }

        battery = Mathf.Max(
            0f,
            battery - batteryDrainPerSecond * Time.deltaTime);

        if (battery <= 0f)
        {
            gameOver = true;
            RefreshUI();
            return;
        }

        if (detected)
        {
            detectionTimer += Time.deltaTime;

            if (detectionTimer >= captureDelay)
            {
                CapturePlayer();
                return;
            }
        }
        else
        {
            detectionTimer = 0f;
        }

        if (captureMessageTimer > 0f)
        {
            captureMessageTimer -= Time.deltaTime;
        }

        RefreshUI();
    }

    private void UpdateDetectionBorder()
    {
        if (detectionBorder == null)
        {
            return;
        }

        float targetAlpha = 0f;

        if (detected)
        {
            if (detectionTimer < flickerStartDelay)
            {
                // Solid red during the beginning of detection.
                targetAlpha = detectedBorderAlpha;
            }
            else
            {
                // Flicker after the configured delay.
                float flicker = Mathf.Abs(
                    Mathf.Sin(Time.time * 12f));

                targetAlpha =
                    detectedBorderAlpha * flicker;
            }
        }
        else if (detectionLingerTimer > 0f)
        {
            // Slow flicker after leaving the detection area.
            float remaining = Mathf.Clamp01(
                detectionLingerTimer / detectionLingerDuration);

            float slowFlicker = Mathf.Abs(
                Mathf.Sin(Time.time * 3f));

            targetAlpha =
                0.35f * remaining * slowFlicker;

            detectionLingerTimer -= Time.deltaTime;
        }

        Color borderColor = detectionBorder.color;

        borderColor.a = Mathf.MoveTowards(
            borderColor.a,
            targetAlpha,
            detectionFadeSpeed * Time.deltaTime);

        detectionBorder.color = borderColor;
    }

    public void SetDetected(bool value)
    {
        if (value)
        {
            if (!hidden)
            {
                detected = true;
                detectionLingerTimer =
                    detectionLingerDuration;
            }
        }
        else
        {
            if (detected)
            {
                detectionLingerTimer =
                    detectionLingerDuration;
            }

            detected = false;
            detectionTimer = 0f;
        }

        RefreshUI();
    }

    public void SetRecharging(bool value)
    {
        recharging = value;
        RefreshUI();
    }

    public void SetHidden(bool value)
    {
        hidden = value;

        if (hidden)
        {
            if (detected)
            {
                detectionLingerTimer =
                    detectionLingerDuration;
            }

            detected = false;
            detectionTimer = 0f;
        }

        RefreshUI();
    }

    public void SetRewiringStatus(
        string message,
        bool active)
    {
        rewiringMessage = message;
        rewiring = active;
        RefreshUI();
    }

    public void CompleteBeta()
    {
        betaCompleted = true;
        RefreshUI();
    }

    public void Recharge(float amount)
    {
        battery = Mathf.Min(
            maxBattery,
            battery + amount);

        RefreshUI();
    }

  private void CapturePlayer()
{
    detected = false;
    hidden = false;
    recharging = false;
    rewiring = false;

    detectionTimer = 0f;
    detectionLingerTimer = 0f;
    captureMessageTimer = captureMessageDuration;

    if (player != null && spawnPoint != null)
    {
        Rigidbody2D playerBody =
            player.GetComponent<Rigidbody2D>();

        if (playerBody != null)
        {
            playerBody.linearVelocity = Vector2.zero;
            playerBody.angularVelocity = 0f;
            playerBody.position = spawnPoint.position;
        }
        else
        {
            player.position = spawnPoint.position;
        }
    }

    RefreshUI();
}

    private void RefreshUI()
    {
        if (batteryText != null)
        {
            batteryText.text =
                $"BATTERY: {battery:0}%";
        }

        if (statusText == null)
        {
            return;
        }

        if (gameOver)
        {
            statusText.text =
                "SYSTEM SHUTDOWN - Press R to restart.";
        }
        else if (betaCompleted)
        {
            statusText.text =
                "BETA COMPLETE - AI reprogrammed!";
        }
        else if (captureMessageTimer > 0f)
        {
            statusText.text =
                "YOU HAVE BEEN CAUGHT! Returning to checkpoint.";
        }
        else if (detected)
        {
            float remaining = Mathf.Max(
                0f,
                captureDelay - detectionTimer);

            statusText.text =
                $"DETECTED! Escape or hide: {remaining:0.0}s";
        }
        else if (hidden)
        {
            statusText.text =
                "HIDDEN - Agent cannot detect you.";
        }
        else if (recharging)
        {
            statusText.text =
                "RECHARGING...";
        }
        else if (rewiring)
        {
            statusText.text =
                rewiringMessage;
        }
        else
        {
            statusText.text =
                "Objective: Reach the offline AI.";
        }
    }
}