using UnityEngine;

public class PickupWeapon : MonoBehaviour
{
  [SerializeField] private WeaponItem _weapon;
  [SerializeField] private GameObject _deleteParent;

  public void Pickup()
  {
    GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCombatCore>().AddWeapon(_weapon);
    Destroy(_deleteParent);
  }
}