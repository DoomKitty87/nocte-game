using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetSliderText : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private string _textFormat = "0.00";
    [SerializeField] private float _multiplier = 1;
    
    private void Start() {
        _slider.onValueChanged.AddListener(UpdateText);
        UpdateText(_slider.value);
    }
    
    private void UpdateText(float value) {
        _text.text = (value * _multiplier).ToString(_textFormat);
    }
}
