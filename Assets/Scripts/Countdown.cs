using System;
using TMPro;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    RoomHandler roomHandler;
    TextMeshProUGUI timer;
    double t;

    private void Awake()
    {
        roomHandler = FindAnyObjectByType<RoomHandler>();
        timer = GetComponent<TextMeshProUGUI>();
    }

    void FixedUpdate()
    {
        t = Math.Round(20 - Time.timeSinceLevelLoad, 2);
        if (t >= 0) { timer.text = t.ToString(); }
        else { timer.text = "0.00"; roomHandler.LoseGame(); }
    }
}
