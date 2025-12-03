using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Human : MonoBehaviour
{
    public static Human instance;
    RoomHandler roomHandler;
    GameObject player;

    public GameObject room;

    [SerializeField] public float speed = 1;
    float moveDuration;
    float p_x, p_y;
    Vector2 dir;
    bool isMoving;
    bool failedMovement;

    [SerializeField] AudioClip movementclip;
    AudioSource source;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        roomHandler = FindAnyObjectByType<RoomHandler>();
        player = FindAnyObjectByType<Player>().gameObject; //use gameobject so that we can destroy player component when dead
        source = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        p_x = player.transform.position.x;
        p_y = player.transform.position.y;
        if (roomHandler.rooms[roomHandler.activeRoom].gameObject == room)
        {
            if (failedMovement)
            {
                if (Mathf.Abs(p_x - transform.position.x) > Mathf.Abs(p_y - transform.position.y))
                {
                    if (p_y > transform.position.y) { dir = Vector2.up; LeanTween.rotateZ(gameObject, 0, 0.1f); }
                    else { dir = Vector2.down; LeanTween.rotateZ(gameObject, 180, 0.1f); }
                }
                else
                {
                    if (p_x > transform.position.x) { dir = Vector2.right; LeanTween.rotateZ(gameObject, -90, 0.1f); }
                    else { dir = Vector2.left; LeanTween.rotateZ(gameObject, 90, 0.1f); }
                }

                StartCoroutine(Move(dir));
            }
            else if (!isMoving)
            {
                if (Mathf.Abs(p_x - transform.position.x) > Mathf.Abs(p_y - transform.position.y))
                {
                    if (p_x > transform.position.x) { dir = Vector2.right; LeanTween.rotateZ(gameObject, -90, 0.1f); }
                    else { dir = Vector2.left; LeanTween.rotateZ(gameObject, 90, 0.1f); }
                }
                else
                {
                    if (p_y > transform.position.y) { dir = Vector2.up; LeanTween.rotateZ(gameObject, 0, 0.1f); }
                    else { dir = Vector2.down; LeanTween.rotateZ(gameObject, 180, 0.1f); }
                }

                StartCoroutine(Move(dir));
            }
        }
    }

    private IEnumerator Move(Vector2 dir)
    {
        Vector2 startPosition = transform.position;
        Vector2 endPosition = startPosition + dir; //assumed grid size is 1 [otherwise, use (direction * gridSize) ]

        if (!roomHandler.isWithinRoom(endPosition, roomHandler.activeRoom))
        {
            print("CHECK IF WORKING");
            failedMovement = true;
            yield break; //exit coroutine if the endPos is on a border
        }

        foreach (GameObject item_ in roomHandler.items)
        {
            if (endPosition == (Vector2)item_.transform.position)
            {
                failedMovement = true;
                yield break; //exit coroutine if item is in the endPos
            }
        }

        failedMovement = false;
        isMoving = true;

        SoundManager.instance.PlayClip(movementclip, source, true);

        float elapsedTime = 0;
        moveDuration = 1 / speed;
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float percent = elapsedTime / moveDuration;
            transform.position = Vector2.Lerp(startPosition, endPosition, percent);
            yield return null;
        }

        transform.position = endPosition;

        isMoving = false;
        source.Stop();
    }
}
