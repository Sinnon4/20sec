using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void PlayClip(AudioClip clip, AudioSource source, bool loop = false, float vol = 1f, float ptch = 1f)
    {
        //if (source.outputAudioMixerGroup == null) print(source.gameObject.name + " needs master mixer");

        source.loop = loop;
        source.pitch = ptch;
        source.volume = vol;
        source.clip = clip;

        if (!source.isActiveAndEnabled) { Debug.Log($"<color=orange>Error with {source.gameObject.name} trying to play {clip} - <color=blue>AudioSource<color=orange> not enabled"); }
        else source.Play();
    }
}
