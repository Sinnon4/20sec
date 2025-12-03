using UnityEngine;
using UnityEngine.UI;

public class MinimapTracker : MonoBehaviour
{
    RoomHandler roomHandler;

    [SerializeField] Image[] images;
    [SerializeField] float alpha = 0.5f;

    private void Awake()
    {
        roomHandler = FindAnyObjectByType<RoomHandler>();
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
                else if (i == roomHandler.hRoom) images[i].color = Color.red;
                else images[i].color = Color.gray;

                images[i].color = new Color(images[i].color.r, images[i].color.g, images[i].color.b, alpha); //apply alpha
            }
        }
    }
}
