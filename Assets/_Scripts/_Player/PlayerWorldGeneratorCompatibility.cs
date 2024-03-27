using UnityEngine;
using UnityEngine.VFX;

public class PlayerWorldGeneratorCompatibility : MonoBehaviour
{
    public PlayerController _playerController;
    [SerializeField] private WorldGenerator _worldGeneratorObject;
    [SerializeField] private ParticleSystem _rain;
    [SerializeField] private LayerMask _groundMask;
    private bool _hasInitialized;
    private ParticleSystem.ShapeModule _rainShape;

    private static bool _enablePlayer;

    private void Awake() {
        if (enabled && _worldGeneratorObject == null) {
            Debug.LogWarning($"No WorldGeneratorObject found on {this.name}");
            this.enabled = false;
        }

        _playerController = GetComponent<PlayerController>();
        _rainShape = _rain.shape;
    }

    private void Start() {
        _playerController._disableMovement = true;

        WorldGenerator.GenerationComplete += InitiateEnablePlayer;
    }

    private void Update() {
        if (_enablePlayer) EnablePlayer();
        if (!_hasInitialized) return;

        _worldGeneratorObject.UpdatePlayerLoadedChunks(transform.position);
        _rainShape.position = transform.position + Vector3.up * 25f;
        Shader.SetGlobalVector("_PlayerPosition", transform.position);
    }

    private void EnablePlayer() {
        if (Physics.Raycast(Vector3.up * 10000, Vector3.down, out var hit, Mathf.Infinity, _groundMask)) {
            transform.position = hit.point + Vector3.up * 2f;
            _playerController._disableMovement = false;
            Invoke(nameof(ActivatePlayer), 0.1f);
            _hasInitialized = true;
            _enablePlayer = false;
            
            InputReader.Instance.EnablePlayer();
        }
    }

    private static void InitiateEnablePlayer() {
        _enablePlayer = true;
    }

    private void ActivatePlayer() {
        _playerController._disableMovement = false;
    }
}
