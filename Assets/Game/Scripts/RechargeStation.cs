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

    [SerializeField] private float rechargePerSecond = 25f;
    [SerializeField] private float oneTimeRechargeAmount = 50f;

    [Header("Visuals")]
    [SerializeField] private Color usedColor = Color.gray;

    private GameManager gameManager;
    private SpriteRenderer spriteRenderer;
    private bool used;

    private void Awake()
    {
        gameManager =
            FindAnyObjectByType<GameManager>();

        spriteRenderer =
            GetComponent<SpriteRenderer>();
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
        if (other.CompareTag("Player") &&
            gameManager != null)
        {
            gameManager.SetRecharging(false);
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