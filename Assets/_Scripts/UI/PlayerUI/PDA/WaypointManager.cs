using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class WaypointManager : MonoBehaviour
{

  public static WaypointManager Instance;

  private List<Waypoint> _waypoints = new List<Waypoint>();

  [SerializeField] private GameObject _waypointPrefab;

  [SerializeField] private Transform _waypointHolder;

  [SerializeField] private TextMeshProUGUI _waypointName;
  [SerializeField] private TextMeshProUGUI _waypointDistance;
  [SerializeField] private TextMeshProUGUI _playerCoordinates;
  [SerializeField] private TextMeshProUGUI _currentBearing;
  [SerializeField] private TextMeshProUGUI _desiredBearing;

  [SerializeField] private Transform _player;

  [SerializeField] private Transform _compass;
  [SerializeField] private Image _compassFill;

  [SerializeField] private PDAHome _pdaHome;

  private int _currentWaypoint = -1;

  private void Awake() {
    Instance = this;
  }

  private void OnDisable() {
    Instance = null;
  }

  public void CollectedWaypoint(Waypoint waypoint) {
    _waypoints.Add(waypoint);
  }

  public void UpdateWaypoints() {
    for (int i = _waypointHolder.childCount; i > 0; i--) {
      Destroy(_waypointHolder.GetChild(i - 1).gameObject);
    }
    for (int i = 0; i < _waypoints.Count; i++) {
      GameObject waypoint = Instantiate(_waypointPrefab);
      waypoint.transform.parent = _waypointHolder;
      waypoint.transform.position = _waypoints[i]._position;
      waypoint.transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = _waypoints[i]._name;
      waypoint.GetComponent<WaypointSelect>().index = i;
      //waypoint.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = _pdaHome.GetCoordinates(_waypoints[i]._position);
    }
  }

  public void SelectWaypoint(int index) {
    _currentWaypoint = index;
    _waypointName.text = _waypoints[index]._name;
  }

  private void Update() {
    _compass.localRotation = Quaternion.Euler(0, 0, -_player.rotation.eulerAngles.y);
    _compassFill.fillAmount = 0;

    if (_currentWaypoint == -1) return;

    Vector2 waypoint = _waypoints[_currentWaypoint]._position;
    Vector2 player = new Vector2(_player.position.x, _player.position.z);
    _waypointDistance.text = Vector2.Distance(player, waypoint).ToString("F2") + "M";
    _playerCoordinates.text = _pdaHome.GetCoordinates(player);
    float currBearing = _compass.localRotation.eulerAngles.z;
    float desiredBearing = Mathf.Atan2(waypoint.x - player.x, waypoint.y - player.y) * Mathf.Rad2Deg;
    desiredBearing = (desiredBearing + 360) % 360;
    _currentBearing.text = (currBearing).ToString("F2") + "°";
    _desiredBearing.text = (desiredBearing).ToString("F2") + "°";
    _compassFill.fillAmount = (desiredBearing - currBearing) / 360;
  }

}