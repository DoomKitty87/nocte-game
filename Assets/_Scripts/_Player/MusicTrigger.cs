using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
  private void OnTriggerEnter(Collider other) {
    if (other.CompareTag("SoundTrigger")) MusicManager.Instance.EnterSector(other.GetComponent<MusicData>());
  }

  private void OnTriggerExit(Collider other) {
    if (other.CompareTag("SoundTrigger")) MusicManager.Instance.ExitSector(other.GetComponent<MusicData>());
  }
  
}