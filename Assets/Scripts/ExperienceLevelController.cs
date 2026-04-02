using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceLevelController : MonoBehaviour
{
    public static ExperienceLevelController instance;
    public void Awake()
    {
        instance = this;
    }

    public int currentExperience;
    public ExpPickup pickup;
    public List<int> expLevels;
    public int currentLevel = 1, levelCount = 100;

    void Start()
    {
        while (expLevels.Count < levelCount)
        {
            expLevels.Add(Mathf.CeilToInt(expLevels[expLevels.Count - 1] * 1.1f));
        }
    }

    public void GetExp(int amountToGet)
    {
        currentExperience += amountToGet;
        if (currentExperience >= expLevels[currentLevel])
        {
            LevelUp();
        }
        UIController.instance.UpdateExperience(currentExperience, expLevels[currentLevel], currentLevel);
        SFXManager.instance.PlaySFXPitched(2);
    }

    public void SpawnExp(Vector3 position, int expValue)
    {
        Instantiate(pickup, position, Quaternion.identity).expValue = expValue;
    }

    private void LevelUp()
    {
        currentExperience -= expLevels[currentLevel];
        currentLevel++;
        if (currentLevel >= expLevels.Count)
            currentLevel = expLevels.Count - 1;

        UIController.instance.levelUpPanel.SetActive(true);
        Time.timeScale = 0f;

        BuildClassBasedLevelUpChoices();
        PlayerStatController.instance.UpdateDisplay();
    }

    /// <summary>
    /// Builds the list of upgrade choices for the level-up panel using the class progression system.
    /// Priority order:
    ///   1. If no class yet → offer class selections.
    ///   2. If all weapons of active class are maxed → offer Promotion (if available).
    ///   3. Otherwise → offer weapon upgrades / unlocks for the active class.
    ///   4. Small random chance: also offer a new class slot (2nd / 3rd) mixed in.
    /// </summary>
    private void BuildClassBasedLevelUpChoices()
    {
        var buttons = UIController.instance.levelUpButtons;
        var choices = new List<LevelUpChoice>(); // max 3 choices

        ClassManager cm = ClassManager.instance;

        // --- Case 1: Player has no class yet → show class selection ---
        if (cm.HasNoClass)
        {
            var unlockable = cm.GetUnlockableClasses();
            int count = Mathf.Min(unlockable.Count, buttons.Length);
            for (int i = 0; i < count; i++)
                choices.Add(new LevelUpChoice { type = ChoiceType.SelectClass, classData = unlockable[i] });
        }
        else
        {
            ClassData activeClass = cm.ActiveClass;

            // --- Case 2: All active-class weapons are maxed → offer Promotion ---
            if (cm.AreAllWeaponsMaxedForClass(activeClass) && activeClass.promotionClass != null)
            {
                choices.Add(new LevelUpChoice { type = ChoiceType.Promotion, classData = activeClass });
            }
            else
            {
                // --- Case 3: Offer weapon upgrades for the active class ---
                List<Weapon> available = new List<Weapon>();

                // Already assigned (not maxed) → upgrade
                foreach (var w in PlayerController.instance.assignedWeapons)
                {
                    if (activeClass.classWeapons.Contains(w))
                        available.Add(w);
                }
                // Unassigned weapons belonging to active class → unlock
                foreach (var w in PlayerController.instance.unassignedWeapons)
                {
                    if (activeClass.classWeapons.Contains(w))
                        available.Add(w);
                }

                // Pick up to (buttons.Length - 1) weapon choices to leave room for a new-class offer
                int weaponSlots = Mathf.Min(available.Count, buttons.Length - 1);
                for (int i = 0; i < weaponSlots && available.Count > 0; i++)
                {
                    int idx = Random.Range(0, available.Count);
                    choices.Add(new LevelUpChoice { type = ChoiceType.Weapon, weapon = available[idx] });
                    available.RemoveAt(idx);
                }
            }

            // --- Case 4: Random chance to offer unlocking a new class ---
            // Uses each candidate class's own secondClassUnlockChance for weighted selection.
            if (cm.CanUnlockNewClass && choices.Count < buttons.Length)
            {
                var unlockable = cm.GetUnlockableClasses();
                // Try each unlockable class; add the first one whose roll succeeds.
                foreach (var candidate in unlockable)
                {
                    float roll = Random.value;
                    if (roll <= candidate.secondClassUnlockChance)
                    {
                        choices.Add(new LevelUpChoice { type = ChoiceType.SelectClass, classData = candidate });
                        break; // Offer at most one new-class unlock per level-up
                    }
                }
            }
        }

        // --- Fill any remaining slots with a fallback weapon offer (from any class) ---
        if (choices.Count == 0)
        {
            // Fallback: use old random weapon logic so the panel is never empty
            List<Weapon> fallback = new List<Weapon>();
            fallback.AddRange(PlayerController.instance.assignedWeapons);
            if (PlayerController.instance.assignedWeapons.Count + PlayerController.instance.fullyLevelledWeapons.Count
                < PlayerController.instance.maxWeapons)
                fallback.AddRange(PlayerController.instance.unassignedWeapons);

            int pick = Mathf.Min(fallback.Count, buttons.Length);
            for (int i = 0; i < pick; i++)
            {
                int idx = Random.Range(0, fallback.Count);
                choices.Add(new LevelUpChoice { type = ChoiceType.Weapon, weapon = fallback[idx] });
                fallback.RemoveAt(idx);
            }
        }

        // --- Apply choices to buttons ---
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i < choices.Count)
            {
                buttons[i].gameObject.SetActive(true);
                buttons[i].SetChoice(choices[i]);
            }
            else
            {
                buttons[i].gameObject.SetActive(false);
            }
        }
    }
}

// ---------------------------------------------------------------------------
// Data containers for level-up choices
// ---------------------------------------------------------------------------

public enum ChoiceType
{
    Weapon,      // Upgrade or unlock a weapon
    SelectClass, // Choose a new class
    Promotion    // Promote an existing class to a stronger tier
}

[System.Serializable]
public class LevelUpChoice
{
    public ChoiceType type;
    public Weapon weapon;       // Used when type == Weapon
    public ClassData classData; // Used when type == SelectClass or Promotion
}
