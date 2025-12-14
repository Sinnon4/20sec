using NUnit.Framework.Constraints;
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

    [SerializeField] public GameObject hand;
    [SerializeField] GameObject torch;
    public Light2D torchLight;
    Vector3 torchPos;
    [SerializeField] float rotateTime;
    bool torchDied;
    public float intensity;

    public bool hasCow = false;

    public Animator anim;

    [Header("Sounds")]
    [SerializeField] AudioClip torchOffClip;
    [SerializeField] AudioClip
        movementOutsideClip,
        movementInsideClip;
    public AudioSource source;

    private void Awake()
    {
        roomHandler = FindAnyObjectByType<RoomHandler>();
        
        torchLight = torch.GetComponentInChildren<Light2D>();
        torchPos = torch.transform.localPosition;
        intensity = torchLight.intensity;

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
                LeanTween.rotateZ(hand, 0, rotateTime);
                LeanTween.moveLocal(torch, torchPos, rotateTime);
                StartCoroutine(Move(Vector2.up, "upCheck"));
            }
            else if (inputFunction(KeyCode.DownArrow) || inputFunction(KeyCode.S))
            {
                LeanTween.rotateZ(hand, 180, rotateTime);
                LeanTween.moveLocal(torch, torchPos, rotateTime);
                StartCoroutine(Move(Vector2.down, "downCheck"));
            }
            else if (inputFunction(KeyCode.LeftArrow) || inputFunction(KeyCode.A))
            {
                LeanTween.rotateZ(hand, 90, rotateTime);
                LeanTween.moveLocal(torch, new Vector3(0, 0.08f, torch.transform.position.z), rotateTime);
                StartCoroutine(Move(Vector2.left, "leftCheck"));
            }
            else if (inputFunction(KeyCode.RightArrow) || inputFunction(KeyCode.D))
            {
                LeanTween.rotateZ(hand, -90, rotateTime);
                LeanTween.moveLocal(torch, new Vector3(0, 0.08f, torch.transform.position.z), rotateTime);
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
        if (roomHandler.activeRoom < 2) SoundManager.instance.PlayClip(movementOutsideClip, source, true, 1, Random.Range(0.9f, 1.1f));
        else SoundManager.instance.PlayClip(movementInsideClip, source, true, 1, Random.Range(0.9f,1.1f));

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
                torchLight.intensity = intensity;
                torchDied = false;
                roomHandler.humans[0].speed++;
                roomHandler.humans[1].speed++;
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
        roomHandler.humans[0].speed--;
        roomHandler.humans[1].speed--;
    }

    IEnumerator flickerTorch()
    {
        torchLight.intensity = 0.4f;
        float sec;

        sec = Random.Range(0.2f, 0.5f);
        yield return new WaitForSeconds(sec);
        torchLight.intensity = intensity;

        sec = Random.Range(0.4f, 0.7f);
        yield return new WaitForSeconds(sec);
        torchLight.intensity = 0.4f;

        sec = Random.Range(0.5f, 0.8f);
        yield return new WaitForSeconds(sec);
        torchLight.intensity = intensity;

        sec = Random.Range(0.1f, 0.4f);
        yield return new WaitForSeconds(sec);
        torchLight.intensity = 0.1f;

        sec = Random.Range(0.2f, 0.3f);
        yield return new WaitForSeconds(sec);
        torchLight.intensity = intensity;

        sec = Random.Range(0.1f, 0.2f);
        yield return new WaitForSeconds(sec);
        torchLight.intensity = 0.4f;
        torchLight.pointLightOuterRadius = 1.8f;

        yield return true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Human h))
        {
            if (roomHandler.enableDeath)
            {
                source.Stop();
                roomHandler.LoseGame();
            }
        }
    }
}
