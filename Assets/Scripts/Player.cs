using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    RoomHandler roomHandler;

    [SerializeField] private float moveDuration = 0.5f;
    bool isMoving = false;

    [SerializeField] Transform torch;
    [SerializeField] float rotateTime;

    private void Awake()
    {
        roomHandler = FindAnyObjectByType<RoomHandler>();

        if (roomHandler.enableFOV) torch.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isMoving)
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
        Vector2 startPosition = transform.position;
        Vector2 endPosition = startPosition + (direction * roomHandler.gridSize);

        float xMid = (endPosition.x + startPosition.x) / 2;
        float yMid = (endPosition.y + startPosition.y) / 2;

        //check for walls and door positions
        if (xMid == roomHandler.activeBorder1.x || xMid == roomHandler.activeBorder2.x ||
            yMid == roomHandler.activeBorder1.y || yMid == roomHandler.activeBorder2.y)
        {
            bool pass = false;
            foreach (GameObject door in roomHandler.doors)
            {
                if (door.transform.position == new Vector3(xMid, yMid, door.transform.position.z)) { pass = true; roomHandler.updateActiveRoom(endPosition); }
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

        if (endPosition == roomHandler.starPos) { roomHandler.WinGame(); }

        isMoving = false;
    }
}
