using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class OfflineAI : MonoBehaviour
{
    [Header("Puzzle")]
    [SerializeField] private WirePuzzleController puzzleController;

    [Header("AI Visual")]
    [SerializeField] private Sprite onlineSprite;
    [SerializeField] private Color completedColor = Color.green;

    private GameManager gameManager;
    private SpriteRenderer spriteRenderer;

    private bool playerInRange;
    private bool completed;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (completed || !playerInRange || puzzleController == null)
            return;

        if (gameManager != null)
        {
            gameManager.SetRewiringStatus(
                "Press E to reprogram the AI.",
                true
            );
        }

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            puzzleController.OpenPuzzle();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (gameManager != null)
                gameManager.SetRewiringStatus("", false);
        }
    }

    public void CompleteReprogramming()
    {
        completed = true;

        if (spriteRenderer != null)
        {
            if (onlineSprite != null)
            {
                spriteRenderer.sprite = onlineSprite;
                spriteRenderer.color = Color.white;
            }
            else
            {
                spriteRenderer.color = completedColor;
            }
        }

        Collider2D aiCollider = GetComponent<Collider2D>();

        if (aiCollider != null)
            aiCollider.enabled = false;

        if (gameManager != null)
        {
            gameManager.SetRewiringStatus("", false);
            gameManager.CompleteBeta();
        }
    }
}