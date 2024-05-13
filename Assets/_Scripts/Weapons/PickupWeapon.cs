using System;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class PickupWeapon : MonoBehaviour
{
  [SerializeField] private Interactable _interactable;
  [SerializeField] private WeaponItem _weapon;
  [SerializeField] private GameObject _deleteParent;

  private PlayerCombatCore _playerCombatCore;
  
  private void Start() {
    GameObject go = GameObject.FindGameObjectWithTag("Player");
    _playerCombatCore = go.GetComponent<PlayerCombatCore>();
    _interactable = gameObject.GetComponent<Interactable>();
    _interactable._OnHoverStart.AddListener(OnHoverStart);
    _interactable._InteractedWith.AddListener(Pickup);
  }

  private void OnHoverStart() {
    if (_playerCombatCore.GetWeaponCount() >= _playerCombatCore.GetMaxWeaponCount()) {
      _interactable.PromptText = "Inventory Full";
    }
  }
  
  public void Pickup() {
    if (_playerCombatCore.AddWeapon(_weapon)) {
      _playerCombatCore.UnequipCurrentWeapon();
      _playerCombatCore.EquipWeaponByWeaponItem(_weapon);
      Destroy(_deleteParent);
    }
  }

  private void OnDisable() {
    _interactable._OnHoverStart.RemoveListener(OnHoverStart);
    _interactable._InteractedWith.RemoveListener(Pickup);
  }
}