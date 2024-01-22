using UnityEngine;
using UnityEngine.VFX;

public class PlayerWorldGeneratorCompatibility : MonoBehaviour
{
    private PlayerController _playerController;
    [SerializeField] private WorldGenerator _worldGeneratorObject;
    [SerializeField] private VisualEffect _rain;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _timeToInitialize;
    private bool _hasInitialized;
    private Vector3 location;
    
    
    private void Awake() {
        if (enabled && _worldGeneratorObject == null) throw new NullReferenceException("No WorldGeneratorObject found.");

        _playerController = GetComponent<PlayerController>();
    }

    private void Start() {
        _playerController._disableMovement = true;
    }

    private void Update() {
        _worldGeneratorObject.UpdatePlayerLoadedChunks(transform.position);
        _rain.SetVector3("PlayerPos", transform.position);
        
        // Delayed start
        if (!_hasInitialized && Time.time > _timeToInitialize) {
            
            // Arbitrarily high origin, pointed downwards due to World Gen Chunks only having upwards facing colliders
            if (Physics.Raycast(Vector3.up * 10000 + new Vector3(10, 0, 10), Vector3.down, out var hit, Mathf.Infinity, _groundMask)) {
                transform.position = hit.point + Vector3.up * 2f;
                Invoke(nameof(ActivatePlayer), 0.1f);
                _hasInitialized = true;
            }
        }
    }

    private void ActivatePlayer() {
        _playerController._disableMovement = false;
    }
}
