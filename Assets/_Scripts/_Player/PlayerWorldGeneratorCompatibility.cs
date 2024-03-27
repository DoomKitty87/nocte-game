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

    public static bool _entryAnimationFinished = false;

    public void SetEntryAnimationFinished() {
      _entryAnimationFinished = true;
    }

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
      WorldGenerator.GenerationComplete += SetEntryAnimationFinished;
    }

    private void OnDisable() {
      WorldGenerator.GenerationComplete -= SetEntryAnimationFinished;
    }

    private void Update() {
      if (_entryAnimationFinished) EnablePlayer();
      if (!_hasInitialized) return;

      _worldGeneratorObject.UpdatePlayerLoadedChunks(transform.position);
      _rainShape.position = transform.position + Vector3.up * 25f;
      Shader.SetGlobalVector("_PlayerPosition", transform.position);
    }

    private void EnablePlayer() {
      _playerController.SetPosition(Vector3.up * WorldGenInfo._worldGenerator.GetHeightValue(Vector2.zero) + Vector3.up * 2f);
      _playerController._disableMovement = false;
      _hasInitialized = true;
      _entryAnimationFinished = false;
      
      InputReader.Instance.EnablePlayer();
    }

}
