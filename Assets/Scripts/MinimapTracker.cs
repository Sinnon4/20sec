using UnityEngine;
using UnityEngine.UI;

public class MinimapTracker : MonoBehaviour
{
    RoomHandler roomHandler;
    Player player;

    [SerializeField] Image[] images;
    [SerializeField] float alpha = 0.5f;
    [SerializeField] public GameObject[] flashBars;
    float t;
    [SerializeField] float onTime, offTime;

    private void Awake()
    {
        roomHandler = FindAnyObjectByType<RoomHandler>();
        player = FindAnyObjectByType<Player>();
    }

    private void Update()
    {
        if (player.hasCow)
        {
            if (t > 0) t -= Time.deltaTime;
            else
            {
                if (flashBars[0].activeInHierarchy)
                {
                    foreach (GameObject bar in flashBars) bar.SetActive(false);
                    t = offTime;
                }
                else
                {
                    foreach (GameObject bar in flashBars) bar.SetActive(true);
                    t = onTime;
                }
            }
        }
    }

    public void updateMap()
    {
        if (roomHandler.hideHuman)
        {
            for (int i = 0; i < images.Length; i++)
            {
                if (i == roomHandler.activeRoom) images[i].color = Color.white;
                else if (i == roomHandler.cowRoom) images[i].color = Color.green;
                else images[i].color = Color.gray;

                images[i].color = new Color(images[i].color.r, images[i].color.g, images[i].color.b, alpha); //apply alpha
            }
        }
        else
        {
            for (int i = 0; i < images.Length; i++)
            {
                if (i == roomHandler.activeRoom) images[i].color = Color.white;
                else if (i == roomHandler.cowRoom) images[i].color = Color.green;
                else if (roomHandler.hRooms.Contains(i)) images[i].color = Color.red;
                else images[i].color = Color.gray;

                images[i].color = new Color(images[i].color.r, images[i].color.g, images[i].color.b, alpha); //apply alpha
            }
        }
    }
}
