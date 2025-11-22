using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class Player : MonoBehaviour
{
    RoomHandler roomHandler;

    [SerializeField] private float moveDuration = 0.5f;
    bool isMoving = false;
    bool pass;
    int doorNo;
    
    Light2D torch;
    [SerializeField] float rotateTime;

    private void Awake()
    {
        roomHandler = FindAnyObjectByType<RoomHandler>();

        torch = GetComponentInChildren<Light2D>();
        if (roomHandler.enableFOV) torch.enabled = true;
    }

    private void Update()
    {
        if (!isMoving && !roomHandler.pendingRound)
        {
            System.Func<KeyCode, bool> inputFunction;
            inputFunction = Input.GetKey;

            if (inputFunction(KeyCode.UpArrow) || inputFunction(KeyCode.W))
            {
                LeanTween.rotateZ(torch.gameObject, 0, rotateTime);
                StartCoroutine(Move(Vector2.up));
            }
            else if (inputFunction(KeyCode.DownArrow) || inputFunction(KeyCode.S))
            {
                LeanTween.rotateZ(torch.gameObject, 180, rotateTime);
                StartCoroutine(Move(Vector2.down));
            }
            else if (inputFunction(KeyCode.LeftArrow) || inputFunction(KeyCode.A))
            {
                LeanTween.rotateZ(torch.gameObject, 90, rotateTime);
                StartCoroutine(Move(Vector2.left));
            }
            else if (inputFunction(KeyCode.RightArrow) || inputFunction(KeyCode.D))
            {
                LeanTween.rotateZ(torch.gameObject, -90, rotateTime);
                StartCoroutine(Move(Vector2.right));
            }
        }
    }

    private IEnumerator Move(Vector2 direction)
    {
        pass = false;

        Vector2 startPosition = transform.position;
        Vector2 endPosition = startPosition + direction; //assumed grid size is 1 [otherwise, use (direction * gridSize) ]

        if (endPosition.x < roomHandler.activeBorder1.x || endPosition.x > roomHandler.activeBorder2.x ||
            endPosition.y > roomHandler.activeBorder1.y || endPosition.y < roomHandler.activeBorder2.y)
        {
            for (int i = 0; i < roomHandler.doors.Count; i++)
            {
                if (roomHandler.doors[i].transform.position == new Vector3(endPosition.x, endPosition.y, roomHandler.doors[i].transform.position.z)) { pass = true; doorNo = i; }
            }
            if (!pass) yield break; //exit coroutine if the endPos is on a border and no door there
        }

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

        if (pass == true)
        {
            DoorPath dp = roomHandler.doors[doorNo].GetComponent<DoorPath>();
            if (dp.destination == null) { print("<color=yellow>Locked! :O"); transform.position = startPosition; }
            else if (dp.xDir == 1) transform.position = dp.destination.transform.position + Vector3.right;
            else if (dp.xDir == -1) transform.position = dp.destination.transform.position + Vector3.left;
            else if (dp.yDir == 1) transform.position = dp.destination.transform.position + Vector3.up;
            else if (dp.yDir == -1) transform.position = dp.destination.transform.position + Vector3.down;

            roomHandler.updateActiveRoom(transform.position);
        }
        else if (endPosition == roomHandler.cowPos) { roomHandler.WinGame(); }

        isMoving = false;
    }
}
