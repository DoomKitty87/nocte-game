using UnityEngine;

public class RadioQuality : MonoBehaviour
{

  private AudioSource _audioSource;

  [SerializeField] AudioMixer _audioMixer;

  private void Start() {
    _audioSource = GetComponent<AudioSource>();
    _audioSource.output =_audioMixer;
  }

  private void Update() {
    float distance = Vector2.Magnitude(transform.position.xz);
    bool isBlocked = Physics.Raycast(Vector3.up * 10, transform.position.normalized, distance);
    if (isBlocked) distance *= 5;
    float distortionLevel = Mathf.Min(distance / 100f, 1);
    //_audioMixer.SetFloat whatever the distortion level is
  }

}