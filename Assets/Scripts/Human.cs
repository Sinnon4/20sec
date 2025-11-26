using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEditor.ShaderData;

public class Human : MonoBehaviour
{
    RoomHandler roomHandler;
    Player player;

    public GameObject room;

    float p_x, p_y;
    Vector2 dir;
    bool isMoving;
    bool failedMovement;
    [SerializeField] float moveDuration; //INVERSE TO MAKE SPEED

    Light2D torch;

    private void Awake()
    {
        roomHandler = FindAnyObjectByType<RoomHandler>();
        player = FindAnyObjectByType<Player>();

        torch = GetComponentInChildren<Light2D>();
        if (roomHandler.enableFOV) torch.enabled = true;
    }

    private void FixedUpdate()
    {
        p_x = player.transform.position.x;
        p_y = player.transform.position.y;
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
        else if (roomHandler.activeRoom == room && !isMoving)
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

    private IEnumerator Move(Vector2 dir)
    {
        Vector2 startPosition = transform.position;
        Vector2 endPosition = startPosition + dir; //assumed grid size is 1 [otherwise, use (direction * gridSize) ]

        if (endPosition.x < roomHandler.activeBorder1.x || endPosition.x > roomHandler.activeBorder2.x ||
            endPosition.y > roomHandler.activeBorder1.y || endPosition.y < roomHandler.activeBorder2.y)
        {
            for (int i = 0; i < roomHandler.doors.Count; i++)
            {
                if (roomHandler.doors[i].transform.position == new Vector3(endPosition.x, endPosition.y, roomHandler.doors[i].transform.position.z)) { failedMovement = true; yield break; } //exit coroutine if the endPos is on a border
            }
        }

        foreach (GameObject item_ in roomHandler.items)
        {
            if (endPosition == (Vector2)item_.transform.position) { failedMovement = true; yield break; } //exit coroutine if item is in the endPos
        }

        failedMovement = false;
        isMoving = true;

        float elapsedTime = 0;
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float percent = elapsedTime / moveDuration;
            transform.position = Vector2.Lerp(startPosition, endPosition, percent);
            yield return null;
        }

        transform.position = endPosition;

        isMoving = false;
    }
}
