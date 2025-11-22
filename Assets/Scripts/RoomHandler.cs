using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomHandler : MonoBehaviour
{
    Player player;
    Countdown timer;

    [SerializeField] public bool enableFOV, enableTimer;

    [Header("Rooms")]
    [SerializeField] RoomTiles[] rooms;
    GameObject activeRoom;
    public List<GameObject> doors = new List<GameObject>();
    [SerializeField][Range(6, 12)] int maxRoomWidth;
    [SerializeField][Range(6, 12)] int maxRoomHeight;
    int roomWidth, roomHeight;
    int playX, playY;
    int roomNo;

    public Vector2 activeBorder1, activeBorder2;
    List<Vector4> roomBorders = new List<Vector4>(); //2x Vector2

    [Header("Cow")]
    [SerializeField] GameObject cow;
    GameObject activeCow;
    public Vector2 cowPos;
    [SerializeField] TextMeshProUGUI cowText;
    int round = 0;
    [SerializeField] TextMeshProUGUI roundText;

    [Space][Space]
    [SerializeField] GameObject winText;
    [SerializeField] GameObject loseText;
    public bool pendingRound;

    [SerializeField] GameObject background;

    private void Awake()
    {
        player = FindAnyObjectByType<Player>();
        timer = FindAnyObjectByType<Countdown>();

        winText.SetActive(false);
        loseText.SetActive(false);

        activeRoom = rooms[0].gameObject; //livingRoom

        randRoomAndDoors();

        if (enableFOV) background.SetActive(true); //torch component
    }

    private void Start()
    {
        StartNewRound();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R)) { SceneManager.LoadScene(1); }

        if (pendingRound && (Input.GetKeyUp(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Space))) StartNewRound();
    }

    void randRoomAndDoors()
    {
        /* randomise the room size for each room, using 6 as a minimum width and height.
           uses + 1 on second parameter due to max exclusive */
        for (int i = 0; i < rooms.Length; i++)
        {
            roomWidth = 6 + Random.Range(0, maxRoomWidth - 6 + 1);
            roomHeight = 6 + Random.Range(0, maxRoomHeight - 6 + 1);

            roomBorders.Add(rooms[i].randomiseSize(roomWidth, roomHeight)); //changes room size and returns Vector4 as result

            //choose location for doors
            foreach (GameObject d in rooms[i].door)
            {
                DoorPath dp = d.GetComponent<DoorPath>();
                if (dp.xDir == -1)
                    d.transform.position = new Vector2(d.transform.position.x, Random.Range((int)d.transform.position.y - roomHeight + 1, (int)d.transform.position.y + 1));
                else if (dp.xDir == 1)
                    d.transform.position = new Vector2(d.transform.position.x + roomWidth - 1, Random.Range((int)d.transform.position.y - roomHeight + 1, (int)d.transform.position.y + 1));
                else if (dp.yDir == -1)
                    d.transform.position = new Vector2(Random.Range((int)d.transform.position.x, (int)d.transform.position.x + roomWidth - 1), d.transform.position.y - roomHeight + 1);
                else if (dp.yDir == 1)
                    d.transform.position = new Vector2(Random.Range((int)d.transform.position.x, (int)d.transform.position.x + roomWidth - 1), d.transform.position.y);
            }
        }

        activeBorder1 = new Vector2(roomBorders[0][0], roomBorders[0][3]); //top left border
        activeBorder2 = new Vector2(roomBorders[0][1], roomBorders[0][2]); //bottom right border

        //set player spawn point to somewhere in middle of the room
        playX = Random.Range((int)activeBorder1.x + 2, (int)activeBorder2.x - 1); //max exclusive
        playY = Random.Range((int)activeBorder2.y + 2, (int)activeBorder1.y - 1);
        player.transform.position = new Vector2(playX, playY);
    }

    public void updateActiveRoom(Vector2 pos)
    {
        //check each rooms borders to see which room the player is in and then activate that room and reactivate the previous one
        for (int i = 0; i < rooms.Length; i++)
        {
            if (pos.x > roomBorders[i][0] && pos.x < roomBorders[i][1] &&
                pos.y > roomBorders[i][2] && pos.y < roomBorders[i][3])
                roomNo = i;
        }

        activeRoom = rooms[roomNo].gameObject;

        activeBorder1 = new Vector2(roomBorders[roomNo][0], roomBorders[roomNo][3]);
        activeBorder2 = new Vector2(roomBorders[roomNo][1], roomBorders[roomNo][2]);
    }

    void StartNewRound()
    {
        pendingRound = false;
        winText.SetActive(false);
        timer.ResetTimer();
        round++;
        roundText.text = $"Round {round.ToString()}";

        //spawn cow in random location
        int randRoom = Random.Range(0, rooms.Length);
        while (rooms[randRoom].name == activeRoom.name) { randRoom = Random.Range(0, rooms.Length); }
        int randPosX = Random.Range((int)(roomBorders[randRoom][0] + 0.5f), (int)(roomBorders[randRoom][1] + 0.5f)); //uses +0.5f on second parameter as it is maxExlusive
        int randPosY = Random.Range((int)(roomBorders[randRoom][2] + 0.5f), (int)(roomBorders[randRoom][3] + 0.5f));
        cowPos = new Vector2(randPosX, randPosY);
        activeCow = Instantiate(cow, cowPos, Quaternion.identity);
        cowText.text = $"The cow is in the {rooms[randRoom].name}";
    }

    public void WinGame()
    {
        print("<color=green>Woohoo!");
        Destroy(activeCow);
        winText.SetActive(true);
        pendingRound = true;
        //Destroy(timer);
        //Destroy(player);
    }

    public void LoseGame()
    {
        print("<color=red>Loser...");
        loseText.SetActive(true);
        Destroy(timer);
        Destroy(player);
    }
}
