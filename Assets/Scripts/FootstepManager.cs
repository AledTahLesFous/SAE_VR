using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepManager : MonoBehaviour
{
    [Header("Fichiers Audio (Boucles)")]
    [Tooltip("Glissez ici le fichier 'indoor-footsteps'")]
    [SerializeField] private AudioClip walkSound;

    [Tooltip("Glissez ici le fichier 'running'")]
    [SerializeField] private AudioClip runSound;

    [Header("Réglages")]
    [Tooltip("Vitesse minimale pour que le son commence (ex: 0.1)")]
    [SerializeField] private float minMovementSpeed = 0.1f;

    [Tooltip("Vitesse à partir de laquelle on passe au son de course (ex: 2.0)")]
    [SerializeField] private float runSpeed = 2.0f;

    private AudioSource audioSource;
    private Vector3 lastPosition;
    private bool isRunning;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;          // Important pour vos fichiers longs
        audioSource.playOnAwake = false;  // Ne pas jouer au lancement
        audioSource.spatialBlend = 1f;    // Son 3D
    }

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        // 1. Calcul de la vitesse
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        float currentSpeed = distanceMoved / Time.deltaTime;
        
        lastPosition = transform.position;

        // 2. Est-ce qu'on bouge ?
        if (currentSpeed > minMovementSpeed)
        {
            // On détermine quel son jouer (Marche ou Course ?)
            AudioClip targetClip = (currentSpeed > runSpeed) ? runSound : walkSound;

            // Si on ne joue pas le bon son, on le change et on lance la lecture
            if (audioSource.clip != targetClip || !audioSource.isPlaying)
            {
                audioSource.clip = targetClip;
                audioSource.Play();
            }
        }
        else
        {
            // 3. ARRÊT IMMÉDIAT (C'est ça qui fait l'effet "0.1s" si on s'arrête vite)
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}