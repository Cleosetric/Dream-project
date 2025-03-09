using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusUIManager : MonoBehaviour
{
    public static StatusUIManager Instance;

    public Transform playerStatusContainer;
    public Transform enemyStatusContainer;
    public GameObject statusUIPrefab;

    private Dictionary<Battler, StatusUI> statusUIDict = new Dictionary<Battler, StatusUI>();

    private void Awake()
    {
        Instance = this;
    }

    public void SetupStatusUI(List<Battler> playerParty, List<Battler> enemyParty)
    {
        // Clear old UI
        foreach (Transform child in playerStatusContainer)
            Destroy(child.gameObject);
        foreach (Transform child in enemyStatusContainer)
            Destroy(child.gameObject);

        statusUIDict.Clear();

        // Create UI for each battler in player party
        foreach (var player in playerParty)
        {
            GameObject ui = Instantiate(statusUIPrefab, playerStatusContainer);
            StatusUI statusUI = ui.GetComponent<StatusUI>();
            statusUI.SetBattler(player);
            statusUIDict[player] = statusUI;
        }

        // Create UI for each battler in enemy party
        foreach (var enemy in enemyParty)
        {
            GameObject ui = Instantiate(statusUIPrefab, enemyStatusContainer);
            StatusUI statusUI = ui.GetComponent<StatusUI>();
            statusUI.SetBattler(enemy);
            statusUIDict[enemy] = statusUI;
        }
    }

    public void UpdateStatusUI(Battler battler)
    {
        if (statusUIDict.ContainsKey(battler))
        {
            statusUIDict[battler].UpdateUI();
        }
    }
}
