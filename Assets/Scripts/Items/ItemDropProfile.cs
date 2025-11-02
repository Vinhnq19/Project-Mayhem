using System.Collections;
using System.Collections.Generic;
using ProjectMayhem.Effects;
using UnityEngine;

public enum DropType { Weapon, Effect }

[CreateAssetMenu(fileName = "New Item Drop", menuName = "ProjectMayhem/Item Drop Profile")]

public class ItemDropProfile : ScriptableObject
{
    [Header("Drop Type")]
    public DropType dropType;

    [Header("Weapon Drop")]
    public WeaponData weaponData; //Only if dropType is Weapon
    [Header("Effect Drop")]
    public BaseEffect effectData; //Only if dropType is Effect

    [Header("Spawn Settings")]
    public float spawnWeight = 1f; // spawn probability weight

    [Header("Visuals")]
    public Sprite dropIcon;
    public Color dropColor = Color.white;

    private void OnValidate()
    {
        if (dropType == DropType.Weapon && weaponData == null)
        {
            Debug.LogWarning($"[ItemDropProfile] {name} is Weapon type but has no WeaponData!");
        }
        
        if (dropType == DropType.Effect && effectData == null)
        {
            Debug.LogWarning($"[ItemDropProfile] {name} is Effect type but has no EffectData!");
        }
    }
}
