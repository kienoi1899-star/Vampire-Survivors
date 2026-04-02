using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Class", menuName = "Class Survival/Class Data")]
public class ClassData : ScriptableObject
{
    [Header("Class Info")]
    public string className;
    [TextArea(2, 4)]
    public string classDescription;
    public Sprite classIcon;

    [Header("Weapons")]
    public List<Weapon> classWeapons; // Weapon prefab/GameObjects assigned in scene, set at runtime by ClassManager

    [Header("Promotion")]
    public ClassData promotionClass; // The stronger class this promotes into (null = no promotion available)
    public string promotionDescription;

    [Header("Unlock")]
    [Range(0f, 1f)]
    [Tooltip("Chance that this class will be offered as a 2nd or 3rd class unlock. Used by ExperienceLevelController.")]
    public float secondClassUnlockChance = 0.15f;
}
