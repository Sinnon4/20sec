using UnityEngine;

public class RoomTiles : MonoBehaviour
{
    [SerializeField] public GameObject base_;
    [SerializeField] public GameObject[] door;
    [SerializeField] public Sprite
        topLeft, top, topRight,
        left, middle, right,
        bottomLeft, bottom, bottomRight,
        wall;

    int newX, newY;

    public Vector4 randomiseSize(int x, int y)
    {
        for (int i = 0; i < x; i++)
            for (int j = -1; j < y; j++)
            {
                if (i == 0 && j == 0)
                { continue; } //ignore as top left already exists
                else
                {
                    newX = (int)gameObject.transform.position.x + i;
                    newY = (int)gameObject.transform.position.y - j; //use minus as y is going down from top left
                    SpriteRenderer tile = Instantiate(base_, new Vector2(newX, newY), Quaternion.identity, gameObject.transform).GetComponent<SpriteRenderer>();

                    //update sprites as per position in room - noting top left already accounted for
                    if      (              j == -1   )  { tile.sprite = wall;           tile.name = "Wall"; }
                    else if (i == x - 1 && j == 0    )  { tile.sprite = topRight;       tile.name = "TopRight"; }
                    else if (i == x - 1 && j == y - 1)  { tile.sprite = bottomRight;    tile.name = "BottomRight"; }
                    else if (i == 0     && j == y - 1)  { tile.sprite = bottomLeft;     tile.name = "BottomLeft"; }
                    else if (i == 0                  )  { tile.sprite = left;           tile.name = "Left"; }
                    else if (i == x - 1              )  { tile.sprite = right;          tile.name = "Right"; }
                    else if (              j == 0    )  { tile.sprite = top;            tile.name = "Top"; }
                    else if (              j == y - 1)  { tile.sprite = bottom;         tile.name = "Bottom"; }
                    else                                { tile.sprite = middle;         tile.name = "Middle"; }
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
