using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/CombatWeapon")]
public class WeaponItem : ScriptableObject
{
  public int _id;
  public Vector3 _weaponContainerOffset;
  [Header("UI & Info")]
  public string _weaponName;
  public string _weaponDescription;
  // Should contain weaponScript class reference
  public GameObject _weaponPrefab;
}
