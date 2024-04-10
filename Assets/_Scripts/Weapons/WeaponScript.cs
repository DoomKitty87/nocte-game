using UnityEngine;
using UnityEngine.Serialization;

public abstract class WeaponScript : MonoBehaviour
{
  [FormerlySerializedAs("_instancingCombatCoreScript")] [HideInInspector] public PlayerCombatCore _instancingPlayerCombatCoreScript;
  [HideInInspector] public PlayerInput _inputComponent;
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

  public virtual (float, float) GetAmmo { get {return (-1, -1);} }
}