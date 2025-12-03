using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Player : MonoBehaviour
{
    RoomHandler roomHandler;

    [SerializeField] float speed = 1;
    float moveDuration;
    bool isMoving = false;
    bool pass;
    int doorNo;

    [SerializeField] public Transform hand;
    [SerializeField] GameObject torch;
    public Light2D torchLight;
    float torchPos;
    [SerializeField] float rotateTime;
    bool torchDied;

    public bool hasCow = false;

    public Animator anim;

    [Header("Sounds")]
    [SerializeField] AudioClip openDoorClip;
    [SerializeField] AudioClip
        backyardMovementClip,
        movementClip,
        torchOffClip;
    public AudioSource source;

    private void Awake()
    {
        roomHandler = FindAnyObjectByType<RoomHandler>();
        print("add death animation");
        torchLight = torch.GetComponentInChildren<Light2D>();
        torchPos = torch.transform.localPosition.x;

        anim = GetComponent<Animator>();
        source = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!isMoving && roomHandler.started && !roomHandler.pendingRound)
        {
            anim.SetBool("upCheck", false);
            anim.SetBool("downCheck", false);
            anim.SetBool("leftCheck", false);
            anim.SetBool("rightCheck", false);

            System.Func<KeyCode, bool> inputFunction;
            inputFunction = Input.GetKey;

            if (inputFunction(KeyCode.UpArrow) || inputFunction(KeyCode.W))
            {
                LeanTween.rotateZ(hand.gameObject, 0, rotateTime);
                LeanTween.moveLocalX(torch, torchPos, rotateTime);
                StartCoroutine(Move(Vector2.up, "upCheck"));
            }
            else if (inputFunction(KeyCode.DownArrow) || inputFunction(KeyCode.S))
            {
                LeanTween.rotateZ(hand.gameObject, 180, rotateTime);
                LeanTween.moveLocalX(torch, torchPos, rotateTime);
                StartCoroutine(Move(Vector2.down, "downCheck"));
            }
            else if (inputFunction(KeyCode.LeftArrow) || inputFunction(KeyCode.A))
            {
                LeanTween.rotateZ(hand.gameObject, 90, rotateTime);
                LeanTween.moveLocalX(torch, -0.15f, rotateTime);
                StartCoroutine(Move(Vector2.left, "leftCheck"));
            }
            else if (inputFunction(KeyCode.RightArrow) || inputFunction(KeyCode.D))
            {
                LeanTween.rotateZ(hand.gameObject, -90, rotateTime);
                LeanTween.moveLocalX(torch, 0.15f, rotateTime);
                StartCoroutine(Move(Vector2.right, "rightCheck"));
            }
        }
    }

    private IEnumerator Move(Vector2 direction, string animBool)
    {
        pass = false;

        Vector2 startPosition = transform.position;
        Vector2 endPosition = startPosition + direction; //assumed grid size is 1 [otherwise, use (direction * gridSize) ]

        if (!roomHandler.isWithinRoom(endPosition, roomHandler.activeRoom))
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
        SoundManager.instance.PlayClip(movementClip, source, true);

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

        if (pass == true)
        {
            //SoundManager.instance.PlayClip(openDoorClip, source);

            DoorPath dp = roomHandler.doors[doorNo].GetComponent<DoorPath>();
            if (dp.destination == null) { print("<color=yellow>Locked! :O"); transform.position = startPosition; }
            else if (dp.xDir == 1) transform.position = dp.destination.transform.position + Vector3.right;
            else if (dp.xDir == -1) transform.position = dp.destination.transform.position + Vector3.left;
            else if (dp.yDir == 1) transform.position = dp.destination.transform.position + Vector3.up;
            else if (dp.yDir == -1) transform.position = dp.destination.transform.position + Vector3.down;

            roomHandler.updateActiveRoom(transform.position);
        }
        else if (endPosition == roomHandler.cowPos)
        {
            if (torchDied)
            {
                torchLight.enabled = true;
                torchDied = false;
                Human.instance.speed++;
            }
            roomHandler.grabCow();
        }
        
        //if (endPosition == (Vector2)roomHandler.activeSwitch.transform.position)
        //{
        //    torchOff();
        //    //DialogueHandler.instance.insertText()
        //}

        isMoving = false;
        source.Stop(); //check if messes with opening door sound
    }

    public void torchOff()
    {
        torchDied = true;
        StartCoroutine(flickerTorch());
        Human.instance.speed--;
    }

    IEnumerator flickerTorch()
    {
        torchLight.enabled = false;
        float sec;

        sec = Random.Range(0.2f, 0.5f);
        yield return new WaitForSeconds(sec);
        torchLight.enabled = true;

        sec = Random.Range(0.4f, 0.7f);
        yield return new WaitForSeconds(sec);
        torchLight.enabled = false;

        sec = Random.Range(0.5f, 0.8f);
        yield return new WaitForSeconds(sec);
        torchLight.enabled = true;

        sec = Random.Range(0.1f, 0.4f);
        yield return new WaitForSeconds(sec);
        torchLight.enabled = false;

        sec = Random.Range(0.2f, 0.3f);
        yield return new WaitForSeconds(sec);
        torchLight.enabled = true;

        sec = Random.Range(0.1f, 0.2f);
        yield return new WaitForSeconds(sec);
        torchLight.enabled = false;

        yield return null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == roomHandler.human)
        {
            if (roomHandler.enableDeath)
            {
                source.Stop();
                roomHandler.LoseGame();
            }
        }
    }
}
