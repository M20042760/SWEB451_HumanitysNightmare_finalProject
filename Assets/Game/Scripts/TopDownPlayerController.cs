using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownPlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 5f;

    [Header("Directional Sprites")]
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        moveInput = Vector2.zero;

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
        {
            moveInput.y += 1f;
        }

        if (Keyboard.current.sKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed)
        {
            moveInput.y -= 1f;
        }

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
        {
            moveInput.x -= 1f;
        }

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
        {
            moveInput.x += 1f;
        }

        moveInput = moveInput.normalized;
        UpdateFacingSprite();
    }

    private void FixedUpdate()
    {
        bool isRunning = Keyboard.current != null &&
                         Keyboard.current.leftShiftKey.isPressed;

        float activeSpeed = isRunning ? runSpeed : walkSpeed;
        body.linearVelocity = moveInput * activeSpeed;
    }

    private void UpdateFacingSprite()
    {
        if (spriteRenderer == null ||
            moveInput.sqrMagnitude < 0.01f)
        {
            return;
        }

        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            spriteRenderer.sprite = moveInput.x > 0f
                ? rightSprite
                : leftSprite;
        }
        else
        {
            spriteRenderer.sprite = moveInput.y > 0f
                ? upSprite
                : downSprite;
        }
    }
}