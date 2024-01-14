using UnityEngine;

public class MusicData : MonoBehaviour
{

  [System.Serializable]
  public struct Song {

    public AudioClip clip;
    public string name;
    public string artist;

  }

  public string _name;
  public Song[] _songs;

}