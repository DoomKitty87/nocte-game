using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XPBarHandler : MonoBehaviour
{
	private Slider slider;

	private PlayerExperience playerExperience;

	[SerializeField] private TextMeshProUGUI levelText;

	private void Start() {
		slider = GetComponent<Slider>();

		playerExperience = GameHandler.Instance.transform.GetComponent<PlayerExperience>();

		playerExperience.OnGainExperience.AddListener(UpdateXPBar);
		UpgradeInfo.OnLevelChange.AddListener(UpdateLevelText);
	}

	private void UpdateXPBar() {
		StartCoroutine(LerpSlider(playerExperience.CheckExperience() / 100, 0.2f));
	}

	private void UpdateLevelText() {
		levelText.text = UpgradeInfo.xpLevel.ToString();
	}

	private IEnumerator LerpSlider(float targetValue, float duration) {
		float start = slider.value;
		float time = 0;

		while (time < duration) {
			time += Time.deltaTime;
			slider.value = Mathf.Lerp(start, targetValue, time / duration);
			yield return null;
		}

		slider.value = targetValue;
	}

}
