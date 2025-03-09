using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float chaseSpeed = 3f;
    public float chaseDuration = 5f;

    private Transform player;
    private Rigidbody2D rb;
    private CircleCollider2D detectionCollider;
    private Animator animator;
    private Coroutine currentCoroutine;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        detectionCollider = GetComponent<CircleCollider2D>();
        animator = GetComponent<Animator>();

        if (detectionCollider == null)
        {
            Debug.LogError("No CircleCollider2D found! Add one to the enemy.");
        }
        else
        {
            detectionCollider.isTrigger = true; // Ensure it's a trigger
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other is not CircleCollider2D)
        {
            player = other.transform;

            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }

            currentCoroutine = StartCoroutine(ChasePlayer());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other is not CircleCollider2D)
        {
            StopChase();
        }
    }

    private IEnumerator ChasePlayer()
    {
        float chaseTimer = chaseDuration;

        while (chaseTimer > 0)
        {
            if (player == null)
            {
                StopChase();
                yield break;
            }

            // Move toward the player
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = direction * chaseSpeed;

            // Update animation direction
            UpdateAnimation(direction);

            chaseTimer -= Time.deltaTime;
            yield return null;
        }

        // If player is still inside the circle, restart chase
        if (player != null)
        {
            currentCoroutine = StartCoroutine(ChasePlayer());
        }
        else
        {
            StopChase();
        }
    }

    private void StopChase()
    {
        rb.velocity = Vector2.zero;
        player = null;

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        animator.SetInteger("Direction", -1); // Reset animation (Idle)
    }

    private void UpdateAnimation(Vector2 moveDir)
    {
        int direction = 0;

        if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y))
        {
            if (moveDir.x > 0)
                direction = 3; // Right
            else
                direction = 2; // Left
        }
        else
        {
            if (moveDir.y > 0)
                direction = 1; // Up
            else
                direction = 0; // Down
        }

        animator.SetInteger("Direction", direction);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Ambush();
        }
    }

    private void Ambush()
    {
        Debug.Log("Ambush triggered!");
        GameManager.Instance.ProcessBattle(GameManager.BattleType.Ambushed, this.gameObject);
    }

    private void OnDrawGizmos()
    {
        if (detectionCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionCollider.radius);
        }
    }
}
