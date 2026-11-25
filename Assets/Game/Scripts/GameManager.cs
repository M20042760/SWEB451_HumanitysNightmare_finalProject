using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Battery")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float batteryDrainPerSecond = 2f;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Detection")]
    [SerializeField] private float captureDelay = 5f;
    [SerializeField] private float captureMessageDuration = 2f;
    [SerializeField] private float flickerStartDelay = 3f;
    [SerializeField] private float detectionLingerDuration = 3f;
    [SerializeField] private float captureCooldown = 1.5f;

    [Header("UI")]
    [SerializeField] private TMP_Text batteryText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image detectionBorder;
    [SerializeField] private float detectionFadeSpeed = 5f;

    [Header("Detection Sound")]
    [SerializeField] private AudioSource detectionWarningAudio;

    private float battery;
    private float detectionTimer;
    private float captureMessageTimer;
    private float detectionLingerTimer;
    private float captureCooldownTimer;

    private bool detected;
    private bool recharging;
    private bool hidden;
    private bool rewiring;
    private bool gameOver;
    private bool betaCompleted;

    private string rewiringMessage = "";

    public bool IsHidden => hidden;

    private void Awake()
    {
        battery = maxBattery;

        if (detectionBorder != null)
        {
            Color color = detectionBorder.color;
            color.a = 0f;
            detectionBorder.color = color;
        }

        if (detectionWarningAudio != null)
        {
            detectionWarningAudio.loop = true;
            detectionWarningAudio.playOnAwake = false;
        }

        RefreshUI();
    }

    private void Update()
    {
        UpdateDetectionBorder();
        UpdateDetectionSound();

        if (captureCooldownTimer > 0f)
        {
            captureCooldownTimer -= Time.deltaTime;
        }

        if (gameOver)
        {
            if (Keyboard.current != null &&
                Keyboard.current.rKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene(
                    SceneManager.GetActiveScene().buildIndex);
            }

            return;
        }

        if (betaCompleted)
        {
            return;
        }

        if (!recharging)
        {
            battery -=
                batteryDrainPerSecond *
                Time.deltaTime;

            battery = Mathf.Clamp(
                battery,
                0f,
                maxBattery);
        }

        // Press R happens only when the battery reaches zero.
        if (battery <= 0f)
        {
            battery = 0f;
            gameOver = true;

            StopDetectionSound();
            RefreshUI();

            return;
        }

        if (detected &&
            !hidden &&
            captureCooldownTimer <= 0f)
        {
            detectionTimer += Time.deltaTime;

            if (detectionTimer >= captureDelay)
            {
                CapturePlayer();
                return;
            }
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

        if (detected && !hidden)
        {
            if (detectionTimer < flickerStartDelay)
            {
                targetAlpha = 0.65f;
            }
            else
            {
                float danger = Mathf.Clamp01(
                    detectionTimer /
                    captureDelay);

                float flickerSpeed = Mathf.Lerp(
                    4f,
                    18f,
                    danger);

                float flicker = Mathf.Abs(
                    Mathf.Sin(
                        Time.time *
                        flickerSpeed));

                targetAlpha = Mathf.Lerp(
                    0.2f,
                    0.85f,
                    danger) * flicker;
            }
        }
        else if (detectionLingerTimer > 0f)
        {
            float lingerProgress = Mathf.Clamp01(
                detectionLingerTimer /
                detectionLingerDuration);

            float flickerSpeed = Mathf.Lerp(
                3f,
                10f,
                1f - lingerProgress);

            float flicker = Mathf.Abs(
                Mathf.Sin(
                    Time.time *
                    flickerSpeed));

            targetAlpha =
                lingerProgress *
                0.65f *
                flicker;
        }

        Color borderColor =
            detectionBorder.color;

        borderColor.a = Mathf.MoveTowards(
            borderColor.a,
            targetAlpha,
            detectionFadeSpeed *
            Time.deltaTime);

        detectionBorder.color =
            borderColor;

        if (detectionLingerTimer > 0f)
        {
            detectionLingerTimer -=
                Time.deltaTime;
        }
    }

    private void UpdateDetectionSound()
    {
        if (detectionWarningAudio == null ||
            detectionWarningAudio.clip == null)
        {
            return;
        }

        bool shouldPlay =
            detected &&
            !hidden &&
            !gameOver &&
            !betaCompleted &&
            captureCooldownTimer <= 0f;

        if (shouldPlay)
        {
            if (!detectionWarningAudio.isPlaying)
            {
                detectionWarningAudio.Play();
            }

            float danger = Mathf.Clamp01(
                detectionTimer /
                captureDelay);

            detectionWarningAudio.pitch =
                Mathf.Lerp(
                    1f,
                    1.8f,
                    danger);
        }
        else
        {
            StopDetectionSound();
        }
    }

    private void StopDetectionSound()
    {
        if (detectionWarningAudio != null)
        {
            detectionWarningAudio.Stop();
            detectionWarningAudio.pitch = 1f;
        }
    }

    public void SetDetected(bool value)
    {
        // Ignore detection briefly after respawning.
        if (value &&
            captureCooldownTimer > 0f)
        {
            return;
        }

        if (value)
        {
            // Reset only when detection begins.
            if (!detected)
            {
                detectionTimer = 0f;
            }

            detected = true;
            hidden = false;
            detectionLingerTimer = 0f;
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
    }

    public void SetHidden(bool value)
    {
        hidden = value;

        if (hidden)
        {
            detected = false;
            detectionTimer = 0f;
            detectionLingerTimer =
                detectionLingerDuration;

            StopDetectionSound();
        }
    }

    public void SetRecharging(bool value)
    {
        recharging = value;
    }

    public void SetRewiringStatus(
        string message,
        bool active)
    {
        rewiring = active;
        rewiringMessage = message;
    }

    public void Recharge(float amount)
    {
        battery += amount;

        battery = Mathf.Clamp(
            battery,
            0f,
            maxBattery);
    }

    public void CompleteBeta()
    {
        betaCompleted = true;
        detected = false;
        hidden = false;
        rewiring = false;

        StopDetectionSound();
        RefreshUI();
    }

    private void CapturePlayer()
    {
        // Reset detection only.
        // The battery remains unchanged.
        detected = false;
        hidden = false;
        recharging = false;

        detectionTimer = 0f;
        detectionLingerTimer = 0f;
        captureMessageTimer =
            captureMessageDuration;

        captureCooldownTimer =
            captureCooldown;

        StopDetectionSound();

        if (player == null ||
            playerSpawnPoint == null)
        {
            Debug.LogWarning(
                "Assign Player and Player Spawn Point " +
                "in the GameManager Inspector.");

            RefreshUI();
            return;
        }

        Rigidbody2D body =
            player.GetComponent<Rigidbody2D>();

        if (body != null)
        {
            body.position =
                playerSpawnPoint.position;

            body.linearVelocity =
                Vector2.zero;

            body.angularVelocity = 0f;
        }
        else
        {
            player.position =
                playerSpawnPoint.position;
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (batteryText != null)
        {
            batteryText.text =
                $"BATTERY: {Mathf.CeilToInt(battery)}%";
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
                "REPROGRAMMING COMPLETE!\n" +
                "You freed your friend.";
        }
        else if (captureMessageTimer > 0f)
        {
            statusText.text =
                "YOU HAVE BEEN CAUGHT!\n" +
                "Returning to checkpoint.";
        }
        else if (detected && !hidden)
        {
            float remaining =
                Mathf.Max(
                    0f,
                    captureDelay -
                    detectionTimer);

            statusText.text =
                $"DETECTED! Escape or hide: " +
                $"{remaining:0.0}s";
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
                "OBJECTIVE:\n" +
                "Reach the offline AI.\n" +
                "Manage your battery.\n" +
                "Avoid the agents.\n" +
                "Do not get caught.";
        }
    }
}