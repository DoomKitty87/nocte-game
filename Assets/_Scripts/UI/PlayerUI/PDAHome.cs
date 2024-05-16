using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PDAHome : MonoBehaviour
{

  [SerializeField] private TextMeshProUGUI _location;
  [SerializeField] private TextMeshProUGUI _elevation;
  [SerializeField] private TextMeshProUGUI _time;
  [SerializeField] private TextMeshProUGUI _timeToSunset;
  [SerializeField] private Image _clockFill;
  [SerializeField] private Transform _compass;

  [SerializeField] private Transform _player;
  
  [SerializeField] private float _gpsScale = 10000;

  private float _altitudeOffset;
  private Vector2 _positionOffset;

  private void Start() {
    int seed = int.Parse(Hash128.Compute(WorldGenInfo._seed).ToString().Substring(0, 6), System.Globalization.NumberStyles.HexNumber);
    int seed2 = int.Parse(Hash128.Compute(WorldGenInfo._seed).ToString().Substring(6, 6), System.Globalization.NumberStyles.HexNumber);
    _positionOffset = new Vector2(seed % 360, seed2 % 360);
    _altitudeOffset = seed % 1000;

  }

  private void Update() {
    Vector2 position = new Vector2(_player.position.x / _gpsScale + _positionOffset.x, _player.position.z / _gpsScale + _positionOffset.y);
    position.x = position.x % 360 - 180;
    position.y = position.y % 360 - 180;
    string lattext = "N" + (position.x).ToString("F4") + "°";
    string longtext = "E" + (position.y).ToString("F4") + "°";
    _location.text = "LOCATION - " + lattext + " " + longtext;
    _elevation.text = "ALTITUDE - " + (_player.position.y + _altitudeOffset).ToString("F2") + "M ASL";

    float time = WeatherManager.Instance.GetDayNightCycle();
    _time.text = "TIME - " + Mathf.Floor(time * 24).ToString("00") + ":" + Mathf.Floor((time * 24 % 1) * 60).ToString("00");
    float timeToSunset = time < 0.5f ? 0.5f - time : 1 - time;
    bool isDay = time < 0.5f;
    _timeToSunset.text = "TIME TO " + (isDay ? "SUNSET" : "SUNRISE") + " - " + Mathf.Floor(timeToSunset * 24).ToString("00") + ":" + Mathf.Floor((timeToSunset * 24 % 1) * 60).ToString("00");
    _clockFill.fillAmount = time;

    _compass.localRotation = Quaternion.Euler(0, 0, -_player.rotation.eulerAngles.y);
  }

}