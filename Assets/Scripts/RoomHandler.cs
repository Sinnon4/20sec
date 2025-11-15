using System.Collections.Generic;
//using System.Security.Cryptography;
using UnityEngine;
//using UnityEngine.SceneManagement;

public class RoomHandler : MonoBehaviour
{
    public static RoomHandler instance;
    Player player;

    GameObject startRoom;
    public GameObject activeRoom;
    [SerializeField] GameObject room;
    [SerializeField] GameObject door;
    public List<GameObject> rooms = new List<GameObject>();
    public List<GameObject> doors = new List<GameObject>();
    [SerializeField][Range(6, 12)] int maxRoomWidth;
    [SerializeField][Range(6, 12)] int maxRoomHeight;

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

    // rooms and roomBorders lists will have one more than doors list as they include startRoom

    private void Awake()
    {
        if (instance == null) instance = this;

        player = FindAnyObjectByType<Player>();

        startRoom = gameObject;
        rooms.Add(startRoom);

        roomX = Random.Range(-2, 3); //accounts for max exclusive
        roomY = Random.Range(-2, 3); //accounts for max exclusive
        startRoom.transform.position = new Vector2(roomX, roomY);
        roomWidth = 5 + Random.Range(0, maxRoomWidth / 2 + 1) * 2; //+1 due to function being max exclusive, /2*2 due to grid needing to be on an odd number to align with player
        roomHeight = 5 + Random.Range(0, maxRoomHeight / 2 + 1) * 2; //+1 due to function being max exclusive, /2*2 due to grid needing to be on an odd number to align with player
        startRoom.transform.localScale = new Vector3(roomWidth, roomHeight, 0);

        activeBorder1 = new Vector2(startRoom.transform.position.x - (float)roomWidth / 2, startRoom.transform.position.y + (float)roomHeight / 2); //top left border
        activeBorder2 = new Vector2(startRoom.transform.position.x + (float)roomWidth / 2, startRoom.transform.position.y - (float)roomHeight / 2); //bottom right border
        roomBorders.Add(new Vector4(activeBorder1.x, activeBorder1.y, activeBorder2.x, activeBorder2.y));
        
        activeRoom = startRoom;
    }

    private void Start()
    {
        spawnRooms();

        //spawn star in random location
        int randRoom = Random.Range(1, rooms.Count);
        print(randRoom);
        int randPosX = (int)Random.Range(roomBorders[randRoom][0] + 1, roomBorders[randRoom][2]); //STILL NOT FIXED
        int randPosY = (int)Random.Range(roomBorders[randRoom][3] + 1, roomBorders[randRoom][1]);
        starPos = new Vector2(randPosX, randPosY);
        print($"x bounds: {roomBorders[randRoom][0]} {roomBorders[randRoom][2]}");
        star = Instantiate(star, starPos, Quaternion.identity); //look into bug - spawns in startRoom and also spawns outside of bounds of other rooms entirely
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
    }

    public void WinGame()
    {
        print("<color><yellow>Woohoo!");
        Destroy(player);
    }
}
