using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float dashSpeed = 10f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1.0f; // Cooldown time before next dash
    public float rayDistance = 1.5f;
    public LayerMask enemyLayer;

    private Animator animator;
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Vector2 rayDirection;
    private bool isDashing = false;
    private bool canDash = true;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!isDashing)
        {
            HandleMovement();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && moveDirection != Vector2.zero && canDash)
        {
            StartCoroutine(AttackDash());
        }

        HandleRaycast();
    }

    private void HandleMovement()
    {
        moveDirection = Vector2.zero;

        if (Input.GetKey(KeyCode.A))
        {
            moveDirection.x = -1;
            animator.SetInteger("Direction", 3);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveDirection.x = 1;
            animator.SetInteger("Direction", 2);
        }

        if (Input.GetKey(KeyCode.W))
        {
            moveDirection.y = 1;
            animator.SetInteger("Direction", 1);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveDirection.y = -1;
            animator.SetInteger("Direction", 0);
        }

        moveDirection.Normalize();
        animator.SetBool("IsMoving", moveDirection.magnitude > 0);
        if (moveDirection != Vector2.zero)
        {
            rayDirection = moveDirection;
        }

        rb.velocity = speed * moveDirection;
    }

    private IEnumerator AttackDash()
    {
        isDashing = true;
        canDash = false; // Prevents spamming
        rb.velocity = moveDirection * dashSpeed;

        float dashTime = 0f;
        while (dashTime < dashDuration)
        {
            dashTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        rb.velocity = moveDirection * speed;

        yield return new WaitForSeconds(dashCooldown); // Cooldown before next dash
        canDash = true;
    }

    private void HandleRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, rayDistance);
        Debug.DrawRay(
            transform.position,
            rayDirection * rayDistance,
            hit.collider != null && hit.collider.CompareTag("Enemy") ? Color.red : Color.green
        );

        if (hit.collider != null && hit.collider.CompareTag("Enemy")) //&&isDashing)
        {
            hit.collider.GetComponentInChildren<EnemyHitbox>().OnHit();
            if (Input.GetKeyDown(KeyCode.E))
                Battle();
        }
    }

    private void Battle()
    {
        Debug.Log("Battle initiated!");
    }
}
