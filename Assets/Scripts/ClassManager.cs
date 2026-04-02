using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the player's class slots (up to 3), weapon unlocks per class, and promotions.
/// Attach this to the same GameObject as PlayerController or a persistent manager object.
/// </summary>
public class ClassManager : MonoBehaviour
{
    public static ClassManager instance;

    [Header("Available Classes (all possible classes in the game)")]
    public List<ClassData> allClasses; // Assign all ClassData assets here in Inspector

    [Header("Runtime State")]
    public List<ClassData> playerClasses = new List<ClassData>(); // Classes the player has unlocked (max 3)
    public int activeClassIndex = 0; // Which class slot is "active" for weapon selection
    public int maxClasses = 3;

    void Awake()
    {
        instance = this;
    }

    /// <summary>Returns the currently active class, or null if none.</summary>
    public ClassData ActiveClass => (playerClasses.Count > 0 && activeClassIndex < playerClasses.Count)
        ? playerClasses[activeClassIndex]
        : null;

    /// <summary>True if the player has no classes yet.</summary>
    public bool HasNoClass => playerClasses.Count == 0;

    /// <summary>True if player can unlock more classes.</summary>
    public bool CanUnlockNewClass => playerClasses.Count < maxClasses;

    /// <summary>
    /// Adds a class to the player's class list. Also activates the first weapon of that class.
    /// </summary>
    public void UnlockClass(ClassData classData)
    {
        if (playerClasses.Contains(classData)) return;
        if (playerClasses.Count >= maxClasses) return;

        playerClasses.Add(classData);

        // If this is the first class, set it as active
        if (playerClasses.Count == 1)
            activeClassIndex = 0;
    }

    /// <summary>
    /// Returns the list of weapons in a class that have NOT been added to the player yet
    /// and are not at max level (i.e., can still be offered as an upgrade).
    /// </summary>
    public List<Weapon> GetAvailableWeaponsForClass(ClassData classData)
    {
        var available = new List<Weapon>();
        if (classData == null) return available;

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
    /// Returns true if ALL weapons in the class are either fully levelled or not in the game.
    /// </summary>
    public bool AreAllWeaponsMaxedForClass(ClassData classData)
    {
        if (classData == null) return false;
        if (classData.classWeapons.Count == 0) return false;

        foreach (var weapon in classData.classWeapons)
        {
            if (weapon == null) continue;
            bool isMaxed = PlayerController.instance.fullyLevelledWeapons.Contains(weapon);
            if (!isMaxed) return false;
        }
        return true;
    }

    /// <summary>
    /// Promotes the given class slot: replaces it with its promotionClass and removes old weapons.
    /// </summary>
    public void PromoteClass(ClassData classData)
    {
        int idx = playerClasses.IndexOf(classData);
        if (idx < 0 || classData.promotionClass == null) return;

        playerClasses[idx] = classData.promotionClass;
    }

    /// <summary>
    /// Returns classes that can be offered as a new class unlock (not already owned, not a promotion class).
    /// </summary>
    public List<ClassData> GetUnlockableClasses()
    {
        var result = new List<ClassData>();
        foreach (var c in allClasses)
        {
            if (!playerClasses.Contains(c))
                result.Add(c);
        }
        return result;
    }
}
