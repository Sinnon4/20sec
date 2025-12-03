using System.Collections;
using TMPro;
using UnityEngine;

public class TextTimer : MonoBehaviour
{
    DialogueHandler dlog;
    TextMeshProUGUI txt;

    [SerializeField] float lifespan = 5, fadeTime = 2;
    float t;

    private void Awake()
    {
        dlog = GetComponentInParent<DialogueHandler>();
        txt = GetComponent<TextMeshProUGUI>();
        t = lifespan;
    }

    private void FixedUpdate()
    {
        t -= Time.deltaTime;
        if (t < fadeTime)
        {
            txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, Mathf.Lerp(1, 0, 1 - (t/fadeTime))); // 3rd argument of Lerp refers to the percentage along the transition (0-100)
        }
        if (t < 0)
        {
            dlog.prompts.Remove(gameObject);
            Destroy(gameObject);
        }
    }
}
