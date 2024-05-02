using UnityEngine;
using UnityEngine.Serialization;

public abstract class WeaponScript : MonoBehaviour
{
  [FormerlySerializedAs("_instancingCombatCoreScript")] [HideInInspector] public PlayerCombatCore _instancingPlayerCombatCoreScript;

  [Header("WeaponScript Dependencies")]
  public Transform _leftHandPosMarker;
  [SerializeField] protected float _aimSpeed = 6f;
  
  private float _aimParamTarget;
  protected void LerpAimingParametersUpdate() {
    _instancingPlayerCombatCoreScript._AimParameter = Mathf.Lerp(_instancingPlayerCombatCoreScript._AimParameter, _aimParamTarget, Time.deltaTime * _aimSpeed);
    if (_instancingPlayerCombatCoreScript._AimParameter < 0.02f) _instancingPlayerCombatCoreScript._AimParameter = 0;
    if (_instancingPlayerCombatCoreScript._AimParameter > 0.98f) _instancingPlayerCombatCoreScript._AimParameter = 1;
  }
  protected void LerpAimingParameters(bool aimedIn) { 
    if (aimedIn) {
      _aimParamTarget = 1;
    } else {
      _aimParamTarget = 0;
    }
  }
  
  // Called on any frame fire is down immediately after a frame where fire is up
  public abstract void FireDown();
  // Called for every frame the mouse is down, excluding the FireDown() frame
  public abstract void FireHold();
  // vice versa of FireDown()
  public abstract void FireUp();
  // Called on any frame fire is down immediately after a frame where fire is up
  public abstract void Fire2Down();
  // Called for every frame the mouse is down, excluding the FireDown() frame
  public abstract void Fire2Hold();
  // vice versa of FireDown()
  public abstract void Fire2Up();
  public abstract void ReloadDown();
  // Called for every frame the mouse is down, excluding the ReloadDown() frame
  public abstract void ReloadHold();
  // vice versa of ReloadDown()
  public abstract void ReloadUp();
  // float = time to wait for animations
  public abstract float OnEquip();
  public abstract float OnUnequip();
  public virtual (int, int) GetAmmo {
    get {
      return (-1, -1);
    }
  }
}