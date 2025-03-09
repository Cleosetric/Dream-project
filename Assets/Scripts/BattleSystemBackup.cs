using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleSystemBackup : MonoBehaviour
{
    public List<CharacterStats> playerParty; // Multiple players in the party
    public List<CharacterStats> enemyParty; // Multiple enemies

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

    private List<CharacterStats> turnOrder = new List<CharacterStats>();
    private CharacterStats currentTurnCharacter;
    private int currentTurnIndex = 0;

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
        // Apply battle type modifiers
        if (GameManager.Instance.currentBattleType == GameManager.BattleType.Advantage)
        {
            battleLog.text = "Advantage Battle Initiated !";
            foreach (var player in playerParty)
                player.speed = Mathf.RoundToInt(player.speed * 1.5f);

            foreach (var enemy in enemyParty)
                enemy.currentHP -= playerParty[0].attack;
        }
        else if (GameManager.Instance.currentBattleType == GameManager.BattleType.Ambushed)
        {
            battleLog.text = "Ambushed Battle Initiated !";
            foreach (var player in playerParty)
                player.speed = Mathf.RoundToInt(player.speed * 0.5f);

            foreach (var player in playerParty)
                player.currentHP -= enemyParty[0].attack;
        }
        else
        {
            battleLog.text = "Normal Battle Initiated !";
        }

        // Combine players and enemies into turn order
        turnOrder.AddRange(playerParty);
        turnOrder.AddRange(enemyParty);
        turnOrder.Sort((a, b) => b.speed.CompareTo(a.speed));

        StartNextTurn();
    }

    void StartNextTurn()
    {
        // Check battle end conditions
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

        // Get the next character's turn
        currentTurnCharacter = turnOrder[currentTurnIndex];
        turn.SetText(currentTurnCharacter.characterName + "'s Turn");

        if (playerParty.Contains(currentTurnCharacter))
        {
            // Reset defend state
            currentTurnCharacter.isDefending = false;
            EnablePlayerUI(true);
        }
        else
        {
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator EnemyTurn()
    {
        battleLog.text = currentTurnCharacter.characterName + " is attacking!";
        yield return new WaitForSeconds(1f);

        // Choose a random target from the player's party
        CharacterStats target = playerParty[Random.Range(0, playerParty.Count)];
        int damage = Mathf.Max(
            1,
            currentTurnCharacter.attack - (target.isDefending ? target.defense * 2 : target.defense)
        );
        target.currentHP -= damage;

        battleLog.text =
            target.characterName + " takes " + damage + " damage! HP: " + target.currentHP;
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

        battleLog.text = currentTurnCharacter.characterName + " attacks!";

        enemyParty[0].currentHP -= Mathf.Max(
            1,
            currentTurnCharacter.attack - enemyParty[0].defense
        );
        battleLog.text = "Enemy HP: " + enemyParty[0].currentHP;

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

        EnablePlayerUI(false);
        currentTurnCharacter.currentMP -= currentTurnCharacter.spellCost;

        battleLog.text = currentTurnCharacter.characterName + " casts a spell!";

        int spellDamage = Mathf.Max(1, currentTurnCharacter.spellPower - enemyParty[0].defense);
        enemyParty[0].currentHP -= spellDamage;

        battleLog.text = "Enemy takes " + spellDamage + " damage!";

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

        StartCoroutine(NextTurnDelayed());
    }

    public void PlayerRun()
    {
        EnablePlayerUI(false);
        battleLog.text = "Your party attempts to run!";

        foreach (var player in playerParty)
        {
            // Run animation
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
        currentTurnIndex = (currentTurnIndex + 1) % turnOrder.Count;
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
                EnablePlayerUI(false);
                break;
            case BattleResult.Defeat:
                battleLog.text = "Your party was defeated!";
                gameOverUI.SetActive(true);
                EnablePlayerUI(false);
                break;
            case BattleResult.Run:
                battleLog.text = "You ran away!";
                break;
        }
    }
}
