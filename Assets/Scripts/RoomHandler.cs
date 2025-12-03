using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomHandler : MonoBehaviour
{
    Player player;
    Countdown timer;
    DialogueHandler dLog;

    [Header("Game Options")]
    [SerializeField] public bool enableTimer;
    [SerializeField] public bool enableDeath, enableLogs;

    [Header("Rooms")]
    [SerializeField] public RoomTiles[] rooms;
    public int activeRoom;
    public List<GameObject> doors = new();
    [SerializeField][Range(6, 12)] int maxRoomWidth;
    [SerializeField][Range(6, 12)] int maxRoomHeight;
    int roomWidth, roomHeight;
    int playX, playY;
    int roomNo;
    Vector2 rnd;

    Vector2 activeBorder1, activeBorder2;
    List<Vector4> roomBorders = new(); //2x Vector2

    [Header("Cow")]
    [SerializeField] GameObject cow;
    GameObject activeCow;
    public int cowRoom;
    public Vector2 cowPos;
    [SerializeField] TextMeshProUGUI cowText;
    int cows = -1;
    [SerializeField] TextMeshProUGUI cowCount;

    [Header("Human")]
    [SerializeField] public GameObject human;
    public int hRoom;
    int hPrevRoom;
    Vector2 hSpawn;
    [SerializeField] TextMeshProUGUI hText;
    int humanHideRound;
    public bool hideHuman;

    [Space][Space]
    public bool isDead;
    public bool pendingRound;
    [SerializeField] GameObject UFO, lightPole, fastForward;

    [SerializeField] GameObject item;
    public List<GameObject> items = new();
    int LRcounter = 0;

    [SerializeField] GameObject background, startScreenBlack, startScreen;
    [SerializeField] float startScreenFadeTime = 5;
    public bool started;

    MinimapTracker minimap;

    //[SerializeField] GameObject lightSwitch;
    //public GameObject activeSwitch;
    //Vector2 targetPos; //used in placing light switch - could not use local variable outside if statements

    [SerializeField] int torchOffRounds = 5;
    int torchOffRound;

    [Header("Prompts")]
    [SerializeField] string[] startGamePrompts;
    [SerializeField] string[] startGameColors,
        enterHumanRoomPrompts, enterHumanRoomColors,
        grabCowPrompts, grabCowColors,
        lowTimePrompts, lowTimeColors,
        outOfTimePrompts, outOfTimeColors,
        torchDiedPrompts, torchDiedColors,
        batteriesAcquiredPrompts, batteriesAcquiredColors,
        randomlyTimedPrompts, randomlyTimedColors,
        diedPrompts, diedColors;
    float randPrompt;
    bool check;

    [Header("Sounds")]
    [SerializeField] AudioClip UFOClip;
    [SerializeField] AudioClip
        abductClip,
        grabCowClip,
        loseClip;
    AudioSource source;

    private void Awake()
    {
        startScreenBlack.SetActive(true);
        startScreen.SetActive(true);

        player = FindAnyObjectByType<Player>();
        timer = FindAnyObjectByType<Countdown>();
        dLog = FindAnyObjectByType<DialogueHandler>();
        minimap = FindAnyObjectByType<MinimapTracker>();
        source = GetComponent<AudioSource>();

        activeRoom = 0; //BackYard

        randRoomAndDoors();
        background.SetActive(true);

        humanHideRound = Random.Range(2, 5); //hide the human occasionally

        torchOffRound = torchOffRounds;

        randPrompt = Random.Range(300, 1000);
    }

    private void Start()
    {
        print("add more rooms?");
        human = Instantiate(human, Vector2.one*100, Quaternion.identity); //prespawn human out of map
        startScreenBlack.LeanAlpha(0, startScreenFadeTime); //fade in scene
    }

    void Update()
    {
        if (!started && Input.anyKeyDown) startGame();
        else if (started)
        {
            if (pendingRound)
            {
                if (Input.GetKey(KeyCode.Space)) Time.timeScale = 2;
                else Time.timeScale = 1;
            }

            randPrompt--;
            if (randPrompt < 0 && !pendingRound && !isDead) randPrompt = dLog.insertText(randomlyTimedPrompts, randomlyTimedColors);

            if (timer.t < 5 && !check)
            {
                randPrompt = dLog.insertText(lowTimePrompts, lowTimeColors);
                check = true;
            }

            if (Input.GetKeyUp(KeyCode.R)) { SceneManager.LoadScene(1); } //reload

            if (Input.GetKeyUp(KeyCode.T)) { WinGame(); } //instant win

            if (!isDead && player.hasCow && activeRoom == 0 && player.transform.position.y == activeBorder1.y - 0.5f) WinGame();
        }
    }

    void startGame()
    {
        Destroy(startScreenBlack);
        started = true;
        startScreen.SetActive(false);
        StartNewRound();
    }

    public bool isWithinRoom(Vector2 pos, int room)
    {
        if (pos.x > roomBorders[room][0] && pos.x < roomBorders[room][1] &&
            pos.y > roomBorders[room][2] && pos.y < roomBorders[room][3]) return true;
        else return false;
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
        playX = Random.Range((int)(activeBorder1.x + 1.5f), (int)(activeBorder2.x - 0.5f)); //max exclusive
        playY = (int)(activeBorder1.y - 0.5f); //make player spawn along top fince line in BackYard
        player.transform.position = new Vector2(playX, playY);
    }

    void resetRooms()
    {
        roomBorders.Clear();
        roomBorders.Add(new Vector4(activeBorder1.x, activeBorder2.x, activeBorder2.y, activeBorder1.y));

        //exclude BackYard
        for (int i = 1; i < rooms.Length; i++)
        {
            //delete all except base tiles
            for (int j = rooms[i].tiles.Count - 1; j > 0; j--)
            {
                Destroy(rooms[i].tiles[j]);
                rooms[i].tiles.Remove(rooms[i].tiles[j]);
            }

            roomWidth = 6 + Random.Range(0, maxRoomWidth - 6 + 1);
            roomHeight = 6 + Random.Range(0, maxRoomHeight - 6 + 1);

            roomBorders.Add(rooms[i].randomiseSize(roomWidth, roomHeight)); //changes room size and returns Vector4 as result

            //choose location for doors
            foreach (GameObject d in rooms[i].door)
            {
                DoorPath dp = d.GetComponent<DoorPath>();
                if (dp.xDir == -1)
                    d.transform.position = new Vector2(roomBorders[i][0] - 0.5f, Random.Range((int)(roomBorders[i][2] + 0.5f), (int)(roomBorders[i][3] + 0.5f)));
                else if (dp.xDir == 1)
                    d.transform.position = new Vector2(roomBorders[i][1] + 0.5f, Random.Range((int)(roomBorders[i][2] + 0.5f), (int)(roomBorders[i][3] + 0.5f)));
                else if (dp.yDir == -1)
                    d.transform.position = new Vector2(Random.Range((int)(roomBorders[i][0] + 0.5f), (int)(roomBorders[i][1] + 0.5f)), roomBorders[i][2] - 0.5f);
                else if (dp.yDir == 1)
                    d.transform.position = new Vector2(Random.Range((int)(roomBorders[i][0] + 0.5f), (int)(roomBorders[i][1] + 0.5f)), roomBorders[i][3] + 0.5f);
            }
        }
    }

    public void updateActiveRoom(Vector2 pos)
    {
        //check each rooms borders to see which room the player is in and then activate that room and reactivate the previous one
        for (int i = 0; i < rooms.Length; i++)
        {
            if (isWithinRoom(pos,i)) roomNo = i;
        }

        activeRoom = roomNo;

        activeBorder1 = new Vector2(roomBorders[roomNo][0], roomBorders[roomNo][3]);
        activeBorder2 = new Vector2(roomBorders[roomNo][1], roomBorders[roomNo][2]);

        if (roomNo == hRoom) //reset human if enter same room
        {
            hSpawn = randz(hRoom);

            while (checkTiles("Human", hSpawn)) { hSpawn = randz(hRoom); }

            human.transform.position = hSpawn;

            randPrompt = dLog.insertText(enterHumanRoomPrompts, enterHumanRoomColors);
        }

        minimap.updateMap();
    }

    void StartNewRound()
    {
        timer.ResetTimer();
        cows++;
        cowCount.text = $"Specimens Acquired: {cows.ToString()}";

        if (humanHideRound == cows)
        {
            hideHuman = true;
            humanHideRound = Random.Range(cows + 3, cows + 7);
        }

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
                    if (n > 50) { if (enableLogs) { Debug.Log($"<color=orange>No space for item placement in {activeRoom}"); } break; }
                    n++;
                    rnd = randz(i);
                }

                if (n <= 50)
                {
                    items.Add(Instantiate(item, rnd, Quaternion.identity, transform));
                }
            }
        }


        //spawn hidden light switch trigger next to door
        //int r = Random.Range(2, rooms.Length); //exclude LivingRoom
        //placeLightSwitch(r);


        //spawn cow in random location (minimise amount of times can spawn in living room)
        if (cows == 0) cowRoom = 1; //put cow in LivingRoom for first round
        else if (LRcounter == 0) cowRoom = Random.Range(1, rooms.Length); //exclude BackYard
        else cowRoom = Random.Range(2, rooms.Length); //exclude LivingRoom

        if (cowRoom == 1) LRcounter = 4; //cannot be in LivingRoom again for 4 rounds

        rnd = randz(cowRoom);

        while (checkTiles("Cow", rnd)) { rnd = randz(cowRoom); }

        cowPos = rnd;
        activeCow = Instantiate(cow, cowPos, Quaternion.identity);
        cowText.text = $"The cow is in the <color=green><size=120%>{rooms[cowRoom].name}";


        //relocate human into random room
        hPrevRoom = hRoom;
        if (cows == 0) hRoom = Random.Range(2, rooms.Length); //exclude LivingRoom for first round
        else
        {
            while (hRoom == hPrevRoom) { hRoom = Random.Range(1, rooms.Length); }
        }
        human.transform.position = new Vector2(roomBorders[hRoom][0], roomBorders[hRoom][3]); //top left of room
        human.GetComponent<Human>().room = rooms[hRoom].gameObject;
        if (hideHuman) { hText.text = $"You are unsure where the human is..."; }
        else { hText.text = $"The human is in the <color=red><size=120%>{rooms[hRoom].name}"; }

        minimap.updateMap();
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

    //void placeLightSwitch(int room, bool redo = false)
    //{
    //    bool safe = true;
    //    Vector2 doorPos = rooms[room].door[0].transform.position;

    //    if (doorPos.x == roomBorders[room][0] - 0.5f) //left door
    //    {
    //        if (redo) targetPos = new Vector2(doorPos.x + 1, doorPos.y - 1); //try other side of door if failed first try
    //        else targetPos = new Vector2(doorPos.x + 1, doorPos.y + 1);
    //    }
    //    else if (doorPos.x == roomBorders[room][1] + 0.5f) //right door
    //    {
    //        if (redo) targetPos = new Vector2(doorPos.x - 1, doorPos.y - 1); //try other side of door if failed first try
    //        else targetPos = new Vector2(doorPos.x - 1, doorPos.y + 1);
    //    }
    //    else if (doorPos.y == roomBorders[room][2] - 0.5f) //bottom door
    //    {
    //        if (redo) targetPos = new Vector2(doorPos.x - 1, doorPos.y + 1); //try other side of door if failed first try
    //        else targetPos = new Vector2(doorPos.x + 1, doorPos.y + 1);
    //    }
    //    else if (doorPos.y == roomBorders[room][3] + 0.5f) //top door
    //    {
    //        if (redo) targetPos = new Vector2(doorPos.x - 1, doorPos.y - 1); //try other side of door if failed first try
    //        else targetPos = new Vector2(doorPos.x + 1, doorPos.y - 1);
    //    }

    //    //if (targetPos.x > roomBorders[room][0] && targetPos.x < roomBorders[room][1] &&
    //    //    targetPos.y > roomBorders[room][2] && targetPos.y < roomBorders[room][3]) safe = false;
    //    if (!isWithinRoom(targetPos, room)) safe = false;

    //    if (safe)
    //    {
    //        foreach (GameObject item_ in items)
    //        {
    //            if (targetPos == (Vector2)item_.transform.position) { safe = false; break; }
    //        }
    //    }

    //    if (safe) activeSwitch = Instantiate(lightSwitch, targetPos, Quaternion.identity);
    //    else if (!safe && !redo) placeLightSwitch(room, redo = true);
    //    else if (!safe && redo) Debug.Log($"<color=orange>Could not place light switch in {rooms[activeRoom].name}");
    //}

    public void grabCow()
    {
        if (!isDead)
        {
            print("<color=yellow>Cow acquired");
            SoundManager.instance.PlayClip(grabCowClip, source);
            Destroy(activeCow);
            cowPos = Vector2.positiveInfinity;
            player.hasCow = true;
            cowText.text = "Get the cow through the <color=green>BackYard fence.";
            cowRoom = -1; //used in minimap tracker
            if (cows > 0 && cows == torchOffRound - torchOffRounds) randPrompt = dLog.insertText(batteriesAcquiredPrompts, batteriesAcquiredColors);
            else randPrompt = dLog.insertText(grabCowPrompts, grabCowColors);
        }
    }

    public void WinGame()
    {
        if (hideHuman) hideHuman = false;

        print("<color=green>Woohoo!");
        SoundManager.instance.PlayClip(abductClip, source);
        player.hasCow = false;
        check = false;

        player.anim.SetBool("upCheck", false);
        player.anim.SetBool("downCheck", false);
        player.anim.SetBool("leftCheck", false);
        player.anim.SetBool("rightCheck", false);

        StartCoroutine(Abduct());
    }

    public void LoseGame(bool timeOut = false)
    {
        if (timeOut) dLog.insertText(outOfTimePrompts, outOfTimeColors);
        else dLog.insertText(diedPrompts, diedColors);

        print("<color=red>Loser...");
        player.source.Stop(); //if died while moving
        SoundManager.instance.PlayClip(loseClip, source);
        //loseText.SetActive(true);
        Destroy(timer);
        player.anim.SetBool("isDead", true);
        Destroy(player);
        isDead = true;
    }

    IEnumerator Abduct()
    {
        pendingRound = true;
        fastForward.SetActive(true);
        CameraFollow cam = FindAnyObjectByType<CameraFollow>();
        cam.enabled = false; //disable the camera following
        player.torchLight.enabled = false;

        UFO.transform.position = new Vector2(player.transform.position.x, UFO.transform.position.y); //line UFO up with player
        UFO.gameObject.LeanMoveY(UFO.transform.position.y - 3, 2).setEaseOutCubic(); //UFO descends
        //SoundManager.instance.PlayClip(UFOClip, source);
        yield return new WaitForSeconds(2);

        player.gameObject.LeanMoveY(player.transform.position.y + 5, 2).setEaseInQuad(); //player ascends
        SoundManager.instance.PlayClip(abductClip, source, false, 1, Random.Range(0.8f,1.2f));
        yield return new WaitForSeconds(2);

        UFO.gameObject.LeanMoveX(UFO.transform.position.x + 10, 2).setEaseInCirc(); //UFO goes right
        //SoundManager.instance.PlayClip(UFOClip, source);
        cam.gameObject.LeanMoveX(cam.transform.position.x + 20, 2).setEaseInCirc(); //camera pans right
        yield return new WaitForSeconds(2);

        cam.transform.position = new Vector3(cam.transform.position.x - 40, cam.transform.position.y, cam.transform.position.z); //teleport camera
        UFO.transform.position = new Vector2(Random.Range((int)(activeBorder1.x + 0.5f), (int)(activeBorder2.x + 0.5f)), UFO.transform.position.y); //set new player position with UFO
        player.transform.position = new Vector2(UFO.transform.position.x, player.transform.position.y);
        player.hand.localEulerAngles = new Vector3(0, 0, 180); //rotate torch to face downward
        lightPole.transform.position = new Vector2(Random.Range(activeBorder1.x, activeBorder2.x), lightPole.transform.position.y); //light post moves
        rooms[0].door[0].transform.position = new Vector2(Random.Range((int)(activeBorder1.x + 0.5f), (int)(activeBorder2.x + 0.5f)), rooms[0].door[0].transform.position.y); //reset random door pos
        resetRooms();
        StartNewRound();
        cam.gameObject.LeanMoveX(cam.transform.position.x + 20, 3).setEaseOutQuart(); //camera fades back onto BackYard
        yield return new WaitForSeconds(2);

        player.gameObject.LeanMoveY(player.transform.position.y - 5, 2).setEaseOutCubic(); //player descends
        SoundManager.instance.PlayClip(abductClip, source, false, 1, Random.Range(0.8f, 1.2f));
        yield return new WaitForSeconds(2);

        UFO.gameObject.LeanMoveY(UFO.transform.position.y + 3, 1).setEaseInBack(); //UFO ascends
        //SoundManager.instance.PlayClip(UFOClip, source);
        yield return new WaitForSeconds(1);

        player.torchLight.enabled = true;

        if (cows == torchOffRound)
        {
            player.torchOff();
            torchOffRound += torchOffRounds; //occurs every 5 rounds
            dLog.insertText(torchDiedPrompts, torchDiedColors);
        }
        else randPrompt = dLog.insertText(startGamePrompts, startGameColors);

        cam.enabled = true;
        fastForward.SetActive(false);
        pendingRound = false;
        Time.timeScale = 1;
        yield return null;
    }
}
