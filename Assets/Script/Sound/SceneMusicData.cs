using UnityEngine;

public class SceneMusicData : MonoBehaviour
{
    [Header("Scene-specific Music Clips")]
    public AudioClip normalMusic;
    public AudioClip combatMusic;

    void Start()
    {
        // Khi scene load, báo cho AudioManager biết nhạc của scene này
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSceneMusic(normalMusic, combatMusic);
        }
    }
}
