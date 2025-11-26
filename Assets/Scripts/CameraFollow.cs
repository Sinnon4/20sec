using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Player player;

    Vector3 offset = new Vector3(0, 0, -10); //keep camera at -10 z
    [SerializeField] float smoothTime = 0.2f;
    Vector3 targetPos;
    Vector3 v = Vector3.zero;

    private void Awake()
    {
        player = FindAnyObjectByType<Player>();
    }

    private void Update()
    {
        //transform.position = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z); //keep camera z
        targetPos = player.transform.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref v, smoothTime);
    }
}
