using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public string hitboxName; // Assign this in the Inspector (e.g., "Head", "Body", "Leg")

    public void OnHit()
    {
        Debug.Log($"Player hit the {hitboxName}!");
    }
}
