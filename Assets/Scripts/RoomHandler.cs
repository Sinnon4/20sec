using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class RoomHandler : MonoBehaviour
{
    Player player;
    Countdown timer;
    DialogueHandler dLog;
    [SerializeField] MinimapTracker minimap;

    [Header("Game Options")]
    [SerializeField] public bool enableTimer;
    [SerializeField] public bool enableDeath, enableLogs;

    [Header("Rooms")]
    [SerializeField] public RoomTiles[] rooms;
    /* 0 - Back Yard
     * 1 - Front Yard
     * 2 - Living Room
     * 3 - Kitchen
     * 4 - Bedroom 1
     * 5 - Bedroom 2
     * 6 - Bunker
     * 7 - Secret Room */
    public int activeRoom;
    public List<GameObject> doors = new();
    [SerializeField][Range(4, 12)] int minRoomSize;
    [SerializeField][Range(4, 12)] int maxRoomSize;
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
    int LRcounter = 0;

    [Header("Human")]
    [SerializeField] GameObject human;
    [SerializeField] int humansCount = 2;
    public List<Human> humans = new();
    public List<int> hRooms = new();
    List<int> hPrevRooms = new();
    [SerializeField] float humanDelayTime;
    [SerializeField] TextMeshProUGUI hText;
    int humanHideRound;
    public bool hideHuman;
    int max;

    [Space][Space]
    public bool isDead;
    public bool pendingRound;
    [SerializeField] GameObject gameOver;
    TextMeshProUGUI gameOverText;
    [SerializeField] GameObject UFO, lightPole, fastForward;

    [Header("UFO vectors")]
    [SerializeField] float UFOAscentTime = 2;
    [SerializeField] float
        playerAscentTime = 1,
        //UFOExitTime = 2,
        camExitTime = 1;

    [Header("Obstacles")]
    [SerializeField] GameObject item;
    [SerializeField] Sprite[] itemSprites;
    public List<GameObject> items = new();
    int itemCount;
    [SerializeField] int maxItems = 3;

    [Space][Space]
    public bool started;
    [SerializeField] GameObject background, startScreenBlack, startScreen;
    [SerializeField] float startScreenFadeTime = 5;

    [Header("Buttons")]
    [SerializeField] GameObject pauseButton;
    [SerializeField] GameObject
        buttons,
        continueButton;
    bool paused = false;

    [Space][Space]
    [SerializeField] int torchOffRounds = 2;
    int torchOffRound;
    [SerializeField] List<Light2D> allLights = new();

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
        doorClip,
        grabCowClip1, grabCowClip2,
        buttonClick,
        loseClip;
    AudioSource source;

    private void Awake()
    {
        startScreenBlack.SetActive(true);
        //startScreen.SetActive(true);
        pauseButton.SetActive(false);
        buttons.SetActive(false);

        player = FindAnyObjectByType<Player>();
        timer = FindAnyObjectByType<Countdown>();
        dLog = FindAnyObjectByType<DialogueHandler>();
        //minimap = FindAnyObjectByType<MinimapTracker>();
        source = GetComponent<AudioSource>();

        gameOverText = gameOver.GetComponentInChildren<TextMeshProUGUI>();

        activeRoom = 0; //BackYard

        cowText.gameObject.SetActive(true);
        minimap.gameObject.SetActive(true);

        randRoomAndDoors();
        background.SetActive(true);

        humanHideRound = Random.Range(2, 5); //hide the human occasionally

        foreach (Light2D light in allLights) light.enabled = true;

        torchOffRound = torchOffRounds;

        randPrompt = Random.Range(300, 1000);
    }

    private void Start()
    {
        //prespawn humans out of map
        for (int i = 0; i < humansCount; i++)
        {
            humans.Add(Instantiate(human, Vector2.one * 100, Quaternion.identity).GetComponent<Human>());
            hRooms.Add(-1);
            hPrevRooms.Add(-1);
            allLights.Add(humans[i].humanLight);
            allLights.Add(humans[i].torchLight);
        }

        startScreenBlack.LeanAlpha(0, startScreenFadeTime); //fade in scene
        StartNewRound();
    }

    void Update()
    {
        if (!started && Input.anyKeyDown) startGame();
        else if (started && !paused)
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

            if (!paused && Input.GetKeyDown(KeyCode.Space) && !isDead && !pendingRound) { Pause(); }
            else if (paused && Input.GetKeyDown(KeyCode.Space)) { Continue(); } //take out?

            if (Input.GetKeyDown(KeyCode.R)) { Restart(); } //reload

            if (Input.GetKeyDown(KeyCode.Escape)) { Quit(); } //close game

            if (Input.GetKeyDown(KeyCode.T)) { WinGame(); } //instant win

            if (Input.GetKeyDown(KeyCode.Y)) { foreach (Light2D light in allLights) light.enabled = false; } //turn lights on

            if (!isDead && player.hasCow && activeRoom == 0 && player.transform.position.y == activeBorder1.y - 0.5f) WinGame(); //top of BackYard
            else
            if (!isDead && player.hasCow && activeRoom == 1 && player.transform.position.y == activeBorder2.y + 0.5f) WinGame(true); //bottom of FrontYard

        }
    }

    void startGame()
    {
        //Destroy(startScreenBlack);
        started = true;
        //startScreen.SetActive(false);
        pauseButton.SetActive(true);
        cowText.transform.LeanMoveLocal(new Vector2(810, 390), 1);
        cowText.transform.LeanScale(Vector3.one, 1);
        minimap.transform.LeanMoveLocal(new Vector2(-660, -366), 1);
        minimap.transform.LeanScale(Vector3.one, 1);
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
            if (i < 2) roomHeight = Random.Range(3, Mathf.Max(4, maxRoomSize - 1)); //make FrontYard & BackYard small
            else roomHeight = Random.Range(minRoomSize, maxRoomSize + 1);
            roomWidth = Random.Range(minRoomSize, maxRoomSize + 1);

            if (i == 0) roomBorders.Add(rooms[i].randomiseSize(roomWidth, roomHeight, true)); //changes room size and returns Vector4 as result
            else roomBorders.Add(rooms[i].randomiseSize(roomWidth, roomHeight));

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
        //roomBorders.Add(new Vector4(activeBorder1.x, activeBorder2.x, activeBorder2.y, activeBorder1.y)); //going to cause an issue when going from frontyard to backyard
        print("front yard door not randomising");
        //exclude BackYard -- no
        for (int i = 0; i < rooms.Length; i++)
        {
            //delete all except base tiles
            for (int j = rooms[i].tiles.Count - 1; j > 0; j--)
            {
                Destroy(rooms[i].tiles[j]);
                rooms[i].tiles.Remove(rooms[i].tiles[j]);
            }

            if (i < 2) roomHeight = Random.Range(3, Mathf.Max(4, maxRoomSize - 1)); //make FrontYard & BackYard small
            else roomHeight = Random.Range(minRoomSize, maxRoomSize + 1);
            roomWidth = Random.Range(minRoomSize, maxRoomSize + 1);

            if (i == 0) roomBorders.Add(rooms[i].randomiseSize(roomWidth, roomHeight, true)); //changes room size and returns Vector4 as result
            else roomBorders.Add(rooms[i].randomiseSize(roomWidth, roomHeight));

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

        activeRoom = 0;
        activeBorder1 = new Vector2(roomBorders[0][0], roomBorders[0][3]);
        activeBorder2 = new Vector2(roomBorders[0][1], roomBorders[0][2]);
    }

    public void updateActiveRoom(Vector2 pos)
    {
        SoundManager.instance.PlayClip(doorClip, source);

        //check each rooms borders to see which room the player is in and then activate that room and deactivate the previous one
        for (int i = 0; i < rooms.Length; i++)
        {
            if (isWithinRoom(pos,i)) roomNo = i;
        }

        activeRoom = roomNo;

        activeBorder1 = new Vector2(roomBorders[roomNo][0], roomBorders[roomNo][3]);
        activeBorder2 = new Vector2(roomBorders[roomNo][1], roomBorders[roomNo][2]);

        for (int h = 0; h < humans.Count; h++)
        {
            if (roomNo == hRooms[h]) //reset human if enter same room
            {
                Vector2 hSpawn = randz(hRooms[h]);
                
                while (checkTiles("Human", hSpawn)) { hSpawn = randz(hRooms[h]); }
                
                humans[h].transform.position = hSpawn;
                humans[h].delay = humanDelayTime; //delay human advancing

                randPrompt = dLog.insertText(enterHumanRoomPrompts, enterHumanRoomColors);
            }
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
            humanHideRound = Random.Range(cows + 3, cows + 5);
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

        //spawn random items in every room (exclude BackYard and FrontYard)
        int n;
        for (int i = 2; i < rooms.Length; i++)
        {
            rnd = randz(i, true);
            n = 0;
            itemCount = 0;
            while (n <= 50 && itemCount < maxItems)
            {
                while (checkTiles("Item", rnd))
                {
                    if (n > 50) { if (enableLogs) { Debug.Log($"<color=orange>No space for item placement in {activeRoom}"); } break; }
                    n++;
                    rnd = randz(i, true);
                }

                if (n <= 50)
                {
                    items.Add(Instantiate(item, rnd, Quaternion.identity, transform));
                    items[items.Count - 1].GetComponent<SpriteRenderer>().sprite = itemSprites[Random.Range(0, itemSprites.Length)];
                    itemCount++;
                }
            }
        }

        //spawn cow in random location (minimise amount of times can spawn in living room)
        if (cows == 0) cowRoom = 2; //put cow in LivingRoom for first round
        else if (LRcounter == 0) cowRoom = Random.Range(2, rooms.Length); //exclude BackYard and FrontYard
        else cowRoom = Random.Range(3, rooms.Length); //exclude LivingRoom

        if (cowRoom == 1) LRcounter = 4; //cannot be in LivingRoom again for 4 rounds

        rnd = randz(cowRoom);
        
        while (checkTiles("Cow", rnd)) { rnd = randz(cowRoom); }
        
        cowPos = rnd;
        activeCow = Instantiate(cow, cowPos, Quaternion.identity);
        //cowText.text = $"The cow is in the <color=green><size=120%>{rooms[cowRoom].name}";

        for (int h = 0; h < humans.Count; h++)
        {
            //relocate human into random room
            hPrevRooms[h] = hRooms[h];
            if (cows == 0) hRooms[h] = h+3; //first round
            else
            {
                /* 0 - Back Yard
                 * 1 - Front Yard
                 * 2 - Living Room
                 * 3 - Kitchen
                 * 4 - Bedroom 1
                 * 5 - Bedroom 2
                 * 6 - Bunker
                 * 7 - Secret Room */
                if (cowRoom < 4) max = 6;
                else if (cowRoom == 4 || cowRoom == 5) max = 5;
                else if (cowRoom > 5) max = 7;
                
                while (hRooms[h] == hPrevRooms[h]) { hRooms[h] = Random.Range(2, max); } //exclude FrontYard
            }
            humans[h].transform.position = new Vector2(roomBorders[hRooms[h]][0], roomBorders[hRooms[h]][3]); //top left of room
            humans[h].room = rooms[hRooms[h]].gameObject;
            if (hideHuman) { hText.text = $"Humans status: <color=red>Unknown"; }
            else { hText.text = $"Humans status: Known"; }
        }

        minimap.updateMap();
    }

    Vector2 randz(int room, bool isItem = false)
    {
        int randPosX, randPosY;
        if (isItem) //avoid room edges
        {
            randPosX = Random.Range((int)(roomBorders[room][0] + 1.5f), (int)(roomBorders[room][1] - 0.5f));
            randPosY = Random.Range((int)(roomBorders[room][2] + 1.5f), (int)(roomBorders[room][3] - 0.5f));
        }
        else
        {
            randPosX = Random.Range((int)(roomBorders[room][0] + 0.5f), (int)(roomBorders[room][1] + 0.5f));
            randPosY = Random.Range((int)(roomBorders[room][2] + 0.5f), (int)(roomBorders[room][3] + 0.5f));
        }

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

            distAway = 1;
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

            distAway = 2;
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

    public void Pause()
    {
        paused = true;
        pauseButton.SetActive(false);
        buttons.SetActive(true);
        player.enabled = false;
        if (player.source.isPlaying) { player.source.Stop(); }
        SoundManager.instance.PlayClip(buttonClick, source);
        Time.timeScale = 0;
    }

    public void Continue()
    {
        paused = false;
        pauseButton.SetActive(true);
        buttons.SetActive(false);
        player.enabled = true;
        SoundManager.instance.PlayClip(buttonClick, source);
        Time.timeScale = 1;
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SoundManager.instance.PlayClip(buttonClick, source);
        SceneManager.LoadScene(1);
    }

    public void Quit()
    {
        Time.timeScale = 1;
        SoundManager.instance.PlayClip(buttonClick, source);
        SceneManager.LoadScene(0);
    }

    public void grabCow()
    {
        if (!isDead)
        {
            print("<color=yellow>Cow acquired");
            if (Random.value < 0.5f) SoundManager.instance.PlayClip(grabCowClip1, source);
            else SoundManager.instance.PlayClip(grabCowClip2, source);
            Destroy(activeCow);
            cowPos = Vector2.positiveInfinity;
            player.hasCow = true;
            cowText.text = "Get to a <color=green>FENCE";
            cowRoom = -1; //used in minimap tracker
            if (cows > 0 && cows == torchOffRound - torchOffRounds) randPrompt = dLog.insertText(batteriesAcquiredPrompts, batteriesAcquiredColors);
            else randPrompt = dLog.insertText(grabCowPrompts, grabCowColors);
        }
    }

    public void WinGame(bool bottomOfMap = false)
    {
        if (hideHuman) hideHuman = false;

        print("<color=green>Woohoo!");
        player.hasCow = false;
        foreach (GameObject bar in minimap.flashBars) bar.SetActive(false);
        check = false;

        player.anim.SetBool("upCheck", false);
        player.anim.SetBool("downCheck", false);
        player.anim.SetBool("leftCheck", false);
        player.anim.SetBool("rightCheck", false);

        StartCoroutine(Abduct(bottomOfMap));
    }

    public void LoseGame(bool timeOut = false)
    {
        if (timeOut)
        {
            player.anim.SetBool("isDead", true);
            dLog.insertText(outOfTimePrompts, outOfTimeColors);
        }
        else
        {
            player.anim.SetBool("isCaught", true);
            dLog.insertText(diedPrompts, diedColors);
        }
        player.GetComponent<SpriteRenderer>().sortingOrder = 100; //put in front of black screen
        //startScreenBlack.LeanAlpha(0.98f, startScreenFadeTime - 1); //fade out scene
        startScreenBlack.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0.98f);
        //player.transform.LeanScale(Vector3.one * 4, 2);
        //cowCount.transform.LeanScale(Vector3.one * 2, 2);
        cowCount.transform.localPosition = new Vector2(0, -250);
        cowText.color = new Color(1, 1, 1, 0.1f);
        hText.color = new Color(1, 1, 1, 0.1f);
        timer.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0.1f);
        gameOver.SetActive(true);

        foreach (Human human in humans) human.chase = false;
        foreach (Light2D light in allLights) light.enabled = false;
        foreach (GameObject bar in minimap.flashBars) bar.SetActive(false);

        print("<color=red>Loser...");
        pauseButton.SetActive(false);
        buttons.transform.localPosition = new Vector2(0, -400);
        buttons.SetActive(true);
        continueButton.SetActive(false);
        player.source.Stop(); //if died while moving
        SoundManager.instance.PlayClip(loseClip, source);
        Destroy(timer);
        Destroy(player);
        isDead = true;
    }

    IEnumerator Abduct(bool bottomOfMap = false)
    {
        pendingRound = true;
        fastForward.SetActive(true);
        pauseButton.SetActive(false);
        cowText.enabled = false;
        CameraFollow cam = FindAnyObjectByType<CameraFollow>();
        cam.enabled = false; //disable the camera following
        player.torchLight.enabled = false;
        humans[0].torchLight.enabled = false;
        humans[1].torchLight.enabled = false;

        UFO.transform.position = new Vector2(player.transform.position.x, player.transform.position.y + 13); //line UFO up with player
        UFO.gameObject.LeanMoveY(UFO.transform.position.y - 10, UFOAscentTime).setEaseOutCubic(); //UFO descends
        //SoundManager.instance.PlayClip(UFOClip, source);
        yield return new WaitForSeconds(UFOAscentTime);

        player.gameObject.LeanMoveY(player.transform.position.y + 3, playerAscentTime).setEaseInQuad(); //player ascends
        SoundManager.instance.PlayClip(abductClip, source, false, 1, Random.Range(0.8f,1.2f));
        yield return new WaitForSeconds(playerAscentTime);
        
        source.Stop();
        player.GetComponent<SpriteRenderer>().enabled = false;
        UFO.gameObject.LeanMoveX(UFO.transform.position.x + 10, camExitTime).setEaseInCirc(); //UFO goes right
        SoundManager.instance.PlayClip(UFOClip, source);
        cam.gameObject.LeanMoveX(cam.transform.position.x + 20, camExitTime).setEaseInCirc(); //camera pans right
        yield return new WaitForSeconds(camExitTime); //change to camExitTime for quicker?

        resetRooms(); //also resets active room and borders to backyard
        StartNewRound();
        cam.transform.position = new Vector3(cam.transform.position.x - 40, activeBorder1.y - 0.5f, cam.transform.position.z); //teleport camera to backyard
        UFO.transform.position = new Vector2(Random.Range((int)(activeBorder1.x + 0.5f), (int)(activeBorder2.x + 0.5f)), activeBorder1.y - 0.5f + 3);
        player.transform.position = new Vector2(UFO.transform.position.x, activeBorder1.y - 0.5f + 3); //set new player position with UFO
        player.hand.transform.localEulerAngles = new Vector3(0, 0, 180); //rotate torch to face downward
        lightPole.transform.position = new Vector2(Random.Range(activeBorder1.x + 1, activeBorder2.x - 1), lightPole.transform.position.y); //light post moves
        rooms[0].door[0].transform.position = new Vector2(Random.Range((int)(activeBorder1.x + 0.5f), (int)(activeBorder2.x + 0.5f)), rooms[0].door[0].transform.position.y); //reset random door pos
        cam.gameObject.LeanMoveX(cam.transform.position.x + 20, camExitTime).setEaseOutQuart(); //camera fades back onto BackYard
        yield return new WaitForSeconds(camExitTime);

        player.GetComponent<SpriteRenderer>().enabled = true;
        player.gameObject.LeanMoveY(player.transform.position.y - 3, playerAscentTime).setEaseOutCubic(); //player descends
        SoundManager.instance.PlayClip(abductClip, source, false, 1, Random.Range(0.8f, 1.2f));
        yield return new WaitForSeconds(playerAscentTime);

        source.Stop();
        UFO.gameObject.LeanMoveY(UFO.transform.position.y + 10, UFOAscentTime-0.5f).setEaseInBack(); //UFO ascends
        //SoundManager.instance.PlayClip(UFOClip, source);
        yield return new WaitForSeconds(UFOAscentTime/2);

        humans[0].torchLight.enabled = true;
        humans[1].torchLight.enabled = true;
        player.torchLight.enabled = true;

        //if (cows == torchOffRound)
        //{
        //    player.torchOff();
        //    torchOffRound += torchOffRounds; //occurs every 5 rounds
        //    dLog.insertText(torchDiedPrompts, torchDiedColors);
        //}
        //else randPrompt = dLog.insertText(startGamePrompts, startGameColors);

        cam.enabled = true;
        fastForward.SetActive(false);
        pauseButton.SetActive(true);
        cowText.enabled = true;
        cowText.text = "Get the <color=green>COW";
        pendingRound = false;
        Time.timeScale = 1;
        yield return null;
    }
}
