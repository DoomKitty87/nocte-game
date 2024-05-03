using UnityEngine;

public class PickupWeapon : MonoBehaviour
{
  [SerializeField] private WeaponItem _weapon;
  [SerializeField] private GameObject _deleteParent;

  public void Pickup() {
    GameObject go = GameObject.FindGameObjectWithTag("Player");
    PlayerCombatCore combatCore = go.GetComponent<PlayerCombatCore>();
    combatCore.AddWeapon(_weapon);
    combatCore.EquipWeaponByWeaponItem(_weapon);
    Destroy(_deleteParent);
  }
}