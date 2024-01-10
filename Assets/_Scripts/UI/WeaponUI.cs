using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponUI : MonoBehaviour
{

  [SerializeField] private TextMeshProUGUI _ammoCurrent;
  [SerializeField] private TextMeshProUGUI _ammoMax;

  public void UpdateAmmoCount((float, float) ammoData) {
    _ammoCurrent.text = ammoData.Item1;
    _ammoMax.text = ammoData.Item2;
  }

}