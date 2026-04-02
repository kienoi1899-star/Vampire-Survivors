using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the player's class slots (up to 3), unlocks, active class selection, and promotions.
/// Attach this to the same GameObject as PlayerController or a persistent manager object.
/// </summary>
public class ClassManager : MonoBehaviour
{
    public static ClassManager instance;

    [Header("Available Classes (all possible classes in the game)")]
    public List<ClassData> allClasses = new List<ClassData>();

    [Header("Runtime State")]
    public List<ClassData> playerClasses = new List<ClassData>();
    public int activeClassIndex = 0;
    public int maxClasses = 3;

    private void Awake()
    {
        instance = this;
    }

    public ClassData ActiveClass
    {
        get
        {
            if (playerClasses.Count == 0) return null;
            if (activeClassIndex < 0 || activeClassIndex >= playerClasses.Count) return null;
            return playerClasses[activeClassIndex];
        }
    }

    public bool HasNoClass => playerClasses.Count == 0;
    public bool CanUnlockNewClass => playerClasses.Count < maxClasses;

    public bool HasClass(ClassData classData)
    {
        return classData != null && playerClasses.Contains(classData);
    }

    public void SetActiveClass(int index)
    {
        if (index < 0 || index >= playerClasses.Count) return;
        activeClassIndex = index;
    }

    public void UnlockClass(ClassData classData)
    {
        if (classData == null) return;
        if (playerClasses.Contains(classData)) return;
        if (playerClasses.Count >= maxClasses) return;

        playerClasses.Add(classData);

        // If this is the first class, make it active
        if (playerClasses.Count == 1)
            activeClassIndex = 0;
    }

    public bool CanPromoteClass(ClassData classData)
    {
        if (classData == null) return false;
        return classData.promotionClass != null;
    }

    /// <summary>
    /// Promotes the given class slot to its promotion class.
    /// This replaces the class in the slot with the promotion class.
    /// </summary>
    public void PromoteClass(ClassData classData)
    {
        if (classData == null) return;
        if (classData.promotionClass == null) return;

        int idx = playerClasses.IndexOf(classData);
        if (idx < 0) return;

        playerClasses[idx] = classData.promotionClass;

        // Keep active index valid
        if (activeClassIndex >= playerClasses.Count)
            activeClassIndex = playerClasses.Count - 1;
    }

    /// <summary>
    /// Returns all classes that can still be unlocked by the player.
    /// </summary>
    public List<ClassData> GetUnlockableClasses()
    {
        var result = new List<ClassData>();

        foreach (var c in allClasses)
        {
            if (c == null) continue;
            if (!playerClasses.Contains(c))
                result.Add(c);
        }

        return result;
    }

    /// <summary>
    /// Returns weapons that belong to the given class and are not fully levelled yet.
    /// </summary>
    public List<Weapon> GetAvailableWeaponsForClass(ClassData classData)
    {
        var available = new List<Weapon>();
        if (classData == null) return available;
        if (PlayerController.instance == null) return available;

        foreach (var weapon in classData.classWeapons)
        {
            if (weapon == null) continue;

            bool isAssigned = PlayerController.instance.assignedWeapons.Contains(weapon);
            bool isMaxed = PlayerController.instance.fullyLevelledWeapons.Contains(weapon);
            bool isUnassigned = PlayerController.instance.unassignedWeapons.Contains(weapon);

            if (!isMaxed && (isAssigned || isUnassigned))
                available.Add(weapon);
        }

        return available;
    }

    /// <summary>
    /// Returns true if all weapons of the class are maxed.
    /// </summary>
    public bool AreAllWeaponsMaxedForClass(ClassData classData)
    {
        if (classData == null) return false;
        if (PlayerController.instance == null) return false;
        if (classData.classWeapons == null || classData.classWeapons.Count == 0) return false;

        foreach (var weapon in classData.classWeapons)
        {
            if (weapon == null) continue;

            bool isMaxed = PlayerController.instance.fullyLevelledWeapons.Contains(weapon);
            if (!isMaxed)
                return false;
        }

        return true;
    }
}