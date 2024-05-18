using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CommandCenterUnderground : MonoBehaviour
{

  public void LeaveUnderground() {
    Invoke("ActuallyLeaveUnderground", 20f);
  }

  private void ActuallyLeaveUnderground() {
    WeatherManager.Instance._sunTransform.gameObject.GetComponent<Light>().enabled = true;
    SceneManager.UnloadSceneAsync("CommandCenter");
    PlayerController.Instance.SetPosition(EntryAnimationHandler._dropPosition + Vector3.up * 2f);
  }

}
