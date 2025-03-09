using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BattlerDisplayManager : MonoBehaviour
{
    public static BattlerDisplayManager Instance;
    public float spacing = 5f;
    public Transform playerBattlerContainer;
    public Transform enemyBattlerContainer;
    public GameObject playerBattlerPrefab;
    public GameObject enemyBattlerPrefab;
    private List<GameObject> playerBattlerObjects = new List<GameObject>();
    private List<GameObject> enemyBattlerObjects = new List<GameObject>();

    private Dictionary<Battler, SpriteRenderer> battlerSprites =
        new Dictionary<Battler, SpriteRenderer>();

    private void Awake()
    {
        Instance = this;
    }

    public void SetupBattlers(List<Battler> playerParty, List<Battler> enemyParty)
    {
        ClearBattlers();

        playerBattlerObjects.Clear();
        enemyBattlerObjects.Clear();

        // Instantiate player battlers
        foreach (var player in playerParty)
        {
            GameObject battlerObj = Instantiate(playerBattlerPrefab, playerBattlerContainer);
            battlerObj.GetComponent<SpriteRenderer>().sprite = player.battlerSprite;
            playerBattlerObjects.Add(battlerObj);
            battlerSprites[player] = battlerObj.GetComponent<SpriteRenderer>();
        }

        // Instantiate enemy battlers
        foreach (var enemy in enemyParty)
        {
            GameObject battlerObj = Instantiate(enemyBattlerPrefab, enemyBattlerContainer);
            battlerObj.GetComponent<SpriteRenderer>().sprite = enemy.battlerSprite;
            enemyBattlerObjects.Add(battlerObj);
            battlerSprites[enemy] = battlerObj.GetComponent<SpriteRenderer>();
        }

        // Position battlers dynamically
        PositionBattlers(playerBattlerObjects, playerBattlerContainer);
        PositionBattlers(enemyBattlerObjects, enemyBattlerContainer);
    }

    private void ClearBattlers()
    {
        // Clear existing battlers
        foreach (var obj in playerBattlerObjects)
            Destroy(obj);
        foreach (var obj in enemyBattlerObjects)
            Destroy(obj);
        battlerSprites.Clear();
    }

    private void PositionBattlers(List<GameObject> battlers, Transform container)
    {
        float totalWidth = (battlers.Count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < battlers.Count; i++)
        {
            Vector3 newPosition = new Vector3(startX + (i * spacing), 0, 0);
            battlers[i].transform.localPosition = newPosition;
        }
    }

    public void PlayAttackAnimation(Battler battler)
    {
        if (battlerSprites.ContainsKey(battler))
        {
            SpriteRenderer sprite = battlerSprites[battler];
            float move;
            if (battler.isPlayer)
            {
                move = -5f;
            }
            else
            {
                move = 5f;
            }
            Vector3 originalPos = sprite.transform.position;
            // Move forward on Z-axis
            sprite
                .transform.DOMoveZ(originalPos.z + move, 0.25f)
                .SetEase(Ease.OutQuad)
                .WaitForCompletion();

            // Return to original position
            sprite
                .transform.DOMoveZ(originalPos.z, 0.25f)
                .SetEase(Ease.InQuad)
                .WaitForCompletion();
        }
    }

    public void PlayDamageAnimation(Battler battler)
    {
        if (battlerSprites.ContainsKey(battler))
        {
            SpriteRenderer sprite = battlerSprites[battler];

            // Damage animation: Flash red
            Color originalColor = sprite.color;
            sprite
                .DOColor(Color.red, 0.1f)
                .OnComplete(() =>
                {
                    sprite.DOColor(originalColor, 0.1f);
                });
        }
    }

    public void PlaySpellAnimation(Battler battler)
    {
        if (battlerSprites.ContainsKey(battler))
        {
            SpriteRenderer sprite = battlerSprites[battler];
            sprite
                .transform.DOShakeScale(0.5f, 0.25f, 10, 90f, false, ShakeRandomnessMode.Full)
                .WaitForCompletion();
        }
    }

    public void PlayDefenseAnimation(Battler battler)
    {
        if (battlerSprites.ContainsKey(battler))
        {
            SpriteRenderer sprite = battlerSprites[battler];
            // Damage animation: Flash red
            Color originalColor = sprite.color;
            sprite
                .DOColor(Color.yellow, 0.3f)
                .OnComplete(() =>
                {
                    sprite.DOColor(originalColor, 0.3f);
                });
        }
    }

    public void PlayRunAnimation(Battler battler)
    {
        if (battlerSprites.ContainsKey(battler))
        {
            SpriteRenderer sprite = battlerSprites[battler];
            Vector3 originalPos = sprite.transform.position;
            // Move forward on Z-axis
            sprite
                .transform.DOMoveZ(originalPos.z + 5f, 0.2f)
                .SetEase(Ease.OutQuad)
                .WaitForCompletion();
        }
    }
}
