using UnityEngine;
using UnityEngine.Serialization;

public abstract class WeaponScript : MonoBehaviour
{
  [FormerlySerializedAs("_instancingCombatCoreScript")] [HideInInspector] public PlayerCombatCore _instancingPlayerCombatCoreScript;

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
}