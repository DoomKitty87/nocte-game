using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CommandCenterUnderground : MonoBehaviour
{

  public void LeaveUnderground() {
    WeatherManager.Instance._sunTransform.gameObject.GetComponent<Light>().enabled = true;
    SceneManager.UnloadSceneAsync("CommandCenter");
    PlayerController.Instance.transform.position = EntryAnimationHandler._dropPosition;
  }

}
