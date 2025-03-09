using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI mpText;
    public Slider hpSlider;
    public Slider mpSlider;

    private Battler battler;

    public void SetBattler(Battler newBattler)
    {
        battler = newBattler;
        nameText.text = battler.characterName;
        hpSlider.maxValue = battler.maxHP;
        mpSlider.maxValue = battler.maxMP;
        UpdateUI();
    }

    public void UpdateUI()
    {
        int currentHP = battler.currentHP;
        int currentMP = battler.currentMP;
        if (currentHP <= 0)
        {
            currentHP = 0;
        }
        if (currentMP <= 0)
        {
            currentMP = 0;
        }
        hpText.text = $"{currentHP} / {battler.maxHP}";
        mpText.text = $"{currentMP} / {battler.maxMP}";
        hpSlider.value = currentHP;
        mpSlider.value = currentMP;
    }
}
