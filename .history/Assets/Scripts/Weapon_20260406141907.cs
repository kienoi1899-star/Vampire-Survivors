using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public List<WeaponStats> stats;
    public int weaponLevel;

    [HideInInspector]
    public bool statsUpdated;

    public Sprite icon;

    [Tooltip("The class this weapon belongs to (used by the Class Survival progression system).")]
    public ClassData ownerClass;

    [HideInInspector]
    public GameObject weaponPrefab;  // Track prefab này được spawn từ đâu

    public bool IsMaxLevel()
    {
        return stats != null && stats.Count > 0 && weaponLevel >= stats.Count - 1;
    }

    public void LevelUp()
    {
        if (stats == null || stats.Count == 0) return;

        if (weaponLevel < stats.Count - 1)
        {
            weaponLevel++;
            statsUpdated = true;

            if (IsMaxLevel())
            {
                if (PlayerController.instance != null)
                {
                    if (!PlayerController.instance.fullyLevelledWeapons.Contains(this))
                        PlayerController.instance.fullyLevelledWeapons.Add(this);

                    PlayerController.instance.assignedWeapons.Remove(this);
                }
            }
        }
    }
}

[System.Serializable]
public class WeaponStats
{
    public float speed, damage, range, timeBetweenAttacks, amount, duration;
    public string upgradeText;
}