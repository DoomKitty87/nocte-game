using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmmoUI : MonoBehaviour
{
  [SerializeField] private PlayerCombatCore _playerCombatCore;
  [SerializeField] private TextMeshProUGUI _ammoText;
  [SerializeField] private Image _ammoFill;

  private void Start() {
    _playerCombatCore.AmmoChanged += UpdateAmmoCount;
  }

  public void UpdateAmmoCount(int currentAmmo, int maxAmmo) {
    _ammoText.text = $"{currentAmmo} / {maxAmmo}";
    _ammoFill.fillAmount = currentAmmo / maxAmmo;
  }
  

  private void OnDisable() {
    _playerCombatCore.AmmoChanged -= UpdateAmmoCount;
  }
}