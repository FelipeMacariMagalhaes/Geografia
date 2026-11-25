using UnityEngine;

public class muscimanager : MonoBehaviour

{
    public static muscimanager instance;

    public AudioSource audioSource;
    public AudioClip musica;

    void Awake()
    {

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        audioSource.clip = musica;
        audioSource.loop = true;
        audioSource.Play();
    }
}