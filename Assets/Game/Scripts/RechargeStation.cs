using UnityEngine;

public class RechargeStation : MonoBehaviour
{
    public enum RechargeMode
    {
        Infinite,
        OneTime
    }

    [Header("Recharge Settings")]
    [SerializeField] private RechargeMode rechargeMode =
        RechargeMode.Infinite;

    [SerializeField] private float rechargePerSecond = 5f;
    [SerializeField] private float oneTimeRechargeAmount = 25f;

    [Header("Visuals")]
    [SerializeField] private Color usedColor = Color.gray;

    [Header("Sound")]
    [SerializeField] private AudioSource rechargeAudio;

    private GameManager gameManager;
    private SpriteRenderer spriteRenderer;
    private bool used;

    private void Awake()
    {
        gameManager =
            FindAnyObjectByType<GameManager>();

        spriteRenderer =
            GetComponent<SpriteRenderer>();

        if (rechargeAudio != null)
        {
            rechargeAudio.loop = true;
            rechargeAudio.playOnAwake = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") ||
            gameManager == null)
        {
            return;
        }

        if (rechargeMode == RechargeMode.OneTime)
        {
            UseOneTimeRecharge();
        }

        if (rechargeAudio != null &&
            rechargeAudio.clip != null &&
            !rechargeAudio.isPlaying)
        {
            rechargeAudio.Play();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player") ||
            gameManager == null)
        {
            return;
        }

        if (rechargeMode == RechargeMode.Infinite)
        {
            gameManager.SetRecharging(true);

            gameManager.Recharge(
                rechargePerSecond * Time.deltaTime);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (gameManager != null)
        {
            gameManager.SetRecharging(false);
        }

        if (rechargeAudio != null)
        {
            rechargeAudio.Stop();
        }
    }

    private void UseOneTimeRecharge()
    {
        if (used)
        {
            return;
        }

        used = true;

        gameManager.Recharge(
            oneTimeRechargeAmount);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = usedColor;
        }
    }
}