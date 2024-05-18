using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using UnityEngine.SceneManagement;

public class CommandCenter : MonoBehaviour
{
  [Header("Audio")]
  [SerializeField] private AudioSource _cmdCenterAudio;
  [SerializeField] private AudioClip _enableBackupPowerSound;
  [SerializeField] private AudioClip _scanSound;
  [SerializeField] private AudioClip _activateHologramSound;
  [SerializeField] private AudioClip _activeAmbientLoop;
  [Header("Screens")]
  [FormerlySerializedAs("_lowPowerScreen")] 
  [SerializeField] private GameObject _emergencyPowerScreen;
  [SerializeField] private GameObject _fullPowerScreen;
  [SerializeField] private GameObject _scanningScreen;
  [SerializeField] private float _scanTime = 3f;
  [SerializeField] private GameObject _mainInterface;
  [SerializeField] private TextMeshProUGUI _distanceText;
  [Header("Hologram")]
  [SerializeField] private VisualEffect _hologramEffect;
  [SerializeField] private Canvas _holoCanvas;
  [Header("Animation Triggers")]
  [SerializeField] private List<Animator> _animatorsToTrigger;
  [SerializeField] private string _powerTrigger;
  [SerializeField] private string _scanTrigger;
  
  [SerializeField] private Dialogue _endDialogue;

  [SerializeField] private GameObject _secondDialogueTrigger;

  [SerializeField] private Animator _elevatorAnimator;
  [SerializeField] private AudioClip _elevatorSound;

  public static CommandCenter Instance;
  
  private bool _powered = false;

  private void Awake() {
    Instance = this;
  }

  public void ActivateElevator() {
    Debug.Log("Elevator activated");
    _elevatorAnimator.enabled = true;
    Invoke("TransportPlayer", 1f);
    if (_elevatorSound != null) _cmdCenterAudio.PlayOneShot(_elevatorSound);
  }

  private void TransportPlayer() {
    WorldGenInfo._worldGenerator.gameObject.SetActive(false);
    SceneManager.LoadScene("CommandCenter", LoadSceneMode.Additive);
    PlayerController.Instance.transform.position = new Vector3(0, 0, 0);
  }

  private void Start() {
    _emergencyPowerScreen.SetActive(true);
    _fullPowerScreen.SetActive(false);
    _scanningScreen.SetActive(false);
    _mainInterface.SetActive(false);
    _holoCanvas.enabled = false;
    _hologramEffect.SetInt("Spawn Rate", 0);
    int nearestSite = (int) PlaceStructures.Instance.GetNearestSite(transform.position);
    _distanceText.text = $"DIST TO NEAREST: <color=\"green\">{nearestSite}M</color>";
  }
  public void EnableBackupPower() {
    _emergencyPowerScreen.SetActive(false);
    _fullPowerScreen.SetActive(true);
    _powered = true;
  }

  private int _interactCount = 0;
  public void ScreenInteracted() {
    if (!_powered) return;
    if (_interactCount == 0) {
      ScanForStructures();
    }
    _interactCount++;
  }
  
  public void DoneWithExtraction() {
    _secondDialogueTrigger.SetActive(true);
  }

  private bool _scanning;
  private IEnumerator ScanForStructuresCoroutine() {
    _scanning = true;
    _fullPowerScreen.SetActive(false);
    _scanningScreen.SetActive(true);
    yield return new WaitForSeconds(_scanTime);
    _scanningScreen.SetActive(false);
    EnableMainInterface();
    _hologramEffect.SetInt("Spawn Rate", 200);
    _holoCanvas.enabled = true;
    _scanning = false;
  }
  private void ScanForStructures() {
    if (!_powered || _scanning) return;
    StartCoroutine(ScanForStructuresCoroutine());
  }
  
  private void EnableMainInterface() {
    if (!_powered) return;
    DroneSpawner._isSpawning = true;
    DialogueHandler.Instance.PlayDialogue(_endDialogue);
    // Give player coordinates of extraction site
    _fullPowerScreen.SetActive(false);
    _mainInterface.SetActive(true);
  }

}
