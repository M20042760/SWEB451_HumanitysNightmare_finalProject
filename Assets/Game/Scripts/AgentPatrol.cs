using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AgentPatrol : MonoBehaviour
{
    [Header("Patrol Route")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float waypointReachDistance = 0.1f;
    [SerializeField] private bool reverseAtEnd = true;

    [Header("Vision")]
    [SerializeField] private Transform visionArea;
    [SerializeField] private float visionDistance = 1.7f;

    [Header("Visual")]
    [SerializeField] private Sprite agentSideSprite;
    [SerializeField] private Sprite agentDownSprite;
    [SerializeField] private Sprite agentUpSprite;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private int currentWaypoint;
    private int direction = 1;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Transform targetWaypoint = waypoints[currentWaypoint];
        Vector2 currentPosition = body.position;
        Vector2 targetPosition = targetWaypoint.position;
        Vector2 movementDirection = (targetPosition - currentPosition).normalized;

        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            targetPosition,
            speed * Time.fixedDeltaTime
        );

        body.MovePosition(nextPosition);

        UpdateVisionDirection(movementDirection);
        UpdateAgentVisual(movementDirection);

        if (Vector2.Distance(nextPosition, targetPosition) <= waypointReachDistance)
            MoveToNextWaypoint();
    }

    private void MoveToNextWaypoint()
    {
        if (reverseAtEnd)
        {
            currentWaypoint += direction;

            if (currentWaypoint >= waypoints.Length)
            {
                direction = -1;
                currentWaypoint = waypoints.Length - 2;
            }
            else if (currentWaypoint < 0)
            {
                direction = 1;
                currentWaypoint = 1;
            }
        }
        else
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }

    private void UpdateVisionDirection(Vector2 movementDirection)
    {
        if (visionArea == null || movementDirection.sqrMagnitude <= 0.01f)
            return;

        Vector2 patrolDirection = movementDirection.normalized;
        visionArea.position = (Vector2)transform.position + patrolDirection * visionDistance;

        float angle = Mathf.Atan2(patrolDirection.y, patrolDirection.x) * Mathf.Rad2Deg;
        visionArea.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void UpdateAgentVisual(Vector2 movementDirection)
    {
        if (spriteRenderer == null || movementDirection.sqrMagnitude <= 0.01f)
            return;

        if (Mathf.Abs(movementDirection.x) > Mathf.Abs(movementDirection.y))
        {
            spriteRenderer.sprite = agentSideSprite;
            spriteRenderer.flipX = movementDirection.x > 0f;
        }
        else
        {
            spriteRenderer.flipX = false;
            spriteRenderer.sprite = movementDirection.y < 0f
                ? agentDownSprite
                : agentUpSprite;
        }
    }
}