using System.Collections.Generic;
using UnityEngine;

public class RoomTiles : MonoBehaviour
{
    [SerializeField] public GameObject base_;
    public List<GameObject> tiles = new();
    [SerializeField] public GameObject[] door;
    [SerializeField] public Sprite floor, wall, backyardSky;
    [SerializeField] public Sprite[] obstacleSprites;

    public AudioSource source;

    int newX, newY;

    private void Awake()
    {
        source = GetComponent<AudioSource>();

        tiles.Add(base_);
    }

    public Vector4 randomiseSize(int x, int y, bool backyard = false)
    {
        for (int i = 0; i < x; i++)
        {
            for (int j = -1; j < y; j++)
            {
                if (i == 0 && j == 0)
                { continue; } //ignore as top left already exists
                else
                {
                    newX = (int)gameObject.transform.position.x + i;
                    newY = (int)gameObject.transform.position.y - j; //use minus as y is going down from top left
                    tiles.Add(Instantiate(base_, new Vector2(newX, newY), Quaternion.identity, gameObject.transform));
                    SpriteRenderer tile = tiles[tiles.Count - 1].GetComponent<SpriteRenderer>();
                    //update sprites as per position in room - noting top left already accounted for
                    if (j == -1) { tile.sprite = wall; tile.name = "Wall"; }
                    else tile.sprite = floor; tile.name = "Floor";
                }

                if (backyard && j == -1) //add extra wall layers for BackYard sky
                {
                    newX = (int)gameObject.transform.position.x + i;
                    newY = (int)gameObject.transform.position.y - j + 1;
                    tiles.Add(Instantiate(base_, new Vector2(newX, newY), Quaternion.identity, gameObject.transform));
                    SpriteRenderer tile = tiles[tiles.Count - 1].GetComponent<SpriteRenderer>();
                    tile.sprite = backyardSky; tile.name = "Sky";
                }
            }
        }

        return new Vector4(
            transform.position.x - 0.5f, //left - represented as roomBorders[i][0]
            transform.position.x - 0.5f + x, //right - represented as roomBorders[i][1]
            transform.position.y + 0.5f - y, //bottom - represented as roomBorders[i][2]
            transform.position.y + 0.5f //top - represented as roomBorders[i][3]
            );
    }
}
