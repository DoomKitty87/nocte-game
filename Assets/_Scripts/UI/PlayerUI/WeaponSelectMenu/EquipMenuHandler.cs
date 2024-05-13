using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class EquipMenuHandler : MonoBehaviour
{
    [SerializeField] private PlayerCombatCore _playerCombatCore;
    [SerializeField] private RadialMenu _radialMenu;
    [SerializeField] private TextMeshProUGUI _centerNameText;
    [SerializeField] private TextMeshProUGUI _centerDescText;
    
    private void Start() {
        _playerCombatCore._OnInventoryChanged.AddListener(UpdateEquipMenu);
        InputReader.Instance.PlayerInput.AlwaysOn.OpenEquipMenu.performed += _ => OpenEquip();
        _radialMenu._OnHovered.AddListener((value) => OnHovered(value));
        _radialMenu._OnSelected.AddListener((value) => OnSelected(value));
    }
    
    private void UpdateEquipMenu() {
        for (int i = 0; i < _radialMenu.GetSelectionCount(); i++) {
            if (i >= _playerCombatCore.GetWeaponCount()) {
                _radialMenu.SetImageOfIndex(i, null);
                continue;
            }
            _radialMenu.SetImageOfIndex(i, _playerCombatCore.GetWeaponInventory()[i]._weaponItem._weaponIcon);
        }
    }

    private void OnSelected(int index) {
        if (index >= _playerCombatCore.GetWeaponCount()) return;
        _playerCombatCore.EquipWeaponByIndex(index);
    }
    
    private void OnHovered(int index) {
        if (index >= _playerCombatCore.GetWeaponCount()) {
            UpdateCenterText(null);
            return;
        }
        UpdateCenterText(_playerCombatCore.GetWeaponInventory()[index]._weaponItem);
    }

    private void OpenEquip() {
        UpdateCenterText(null);
    }
    
    private void UpdateCenterText(WeaponItem weaponItem) {
        if (weaponItem == null) {
            _centerNameText.text = "";
            _centerDescText.text = "";
            return;
        }
        _centerNameText.text = weaponItem._weaponName;
        _centerDescText.text = weaponItem._weaponDescription;
    }

    private void OnDisable() {
        _radialMenu._OnHovered.RemoveListener((value) => OnHovered(value));
        _radialMenu._OnSelected.RemoveListener((value) => OnSelected(value));
    }
}

