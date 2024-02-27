using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponUI : MonoBehaviour
{

  [SerializeField] private TextMeshProUGUI _ammoText;
  [SerializeField] private Image _ammoIcon;

  public void UpdateAmmoCount((float, float) ammoData) {
    _ammoText.text = ammoData.Item1.ToString();
    _ammoIcon.fillAmount = ammoData.Item1 / ammoData.Item2;
  }

}