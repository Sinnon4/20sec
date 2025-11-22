using UnityEngine;
using UnityEngine.UIElements;

public class DoorPath : MonoBehaviour
{
    public GameObject destination;
    [Range(-1,1)] public int xDir, yDir;
}
