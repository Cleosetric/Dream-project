using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class Battler
{
    public string characterName;
    public int maxHP,
        currentHP;
    public int maxMP,
        currentMP;
    public int attack,
        defense,
        speed;
    public int spellPower,
        spellCost;
    public bool isDefending;
    public bool isPlayer;
    public Sprite battlerSprite;

    public Battler(CharacterStats stats, bool isPlayer)
    {
        characterName = stats.characterName;
        maxHP = stats.maxHP;
        currentHP = stats.maxHP;
        maxMP = stats.maxMP;
        currentMP = stats.maxMP;
        attack = stats.attack;
        defense = stats.defense;
        speed = stats.speed;
        spellPower = stats.spellPower;
        spellCost = stats.spellCost;
        this.isPlayer = isPlayer;
        isDefending = false;
        battlerSprite = stats.battlerSprite;
    }
}

public class BattleSystem : MonoBehaviour
{
    public List<CharacterStats> playerPartyStats; // ScriptableObjects
    public List<CharacterStats> enemyPartyStats; // ScriptableObjects

    private List<Battler> playerParty = new List<Battler>(); // Now using Battler
    private List<Battler> enemyParty = new List<Battler>();

    public TextMeshProUGUI battleLog;
    public TextMeshProUGUI turn;

    [Space]
    public GameObject gameOverUI;
    public GameObject victoryUI;
    public GameObject playerCommand;

    public Button attackButton,
        spellButton,
        defendButton,
        runButton;

    private List<Battler> turnOrder = new List<Battler>();
    private Battler currentTurnCharacter;
    private int currentTurnIndex = 0;
    private BattlerDisplayManager battlerDisplayManager;

    public enum BattleResult
    {
        None,
        Victory,
        Defeat,
        Run,
    }

    private BattleResult battleResult = BattleResult.None;

    void Start()
    {
        SetupBattle();
    }

    void SetupBattle()
    {
        battlerDisplayManager = BattlerDisplayManager.Instance;

        // Convert CharacterStats to Battler
        foreach (var stats in playerPartyStats)
            playerParty.Add(new Battler(stats, true));

        foreach (var stats in enemyPartyStats)
            enemyParty.Add(new Battler(stats, false));

        // Apply battle type modifiers
        if (GameManager.Instance.currentBattleType == GameManager.BattleType.Advantage)
        {
            battleLog.text = "Advantage Battle Initiated!";
            foreach (var player in playerParty)
                player.speed = Mathf.RoundToInt(player.speed * 1.5f);

            foreach (var enemy in enemyParty)
                enemy.currentHP -= playerParty[0].attack;
        }
        else if (GameManager.Instance.currentBattleType == GameManager.BattleType.Ambushed)
        {
            battleLog.text = "Ambushed Battle Initiated!";
            foreach (var player in playerParty)
                player.speed = Mathf.RoundToInt(player.speed * 0.5f);

            foreach (var player in playerParty)
                player.currentHP -= enemyParty[0].attack;
        }
        else
        {
            battleLog.text = "Normal Battle Initiated!";
        }

        // Combine players and enemies into turn order
        turnOrder.AddRange(playerParty);
        turnOrder.AddRange(enemyParty);
        turnOrder.Sort((a, b) => b.speed.CompareTo(a.speed));

        // Initialize UI
        StatusUIManager.Instance.SetupStatusUI(playerParty, enemyParty);
        BattlerDisplayManager.Instance.SetupBattlers(playerParty, enemyParty);

        StartNextTurn();
    }

    void StartNextTurn()
    {
        if (AllPlayersDefeated())
        {
            EndBattle(BattleResult.Defeat);
            return;
        }
        if (AllEnemiesDefeated())
        {
            EndBattle(BattleResult.Victory);
            return;
        }

        currentTurnCharacter = turnOrder[currentTurnIndex];
        turn.SetText(currentTurnCharacter.characterName + "'s Turn");

        if (currentTurnCharacter.isPlayer)
        {
            currentTurnCharacter.isDefending = false;
            EnablePlayerUI(true);
        }
        else
        {
            bool useSpell = Random.value > 0.5f;

            if (useSpell)
            {
                if (currentTurnCharacter.currentMP >= currentTurnCharacter.spellCost)
                {
                    StartCoroutine(EnemySpell());
                }
                else
                {
                    StartCoroutine(EnemyAttack());
                }
            }
            else
            {
                StartCoroutine(EnemyAttack());
            }
        }
    }

    IEnumerator EnemyAttack()
    {
        battleLog.text = currentTurnCharacter.characterName + " is attacking!";
        yield return new WaitForSeconds(1f);

        // Play attack animation for enemy
        battlerDisplayManager.PlayAttackAnimation(currentTurnCharacter);

        Battler target = playerParty[Random.Range(0, playerParty.Count)];
        if (target.currentHP <= 0)
        {
            target = playerParty.FirstOrDefault(e => e.currentHP > 0);
        }
        int damage = Mathf.Max(
            1,
            currentTurnCharacter.attack - (target.isDefending ? target.defense * 2 : target.defense)
        );
        target.currentHP -= damage;

        // Play damage animation on player
        battlerDisplayManager.PlayDamageAnimation(target);

        battleLog.text =
            target.characterName + " takes " + damage + " damage! HP: " + target.currentHP;

        StatusUIManager.Instance.UpdateStatusUI(target);
        yield return new WaitForSeconds(1f);

        if (AllPlayersDefeated())
        {
            EndBattle(BattleResult.Defeat);
            yield break;
        }

        NextTurn();
    }

    IEnumerator EnemySpell()
    {
        battleLog.text = currentTurnCharacter.characterName + " is attacking!";
        yield return new WaitForSeconds(1f);

        // Play attack animation for enemy
        battlerDisplayManager.PlaySpellAnimation(currentTurnCharacter);

        Battler target = playerParty[Random.Range(0, playerParty.Count)];
        if (target.currentHP <= 0)
        {
            target = playerParty.FirstOrDefault(e => e.currentHP > 0);
        }

        int spellDamage = Mathf.Max(
            1,
            currentTurnCharacter.spellPower
                - (target.isDefending ? target.defense * 2 : target.defense)
        );
        currentTurnCharacter.currentMP -= currentTurnCharacter.spellCost;
        target.currentHP -= spellDamage;

        // Play damage animation on player
        battlerDisplayManager.PlayDamageAnimation(target);

        battleLog.text =
            target.characterName
            + " takes "
            + spellDamage
            + " spell damage! HP: "
            + target.currentHP;

        StatusUIManager.Instance.UpdateStatusUI(target);
        yield return new WaitForSeconds(1f);

        if (AllPlayersDefeated())
        {
            EndBattle(BattleResult.Defeat);
            yield break;
        }

        NextTurn();
    }

    void EnablePlayerUI(bool enable)
    {
        playerCommand.SetActive(enable);
        attackButton.interactable = enable;
        spellButton.interactable = enable;
        defendButton.interactable = enable;
        runButton.interactable = enable;
    }

    public void PlayerAttack()
    {
        EnablePlayerUI(false);

        // Play attack animation
        battlerDisplayManager.PlayAttackAnimation(currentTurnCharacter);

        Battler target = enemyParty[Random.Range(0, enemyParty.Count)];
        if (target.currentHP <= 0)
        {
            target = enemyParty.FirstOrDefault(e => e.currentHP > 0);
        }
        int damage = Mathf.Max(1, currentTurnCharacter.attack - target.defense);
        target.currentHP -= damage;

        // Play damage animation on enemy
        battlerDisplayManager.PlayDamageAnimation(target);

        battleLog.text = currentTurnCharacter.characterName + " attacks " + target.characterName;
        StatusUIManager.Instance.UpdateStatusUI(target);

        if (AllEnemiesDefeated())
        {
            EndBattle(BattleResult.Victory);
        }
        else
        {
            StartCoroutine(NextTurnDelayed());
        }
    }

    public void PlayerSpell()
    {
        if (currentTurnCharacter.currentMP < currentTurnCharacter.spellCost)
        {
            battleLog.text = "Not enough mana!";
            return;
        }

        // Play attack animation
        battlerDisplayManager.PlaySpellAnimation(currentTurnCharacter);

        EnablePlayerUI(false);
        currentTurnCharacter.currentMP -= currentTurnCharacter.spellCost;

        battleLog.text = currentTurnCharacter.characterName + " casts a spell!";

        Battler target = enemyParty[Random.Range(0, enemyParty.Count)];
        if (target.currentHP <= 0)
        {
            target = enemyParty.FirstOrDefault(e => e.currentHP > 0);
        }

        int spellDamage = Mathf.Max(1, currentTurnCharacter.spellPower - target.defense);
        target.currentHP -= spellDamage;

        // Play damage animation on enemy
        battlerDisplayManager.PlayDamageAnimation(target);

        battleLog.text = "Enemy takes " + spellDamage + " damage!";

        StatusUIManager.Instance.UpdateStatusUI(currentTurnCharacter);

        if (AllEnemiesDefeated())
        {
            EndBattle(BattleResult.Victory);
        }
        else
        {
            StartCoroutine(NextTurnDelayed());
        }
    }

    public void PlayerDefend()
    {
        EnablePlayerUI(false);
        currentTurnCharacter.isDefending = true;
        battleLog.text = currentTurnCharacter.characterName + " is defending!";
        // Play attack animation
        battlerDisplayManager.PlayDefenseAnimation(currentTurnCharacter);

        StatusUIManager.Instance.UpdateStatusUI(currentTurnCharacter);

        StartCoroutine(NextTurnDelayed());
    }

    public void PlayerRun()
    {
        EnablePlayerUI(false);
        battleLog.text = "Your party attempts to run!";
        foreach (var player in playerParty)
        {
            // Play attack animation
            battlerDisplayManager.PlayRunAnimation(player);
        }

        EndBattle(BattleResult.Run);
    }

    IEnumerator NextTurnDelayed()
    {
        yield return new WaitForSeconds(1f);
        NextTurn();
    }

    void NextTurn()
    {
        //currentTurnIndex = (currentTurnIndex + 1) % turnOrder.Count;
        do
        {
            currentTurnIndex = (currentTurnIndex + 1) % turnOrder.Count;
        } while (turnOrder[currentTurnIndex].currentHP <= 0); // Skip dead battlers

        StartNextTurn();
    }

    bool AllPlayersDefeated()
    {
        return playerParty.All(p => p.currentHP <= 0);
    }

    bool AllEnemiesDefeated()
    {
        return enemyParty.All(e => e.currentHP <= 0);
    }

    void EndBattle(BattleResult result)
    {
        battleResult = result;
        EnablePlayerUI(false);

        switch (battleResult)
        {
            case BattleResult.Victory:
                battleLog.text = "You won the battle!";
                victoryUI.SetActive(true);
                break;
            case BattleResult.Defeat:
                battleLog.text = "Your party was defeated!";
                gameOverUI.SetActive(true);
                break;
            case BattleResult.Run:
                battleLog.text = "You ran away!";
                EndBattleReturn();
                break;
        }
    }

    public void EndBattleReturn()
    {
        GameManager.Instance.ProcessBattleEnd(battleResult);
    }

    public void EndBattleQuit()
    {
        Application.Quit();
    }
}
