using System;
using TMPro;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    RoomHandler roomHandler;
    TextMeshProUGUI timer;
    [SerializeField] float time = 20;
    public double t;
    bool t1, t2, t3, t4, t5;

    private void Awake()
    {
        roomHandler = FindAnyObjectByType<RoomHandler>();
        timer = GetComponent<TextMeshProUGUI>();
        t = time;
    }

    void FixedUpdate()
    {
        if (roomHandler.enableTimer && !roomHandler.pendingRound)
        {
            t = Math.Round(t - Time.deltaTime, 2);
            if (t >= 0) { timer.text = t.ToString(); }
            else { timer.text = "0.00"; LeanTween.scale(gameObject, Vector2.one * 2, 0.5f); roomHandler.LoseGame(); }

            if      (t < 5 && !t5) { t5 = true; LeanTween.scale(gameObject, Vector2.one * 2.5f, 0.4f); }
            else if (t < 4 && !t4) { t4 = true; LeanTween.scale(gameObject, Vector2.one * 2, 0.4f); }
            else if (t < 3 && !t3) { t3 = true; LeanTween.scale(gameObject, Vector2.one * 3, 0.4f); }
            else if (t < 2 && !t2) { t2 = true; LeanTween.scale(gameObject, Vector2.one * 2, 0.4f); }
            else if (t < 1 && !t1) { t1 = true; LeanTween.scale(gameObject, Vector2.one * 4, 0.4f); }
        }
    }

    public void ResetTimer()
    {
        t = 20;
        transform.localScale = Vector3.one * 2;
        t1 = false; t2 = false; t3 = false; t4 = false; t5 = false;
    }
}
