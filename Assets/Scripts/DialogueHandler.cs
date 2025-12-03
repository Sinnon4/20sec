using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueHandler : MonoBehaviour
{
    RoomHandler roomHandler;

    [SerializeField] GameObject txtPrefab;
    public List<GameObject> prompts = new();
    [SerializeField] float yUp;
    [SerializeField] float leanTime;
    [SerializeField] LeanTweenType leanType;

    [SerializeField] AudioClip newMessage;
    AudioSource source;

    private void Awake()
    {
        roomHandler = FindAnyObjectByType<RoomHandler>();
        source = GetComponent<AudioSource>();
    }

    public float insertText(string[] strings, string[] colors)
    {
        if (!roomHandler.isDead)
        {
            if (prompts.Count > 0)
            {
                for (int i = 0; i < prompts.Count; i++)
                {
                    LeanTween.moveLocalY(prompts[i], (prompts.Count - i) * yUp, leanTime).setEase(leanType);
                }
            }

            prompts.Add(Instantiate(txtPrefab, transform));
            prompts[prompts.Count - 1].transform.localPosition = new Vector2(0, -200);
            int r = Random.Range(0, strings.Length);
            prompts[prompts.Count - 1].GetComponent<TextMeshProUGUI>().text = $"<color={colors[r]}>{strings[r]}"; //choose a random string from list
            LeanTween.moveLocalY(prompts[prompts.Count - 1], 0, leanTime).setEase(leanType);

            SoundManager.instance.PlayClip(newMessage, source, false, 0.5f);
            
            return Random.Range(1800, 4000);
        }
        else
        {
            for (int i = 0; i < prompts.Count; i++)
            {
                LeanTween.moveLocalX(prompts[i], 1000, leanTime);
            }
            return 1000;
        }
    }
}
