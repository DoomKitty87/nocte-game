using UnityEngine;

public class RobotSpawner : MonoBehaviour
{

  [SerializeField] private GameObject _robotPrefab;
  [SerializeField] private Transform _spawnPoint;

  private void Start()
  {
    Instantiate(_robotPrefab, _spawnPoint.position, Quaternion.identity).GetComponent<RobotAI>()._playerTransform = InitiatePlayer.PlayerTransform;
  }
  
}