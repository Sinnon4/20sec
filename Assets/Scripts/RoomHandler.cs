using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomHandler : MonoBehaviour
{
    public static RoomHandler instance;
    Player player;
    Countdown timer;

    [Header("Level Options")]
    [SerializeField] public bool enableFOV;

    [Header("Rooms")]
    [SerializeField] GameObject room;
    [SerializeField] GameObject door;
    GameObject startRoom;
    GameObject activeRoom;
    List<GameObject> rooms = new List<GameObject>();
    public List<GameObject> doors = new List<GameObject>();
    [SerializeField][Range(6, 12)] int maxRoomWidth;
    [SerializeField][Range(6, 12)] int maxRoomHeight;
    [SerializeField] public int gridSize = 1; //Need to adjust for room dimensions if altered

    int roomX;
    int roomY;
    int roomWidth;
    int roomHeight;
    public Vector2 activeBorder1;
    public Vector2 activeBorder2;
    public List<Vector4> roomBorders = new List<Vector4>(); //2x Vector2

    int roomNo;

    [SerializeField] GameObject star;
    public Vector2 starPos;
    int starRoom;
    SpriteRenderer starSR;
    [SerializeField] GameObject winText;
    [SerializeField] GameObject loseText;

    [SerializeField] GameObject background;

    // rooms and roomBorders lists will have one more than doors list as they include startRoom

    private void Awake()
    {
        if (instance == null) instance = this;
        print("need to implement back wall pokemon style");
        player = FindAnyObjectByType<Player>();
        timer = FindAnyObjectByType<Countdown>();

        winText.SetActive(false);
        loseText.SetActive(false);

        startRoom = gameObject;
        rooms.Add(startRoom);

        //set the room position as somewhere from -1 to 1 (default room size is 5x5 - so any more would lead to spawning next to the wall)
        roomX = Random.Range(-1, 2); //accounts for max exclusive
        roomY = Random.Range(-1, 2); //accounts for max exclusive
        startRoom.transform.position = new Vector2(roomX, roomY);

        /*set the room size, using 5 as the minimum width and height.
         * random value (int) is multiplied by 2 so that the result is an even number, then adding to 5 will always give an odd number, so that this always aligns with the player grid.
         * uses +1 on second parameter due to max exclusive.
         */
        roomWidth = 5 + Random.Range(0, maxRoomWidth / 2 + 1) * 2;
        roomHeight = 5 + Random.Range(0, maxRoomHeight / 2 + 1) * 2;
        startRoom.transform.localScale = new Vector3(roomWidth, roomHeight, 0);

        activeBorder1 = new Vector2(startRoom.transform.position.x - (float)roomWidth / 2, startRoom.transform.position.y + (float)roomHeight / 2); //top left border
        activeBorder2 = new Vector2(startRoom.transform.position.x + (float)roomWidth / 2, startRoom.transform.position.y - (float)roomHeight / 2); //bottom right border
        roomBorders.Add(new Vector4(activeBorder1.x, activeBorder1.y, activeBorder2.x, activeBorder2.y));
        
        activeRoom = startRoom;

        if (enableFOV) background.SetActive(true);
    }

    private void Start()
    {
        spawnRooms();
        updateDoors();

        //spawn star in random location
        int randRoom = Random.Range(1, rooms.Count);
        int randPosX = Random.Range((int)(roomBorders[randRoom][0] + 0.5f), (int)(roomBorders[randRoom][2] + 0.5f)); //uses +0.5f on second parameter as it is maxExlusive
        int randPosY = Random.Range((int)(roomBorders[randRoom][3] + 0.5f), (int)(roomBorders[randRoom][1] + 0.5f));
        starPos = new Vector2(randPosX, randPosY);
        star = Instantiate(star, starPos, Quaternion.identity);
        starSR = star.GetComponent<SpriteRenderer>();
        starSR.enabled = false; //hide star (in other room) on spawn
        starRoom = randRoom;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R)) { SceneManager.LoadScene(1); }
    }

    private void spawnRooms()
    {
        int newX;
        int newY;

        //top
        newX = roomX;
        newY = roomY + roomHeight;
        spawnNewRoom(newX, newY);
        doors.Add(Instantiate(door, new Vector2((int)Random.Range(
                rooms[rooms.Count-1].transform.position.x + (roomWidth / 2 - 1),
                rooms[rooms.Count-1].transform.position.x - (roomWidth / 2 - 1)),
                rooms[rooms.Count-1].transform.position.y - (float)roomHeight / 2), Quaternion.identity));

        //bottom
        newX = roomX;
        newY = roomY - roomHeight;
        spawnNewRoom(newX, newY);
        doors.Add(Instantiate(door, new Vector2((int)Random.Range(
                rooms[rooms.Count-1].transform.position.x + (roomWidth / 2 - 1),
                rooms[rooms.Count-1].transform.position.x - (roomWidth / 2 - 1)),
                rooms[rooms.Count-1].transform.position.y + (float)roomHeight / 2), Quaternion.identity));

        //left
        newX = roomX - roomWidth;
        newY = roomY;
        spawnNewRoom(newX, newY);
        doors.Add(Instantiate(door,
            new Vector2(rooms[rooms.Count-1].transform.position.x + (float)roomWidth / 2, (int)Random.Range(
                rooms[rooms.Count-1].transform.position.y + (roomHeight / 2 - 1),
                rooms[rooms.Count-1].transform.position.y - (roomHeight / 2 - 1))), Quaternion.identity));
        doors[doors.Count-1].transform.rotation = Quaternion.Euler(0, 0, 90);

        //right
        newX = roomX + roomWidth;
        newY = roomY;
        spawnNewRoom(newX, newY);
        doors.Add(Instantiate(door,
            new Vector2(rooms[rooms.Count-1].transform.position.x - (float)roomWidth / 2, (int)Random.Range(
                rooms[rooms.Count-1].transform.position.y + (roomHeight / 2 - 1),
                rooms[rooms.Count-1].transform.position.y - (roomHeight / 2 - 1))), Quaternion.identity));
        doors[doors.Count-1].transform.rotation = Quaternion.Euler(0, 0, 90);
    }

    private void spawnNewRoom(int x, int y)
    {
        rooms.Add(Instantiate(room, new Vector2(x, y), Quaternion.identity));
        rooms[rooms.Count-1].transform.localScale = new Vector3(roomWidth, roomHeight, 0);
        roomBorders.Add(new Vector4(
            x - (float)roomWidth / 2, //left x - represented as roomBorders[i][0]
            y + (float)roomHeight / 2, //top y - represented as roomBorders[i][1]
            x + (float)roomWidth / 2, //right x - represented as roomBorders[i][2]
            y - (float)roomHeight / 2)); //bottom y - represented as roomBorders[i][3]
    }

    public void updateActiveRoom(Vector2 pos)
    {
        //check each rooms borders to see which room the player is in and then activate that room and reactivate the previous one
        for (int i = 0; i < rooms.Count; i++)
        {
            if (pos.x > roomBorders[i][0] && pos.x < roomBorders[i][2] &&
                pos.y > roomBorders[i][3] && pos.y < roomBorders[i][1])
            { roomNo = i;}
        }

        activeRoom.GetComponent<SpriteRenderer>().color = Color.black;
        activeRoom = rooms[roomNo];
        activeRoom.GetComponent<SpriteRenderer>().color = Color.white;

        activeBorder1 = new Vector2(roomBorders[roomNo][0], roomBorders[roomNo][1]);
        activeBorder2 = new Vector2(roomBorders[roomNo][2], roomBorders[roomNo][3]);

        if (roomNo == starRoom) starSR.enabled = true;
        else starSR.enabled = false;

        updateDoors();
    }

    void updateDoors()
    {
        //hide all doors outside of the active room
        foreach (GameObject door in doors)
        {
            if (door.transform.position.x >= activeBorder1.x && door.transform.position.x <= activeBorder2.x &&
                door.transform.position.y <= activeBorder1.y && door.transform.position.y >= activeBorder2.y)
            { door.GetComponent<SpriteRenderer>().color = Color.brown; }
            else { door.GetComponent<SpriteRenderer>().color = Color.black; }
        }
    }

    public void WinGame()
    {
        print("<color=yellow>Woohoo!");
        winText.SetActive(true);
        Destroy(timer);
        Destroy(player);
    }

    public void LoseGame()
    {
        print("<color=red>Loser...");
        loseText.SetActive(true);
        Destroy(timer);
        Destroy(player);
    }
}
