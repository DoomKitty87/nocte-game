using UnityEngine;

[AddComponentMenu("Editor Note")]
public class EditorNote : MonoBehaviour
{
  [TextArea(2,1000)]
  public string comment = "Lorem Ipsum";
}
