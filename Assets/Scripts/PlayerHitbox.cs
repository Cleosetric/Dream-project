using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public SpriteRenderer sprite;
    public bool isHitEnemy;
    public GameManager.BattleType battleType;
    public GameObject target;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Ignore CircleCollider2D on the player
            if (other is CircleCollider2D)
            {
                return;
            }

            if (other.GetComponentInChildren<EnemyHitbox>())
            {
                sprite.color = Color.red;
                isHitEnemy = true;

                Debug.Log("hit " + other.GetComponentInChildren<EnemyHitbox>().hitboxName);

                if (other.GetComponentInChildren<EnemyHitbox>().hitboxName == "Front")
                {
                    battleType = GameManager.BattleType.Normal;
                    target = other.gameObject;
                }
                else
                {
                    battleType = GameManager.BattleType.Advantage;
                    target = other.gameObject;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Ignore CircleCollider2D on the player
            if (other is CircleCollider2D)
            {
                return;
            }
            sprite.color = Color.white;
            isHitEnemy = false;
            battleType = GameManager.BattleType.Normal;
            target = null;
        }
    }
}
