using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    RoomHandler roomHandler;

    [SerializeField] private float moveDuration = 0.5f;
    bool isMoving = false;

    //[SerializeField] Transform FOV;
    [SerializeField] Transform torch;

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
                //FOV.transform.eulerAngles = new Vector3 (0,0,90);
                torch.transform.eulerAngles = new Vector3(0, 0, 0);
                StartCoroutine(Move(Vector2.up));
            }
            else if (inputFunction(KeyCode.DownArrow) || inputFunction(KeyCode.S))
            {
                //FOV.transform.eulerAngles = new Vector3(0, 0, -90);
                torch.transform.eulerAngles = new Vector3(0, 0, 180);
                StartCoroutine(Move(Vector2.down));
            }
            else if (inputFunction(KeyCode.LeftArrow) || inputFunction(KeyCode.A))
            {
                //FOV.transform.eulerAngles = new Vector3(0, 0, 180);
                torch.transform.eulerAngles = new Vector3(0, 0, 90);
                StartCoroutine(Move(Vector2.left));
            }
            else if (inputFunction(KeyCode.RightArrow) || inputFunction(KeyCode.D))
            {
                //FOV.transform.eulerAngles = new Vector3(0, 0, 0);
                torch.transform.eulerAngles = new Vector3(0, 0, -90);
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
