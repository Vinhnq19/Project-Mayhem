using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHUD
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI ammoText;

    public PlayerHUD(TextMeshProUGUI damageText, TextMeshProUGUI livesText, TextMeshProUGUI ammoText)
    {
        this.damageText = damageText;
        this.livesText = livesText;
        this.ammoText = ammoText;
    }
    public void UpdateDamageText(float damagePercent)
    {
        if (damageText != null)
        {
            damageText.text = $"Damage: {Mathf.FloorToInt(damagePercent)}%";
        } 
    }

    public void UpdateLivesText(int livesRemaining)
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {livesRemaining}";
        }
    }
    public void UpdateAmmoText(int currentAmmo, int maxAmmo)
    {
        if (ammoText != null)
        {
            ammoText.text = $"Ammo: {currentAmmo}/{maxAmmo}";
        }
    }

}
