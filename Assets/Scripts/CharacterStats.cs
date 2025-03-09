using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "CharacterStats", menuName = "Battle System/Character Stats")]
public class CharacterStats : ScriptableObject
{
    public string characterName;
    public int maxHP;
    public int currentHP;
    public int maxMP;
    public int currentMP;
    public int attack;
    public int defense;
    public int speed;
    public int spellPower;
    public int spellCost;
    public bool isDefending;
    public Sprite battlerSprite;
}
