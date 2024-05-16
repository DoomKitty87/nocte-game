using Foliage;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerWorldGeneratorCompatibility : MonoBehaviour
{
    public PlayerController _playerController;
    private WorldGenerator _worldGeneratorObject;
    [SerializeField] private ParticleSystem _rain;
    [SerializeField] private LayerMask _groundMask;
    private bool _hasInitialized;
    private ParticleSystem.ShapeModule _rainShape;

    public static bool _entryAnimationFinished = false;

    [SerializeField] private TutorialHandler _tutorial;
    
    [SerializeField] private bool _ignoreLackOfWorldGenerator;

    private void Awake() {
      _playerController = GetComponent<PlayerController>();
      _rainShape = _rain.shape;
    }

    private void Start() {
      _worldGeneratorObject = WorldGenInfo._worldGenerator;
    
      _playerController._disableMovement = true;
      if (_worldGeneratorObject == null && _ignoreLackOfWorldGenerator) {
        EnablePlayer();
      }
    }

    private void Update() {
      if (_entryAnimationFinished) EnablePlayer();
      if (!_hasInitialized) return;

      if (_worldGeneratorObject == null && _ignoreLackOfWorldGenerator) return;

      _worldGeneratorObject.UpdatePlayerLoadedChunks(transform.position);
      _rainShape.position = transform.position + Vector3.up * 25f;
      Shader.SetGlobalVector("_PlayerPosition", transform.position);
    }

    private void EnablePlayer() {
      // Debug.Log(EntryAnimationHandler._dropPosition);
      _playerController.SetPosition(EntryAnimationHandler._dropPosition + Vector3.up * 2f);
      _playerController._disableMovement = false;
      _hasInitialized = true;
      _entryAnimationFinished = false;
      
      InputReader.Instance.EnablePlayer();
      _tutorial.InitialDialogue();
    }

}
