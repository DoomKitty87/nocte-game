using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/CombatWeapon")]
public class WeaponItem : ScriptableObject
{
  public int _id;
  [Header("UI & Info")]
  public string _weaponName;
  public string _weaponDescription;
  public enum WeaponType 
  {
    Melee,
    Ranged,
    Charge,
    Area,
  }
  public WeaponType _weaponType;
  // Abstract class reference, contains abstract functions for FireDown, FireHeld, FireUp 
  public WeaponScript _weaponScript;
}
