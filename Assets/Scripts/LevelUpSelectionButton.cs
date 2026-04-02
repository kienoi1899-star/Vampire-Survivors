using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class LevelUpSelectionButton : MonoBehaviour
{
    public TMP_Text upgradeDescText, nameLevelText;
    public Image weaponIcon;

    private LevelUpChoice currentChoice;

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>Configure this button to display the given choice.</summary>
    public void SetChoice(LevelUpChoice choice)
    {
        currentChoice = choice;

        switch (choice.type)
        {
            case ChoiceType.Weapon:
                DisplayWeaponChoice(choice.weapon);
                break;
            case ChoiceType.SelectClass:
                DisplayClassChoice(choice.classData);
                break;
            case ChoiceType.Promotion:
                DisplayPromotionChoice(choice.classData);
                break;
        }
    }

    // Keep old method for backward compatibility with any leftover calls
    public void UpdateButtonDisplay(Weapon theWeapon)
    {
        currentChoice = new LevelUpChoice { type = ChoiceType.Weapon, weapon = theWeapon };
        DisplayWeaponChoice(theWeapon);
    }

    // -----------------------------------------------------------------------
    // Display helpers
    // -----------------------------------------------------------------------

    private void DisplayWeaponChoice(Weapon weapon)
    {
        if (weapon == null) return;

        if (weapon.gameObject.activeSelf)
        {
            upgradeDescText.text = weapon.stats[weapon.weaponLevel].upgradeText;
            nameLevelText.text = weapon.name + " - Lvl " + weapon.weaponLevel;
        }
        else
        {
            upgradeDescText.text = "Unlock " + weapon.name;
            nameLevelText.text = weapon.name;
        }
        if (weaponIcon != null) weaponIcon.sprite = weapon.icon;
    }

    private void DisplayClassChoice(ClassData classData)
    {
        if (classData == null) return;

        nameLevelText.text = "Class: " + classData.className;
        upgradeDescText.text = string.IsNullOrEmpty(classData.classDescription)
            ? "Unlock the " + classData.className + " class."
            : classData.classDescription;
        if (weaponIcon != null && classData.classIcon != null)
            weaponIcon.sprite = classData.classIcon;
    }

    private void DisplayPromotionChoice(ClassData classData)
    {
        if (classData == null) return;

        string promoteTo = classData.promotionClass != null ? classData.promotionClass.className : "???";
        nameLevelText.text = "Promote: " + classData.className + " → " + promoteTo;
        upgradeDescText.text = string.IsNullOrEmpty(classData.promotionDescription)
            ? "Promote " + classData.className + " to a stronger form!"
            : classData.promotionDescription;
        if (weaponIcon != null && classData.classIcon != null)
            weaponIcon.sprite = classData.classIcon;
    }

    // -----------------------------------------------------------------------
    // Button click handler
    // -----------------------------------------------------------------------

    public void SelectUpgrade()
    {
        if (currentChoice == null) return;

        switch (currentChoice.type)
        {
            case ChoiceType.Weapon:
                HandleWeaponChoice(currentChoice.weapon);
                break;
            case ChoiceType.SelectClass:
                HandleClassChoice(currentChoice.classData);
                break;
            case ChoiceType.Promotion:
                HandlePromotionChoice(currentChoice.classData);
                break;
        }

        UIController.instance.UpdateActiveClassDisplay();
        UIController.instance.levelUpPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void HandleWeaponChoice(Weapon weapon)
    {
        if (weapon == null) return;
        if (weapon.gameObject.activeSelf)
            weapon.LevelUp();
        else
            PlayerController.instance.AddWeapon(weapon);
    }

    private void HandleClassChoice(ClassData classData)
    {
        if (classData == null) return;
        ClassManager.instance.UnlockClass(classData);
        // Unlock the first weapon of the new class automatically if available
        if (classData.classWeapons != null && classData.classWeapons.Count > 0)
        {
            Weapon firstWeapon = classData.classWeapons[0];
            if (firstWeapon != null && !PlayerController.instance.assignedWeapons.Contains(firstWeapon))
                PlayerController.instance.AddWeapon(firstWeapon);
        }
    }

    private void HandlePromotionChoice(ClassData classData)
    {
        if (classData == null) return;
        ClassManager.instance.PromoteClass(classData);
    }

    // -----------------------------------------------------------------------
    // Unity lifecycle
    // -----------------------------------------------------------------------

    void Start() { }
    void Update() { }
}
