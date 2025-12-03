using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    GameObject player;

    Vector3 offset = new Vector3(0, 0, -10); //keep camera at -10 z
    [SerializeField] float smoothTime = 0.2f;
    Vector3 targetPos;
    Vector3 v = Vector3.zero;

    private void Awake()
    {
        player = FindAnyObjectByType<Player>().gameObject; //use gameobject so that we can destroy player component when dead
    }

    private void Start()
    {
        transform.position = player.transform.position;
    }

    private void Update()
    {
        targetPos = player.transform.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref v, smoothTime);
    }
}
