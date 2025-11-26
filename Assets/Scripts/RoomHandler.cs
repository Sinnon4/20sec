using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.PlayerSettings;

public class RoomHandler : MonoBehaviour
{
    Player player;
    Countdown timer;

    [SerializeField] public bool enableFOV, enableTimer;

    [Header("Rooms")]
    [SerializeField] RoomTiles[] rooms;
    public GameObject activeRoom;
    public List<GameObject> doors = new List<GameObject>();
    [SerializeField][Range(6, 12)] int maxRoomWidth;
    [SerializeField][Range(6, 12)] int maxRoomHeight;
    int roomWidth, roomHeight;
    int playX, playY;
    int roomNo;
    Vector2 rnd;

    public Vector2 activeBorder1, activeBorder2;
    List<Vector4> roomBorders = new List<Vector4>(); //2x Vector2

    [Header("Cow")]
    [SerializeField] GameObject cow;
    GameObject activeCow;
    public Vector2 cowPos;
    [SerializeField] TextMeshProUGUI cowText;
    int cows = -1;
    [SerializeField] TextMeshProUGUI cowCount;

    [SerializeField] GameObject human;
    int hRoom, hPrevRoom;
    Vector2 hSpawn;
    [SerializeField] TextMeshProUGUI hText;

    [Space][Space]
    [SerializeField] GameObject winText;
    [SerializeField] GameObject loseText;
    public bool pendingRound;

    [SerializeField] GameObject item;
    public List<GameObject> items = new List<GameObject>();
    int LRcounter = 0;

    [SerializeField] GameObject background;

    private void Awake()
    {
        player = FindAnyObjectByType<Player>();
        timer = FindAnyObjectByType<Countdown>();

        winText.SetActive(false);
        loseText.SetActive(false);

        activeRoom = rooms[0].gameObject; //BackYard

        randRoomAndDoors();

        if (enableFOV) background.SetActive(true); //torch component
    }

    private void Start()
    {
        print("light switch trap and text prompts/updates/dialogue");
        human = Instantiate(human, Vector2.one*100, Quaternion.identity); //prespawn human out of map
        StartNewRound();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R)) { SceneManager.LoadScene(1); } //reload

        if (Input.GetKeyUp(KeyCode.T)) { WinGame(); } //instant win

        if (pendingRound && (Input.GetKeyUp(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Space))) StartNewRound();

        if (player.hasCow && activeRoom == rooms[0].gameObject && player.transform.position.y == activeBorder1.y - 0.5f) WinGame();
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
        playY = (int)(activeBorder1.y - 0.5f); //make player spawn along top fince line in BackYard
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

        if (roomNo == hRoom) //reset human if enter same room
        {
            hSpawn = randz(hRoom);

            while (checkTiles("Human", hSpawn)) { hSpawn = randz(hRoom); }

            human.transform.position = hSpawn;
        }
    }

    void StartNewRound()
    {
        pendingRound = false;
        winText.SetActive(false);
        timer.ResetTimer();
        cows++;
        cowCount.text = $"Specimens Acquired: {cows.ToString()}";

        //destroy and remove existing items
        if (items.Count > 1)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                Destroy(items[i]);
                items.Remove(items[i]);
            }
        }

        //spawn random items in every room
        int n;
        for (int i = 1; i < rooms.Length; i++)
        {
            rnd = randz(i);
            n = 0;
            while (n <= 50)
            {
                while (checkTiles("Item", rnd))
                {
                    if (n > 50) { print($"<color=orange>No space for item placement in {activeRoom}"); break; }
                    n++;
                    rnd = randz(i);
                }

                if (n <= 50)
                {
                    items.Add(Instantiate(item, rnd, Quaternion.identity, transform));
                }
            }
        }


        //spawn cow in random location (minimise amount of times can spawn in living room)
        int randRoom;
        if (LRcounter == 0) randRoom = Random.Range(1, rooms.Length); //exclude BackYard
        else randRoom = Random.Range(2, rooms.Length); //exclude LivingRoom

        if (randRoom == 1) LRcounter = 4; //cannot be in LivingRoom again for 4 rounds

        rnd = randz(randRoom);

        while (checkTiles("Cow", rnd)) { rnd = randz(randRoom); }

        cowPos = rnd;
        activeCow = Instantiate(cow, cowPos, Quaternion.identity);
        cowText.text = $"The cow is in the {rooms[randRoom].name}";


        //relocate human into random room
        hPrevRoom = hRoom;
        while (hRoom == hPrevRoom) { hRoom = Random.Range(1, rooms.Length); }
        human.GetComponent<Human>().room = rooms[hRoom].gameObject;

        //if (hRoom == randRoom) { hText.text = $"The human is also in the {rooms[hRoom].name}"; }
        //else { hText.text = $"The human is in the {rooms[hRoom].name}"; }
        hText.text = $"The human is in the {rooms[hRoom].name}";
    }

    Vector2 randz(int room)
    {
        int randPosX = Random.Range((int)(roomBorders[room][0] + 0.5f), (int)(roomBorders[room][1] + 0.5f));
        int randPosY = Random.Range((int)(roomBorders[room][2] + 0.5f), (int)(roomBorders[room][3] + 0.5f));
        return new Vector2(randPosX, randPosY);
    }

    bool checkTiles(string type, Vector2 v2)
    {
        float distAway;

        //cow cannot spawn on an item or within 2 tiles of doors
        if (type == "Cow")
        {
            foreach (GameObject item_ in items)
            {
                if ((Vector2)item_.transform.position == v2) return true;
            }

            distAway = 2;
            foreach (GameObject d in doors)
            {
                Vector2 dPos = d.transform.position;
                if (dPos.x >= v2.x - distAway && dPos.x <= v2.x + distAway &&
                        dPos.y >= v2.y - distAway && dPos.y <= v2.y + distAway) { return true; }
            }
        }

        //human cannot spawn on an item or within 4 tiles of player when entering same room
        else if (type == "Human")
        {
            foreach (GameObject item_ in items)
            {
                if ((Vector2)item_.transform.position == v2) return true;
            }

            distAway = 4;
            Vector2 pPos = player.transform.position;
            if (pPos.x >= v2.x - distAway && pPos.x <= v2.x + distAway &&
                        pPos.y >= v2.y - distAway && pPos.y <= v2.y + distAway) { return true; }
        }

        //items cannot spawn on other items or within 1 tile of another item or doors
        else if (type == "Item")
        {
            distAway = 1;
            foreach (GameObject item_ in items)
            {
                Vector2 pos = item_.transform.position;
                //check adjacent tiles for item
                if (pos.x >= v2.x - distAway && pos.x <= v2.x + distAway &&
                    pos.y >= v2.y - distAway && pos.y <= v2.y + distAway) return true;
            }

            foreach (GameObject d in doors)
            {
                //do not place in front of door
                Vector2 dPos = d.transform.position;
                if (dPos.x >= v2.x - distAway && dPos.x <= v2.x + distAway &&
                        dPos.y >= v2.y - distAway && dPos.y <= v2.y + distAway) return true;
            }
        }

        //avoid infinite while loop if wrong entry
        else
        {
            Debug.LogError("WRONG STRING ENTERED.");
            return true;
        }

            return false;
    }

    public void grabCow()
    {
        print("<color=yellow>Cow acquired");
        Destroy(activeCow);
        player.hasCow = true;
        cowText.text = "Get the cow through the BackYard fence!";
    }

    public void WinGame()
    {
        print("<color=green>Woohoo!");
        player.hasCow = false;
        winText.SetActive(true);
        pendingRound = true;
    }

    public void LoseGame()
    {
        print("<color=red>Loser...");
        loseText.SetActive(true);
        Destroy(timer);
        Destroy(player);
    }
}
