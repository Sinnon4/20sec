using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Human : MonoBehaviour
{
    RoomHandler roomHandler;
    GameObject player;

    public GameObject room;
    [SerializeField] public Light2D humanLight, torchLight;
    [SerializeField] GameObject torch, hand;
    Vector3 torchPos;
    Animator anim;

    [SerializeField] public float speed;
    float moveDuration;
    float p_x, p_y;
    bool isMoving;
    bool failedMovement;

    public float delay;
    public bool chase = true;

    [SerializeField] AudioClip movementClip;
    public AudioSource source;

    private void Awake()
    {
        roomHandler = FindAnyObjectByType<RoomHandler>();
        player = FindAnyObjectByType<Player>().gameObject; //use gameobject so that we can destroy player component when dead
        source = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();

        torchPos = torch.transform.localPosition;

        anim.SetBool("upCheck", false);
        anim.SetBool("downCheck", false);
        anim.SetBool("leftCheck", false);
        anim.SetBool("rightCheck", false);
    }

    private void FixedUpdate()
    {
        if (chase)
        {
            if (delay > 0) delay -= Time.deltaTime;
            else
            {
                p_x = player.transform.position.x;
                p_y = player.transform.position.y;
                if (roomHandler.rooms[roomHandler.activeRoom].gameObject == room)
                {
                    if (failedMovement)
                    {
                        if (Mathf.Abs(p_x - transform.position.x) > Mathf.Abs(p_y - transform.position.y))
                        {
                            if (p_y > transform.position.y)
                            {
                                LeanTween.moveLocal(torch, torchPos, 0.1f);
                                LeanTween.rotateZ(hand, 0, 0.1f);
                                StartCoroutine(Move(Vector2.up, "upCheck"));
                            }
                            else
                            {
                                LeanTween.moveLocal(torch, torchPos, 0.1f);
                                LeanTween.rotateZ(hand, 180, 0.1f);
                                StartCoroutine(Move(Vector2.down, "downCheck"));
                            }
                        }
                        else
                        {
                            if (p_x > transform.position.x)
                            {
                                LeanTween.moveLocal(torch, new Vector3(0, 0.2f, torchLight.transform.position.z), 0.1f);
                                LeanTween.rotateZ(hand, -90, 0.1f);
                                StartCoroutine(Move(Vector2.right, "rightCheck"));
                            }
                            else
                            {
                                LeanTween.moveLocal(torch, new Vector3(0, 0.2f, torchLight.transform.position.z), 0.1f);
                                LeanTween.rotateZ(hand, 90, 0.1f);
                                StartCoroutine(Move(Vector2.left, "leftCheck"));
                            }
                        }
                    }
                    else if (!isMoving)
                    {
                        if (Mathf.Abs(p_x - transform.position.x) > Mathf.Abs(p_y - transform.position.y))
                        {
                            if (p_x > transform.position.x)
                            {
                                LeanTween.moveLocal(torch, new Vector3(0, 0.2f, torchLight.transform.position.z), 0.1f);
                                LeanTween.rotateZ(hand, -90, 0.1f);
                                StartCoroutine(Move(Vector2.right, "rightCheck"));
                            }
                            else
                            {
                                LeanTween.moveLocal(torch, new Vector3(0, 0.2f, torchLight.transform.position.z), 0.1f);
                                LeanTween.rotateZ(hand, 90, 0.1f);
                                StartCoroutine(Move(Vector2.left, "leftCheck"));
                            }
                        }
                        else
                        {
                            if (p_y > transform.position.y)
                            {
                                LeanTween.moveLocal(torch, torchPos, 0.1f);
                                LeanTween.rotateZ(hand, 0, 0.1f);
                                StartCoroutine(Move(Vector2.up, "upCheck"));
                            }
                            else
                            {
                                LeanTween.moveLocal(torch, torchPos, 0.1f);
                                LeanTween.rotateZ(hand, 180, 0.1f);
                                StartCoroutine(Move(Vector2.down, "downCheck"));
                            }
                        }
                    }
                }
            }
        }
    }

    private IEnumerator Move(Vector2 dir, string animBool)
    {
        Vector2 startPosition = transform.position;
        Vector2 endPosition = startPosition + dir;

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

        if (dir == Vector2.up)
        {
            anim.SetBool("downCheck", false);
            anim.SetBool("leftCheck", false);
            anim.SetBool("rightCheck", false);
        }
        else if (dir == Vector2.down)
        {
            anim.SetBool("upCheck", false);
            anim.SetBool("leftCheck", false);
            anim.SetBool("rightCheck", false);
        }
        else if (dir == Vector2.right)
        {
            anim.SetBool("upCheck", false);
            anim.SetBool("leftCheck", false);
            anim.SetBool("downCheck", false);
        }
        else if (dir == Vector2.left)
        {
            anim.SetBool("upCheck", false);
            anim.SetBool("downCheck", false);
            anim.SetBool("rightCheck", false);
        }
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

        isMoving = false;
        source.Stop();
    }
}
