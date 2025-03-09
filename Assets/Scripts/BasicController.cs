using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicController : MonoBehaviour
{
    public float speed = 5f;
    public float hitboxOffset = 90f;
    private Animator animator;
    private Rigidbody2D rb;
    private Vector2 moveDirection;

    [SerializeField]
    private GameObject hitbox;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        HandleMovement();
        HandleCombat();
    }

    private void HandleCombat()
    {
        PlayerHitbox hitbox = GetComponentInChildren<PlayerHitbox>();
        if (hitbox)
        {
            if (hitbox.isHitEnemy && hitbox.target)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Battle(hitbox.battleType, hitbox.target);
                }
            }
        }
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
        rb.velocity = speed * moveDirection;

        UpdateHitboxPosition();
    }

    private void UpdateHitboxPosition()
    {
        if (hitbox != null && moveDirection != Vector2.zero)
        {
            if (moveDirection.magnitude > 0.1f) // Only rotate when moving
            {
                float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                hitbox.transform.rotation = Quaternion.Euler(0, 0, angle + hitboxOffset);
            }
        }
    }

    private void Battle(GameManager.BattleType battleType, GameObject encounter)
    {
        if (battleType == GameManager.BattleType.Normal)
        {
            Debug.Log("Normal Battle initiated!");
        }
        else
        {
            Debug.Log("Advantage Battle initiated!");
        }

        GameManager.Instance.ProcessBattle(battleType, encounter);
    }
}
