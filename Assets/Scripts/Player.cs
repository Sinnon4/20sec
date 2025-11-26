using System.Collections;
using UnityEditor.AnimatedValues;
using UnityEngine;
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

    public bool hasCow = false;

    Animator anim;

    private void Awake()
    {
        roomHandler = FindAnyObjectByType<RoomHandler>();

        torch = GetComponentInChildren<Light2D>();
        if (roomHandler.enableFOV) torch.enabled = true;

        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!isMoving && !roomHandler.pendingRound)
        {
            anim.SetBool("upCheck", false);
            anim.SetBool("downCheck", false);
            anim.SetBool("leftCheck", false);
            anim.SetBool("rightCheck", false);

            System.Func<KeyCode, bool> inputFunction;
            inputFunction = Input.GetKey;

            if (inputFunction(KeyCode.UpArrow) || inputFunction(KeyCode.W))
            {
                //LeanTween.rotateZ(gameObject, 0, rotateTime);
                LeanTween.rotateZ(torch.gameObject, 0, rotateTime);
                StartCoroutine(Move(Vector2.up, "upCheck"));
            }
            else if (inputFunction(KeyCode.DownArrow) || inputFunction(KeyCode.S))
            {
                //LeanTween.rotateZ(gameObject, 180, rotateTime);
                LeanTween.rotateZ(torch.gameObject, 180, rotateTime);
                StartCoroutine(Move(Vector2.down, "downCheck"));
            }
            else if (inputFunction(KeyCode.LeftArrow) || inputFunction(KeyCode.A))
            {
                //LeanTween.rotateZ(gameObject, 90, rotateTime);
                LeanTween.rotateZ(torch.gameObject, 90, rotateTime);
                StartCoroutine(Move(Vector2.left, "leftCheck"));
            }
            else if (inputFunction(KeyCode.RightArrow) || inputFunction(KeyCode.D))
            {
                //LeanTween.rotateZ(gameObject, -90, rotateTime);
                LeanTween.rotateZ(torch.gameObject, -90, rotateTime);
                StartCoroutine(Move(Vector2.right, "rightCheck"));
            }
        }
    }

    private IEnumerator Move(Vector2 direction, string animBool)
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

        foreach (GameObject item_ in roomHandler.items)
        {
            if (endPosition == (Vector2)item_.transform.position) yield break; //exit coroutine if item is in the endPos
        }

        isMoving = true;
        anim.SetBool(animBool, true);

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
        else if (endPosition == roomHandler.cowPos) { roomHandler.grabCow(); }

        isMoving = false;
    }
}
