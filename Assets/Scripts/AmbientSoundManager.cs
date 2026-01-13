using UnityEngine;

/// <summary>
/// Gère la lecture d'un son ambiant en boucle dans la scène
/// </summary>
public class AmbientSoundManager : MonoBehaviour
{
    [Header("Configuration Audio")]
    [Tooltip("Le clip audio à jouer en boucle")]
    [SerializeField] private AudioClip ambientClip;
    
    [Tooltip("Volume du son ambiant (0 à 1)")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;
    
    [Tooltip("Démarrer automatiquement à l'activation")]
    [SerializeField] private bool playOnStart = true;
    
    [Tooltip("Fade in au démarrage (en secondes)")]
    [SerializeField] private float fadeInDuration = 2f;
    
    [Header("Options 3D Audio")]
    [Tooltip("Utiliser le son en mode spatial (3D)")]
    [SerializeField] private bool use3DSound = false;
    
    [Tooltip("Distance minimale pour entendre le son en 3D")]
    [SerializeField] private float minDistance = 1f;
    
    [Tooltip("Distance maximale pour entendre le son en 3D")]
    [SerializeField] private float maxDistance = 50f;
    
    private AudioSource audioSource;
    private float targetVolume;
    private bool isFading = false;
    
    void Awake()
    {
        // Créer ou récupérer le composant AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configuration de l'AudioSource
        ConfigureAudioSource();
        targetVolume = volume;
    }
    
    void Start()
    {
        if (playOnStart && ambientClip != null)
        {
            if (fadeInDuration > 0)
            {
                PlayWithFadeIn();
            }
            else
            {
                Play();
            }
        }
    }
    
    void Update()
    {
        // Gérer le fade in
        if (isFading && audioSource.volume < targetVolume)
        {
            audioSource.volume += (targetVolume / fadeInDuration) * Time.deltaTime;
            
            if (audioSource.volume >= targetVolume)
            {
                audioSource.volume = targetVolume;
                isFading = false;
            }
        }
    }
    
    /// <summary>
    /// Configure l'AudioSource avec les paramètres définis
    /// </summary>
    private void ConfigureAudioSource()
    {
        audioSource.clip = ambientClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
        
        if (use3DSound)
        {
            audioSource.spatialBlend = 1f; // 1 = 3D complet
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
        }
        else
        {
            audioSource.spatialBlend = 0f; // 0 = 2D (ambiant partout)
        }
    }
    
    /// <summary>
    /// Démarre la lecture du son ambiant
    /// </summary>
    public void Play()
    {
        if (ambientClip != null && !audioSource.isPlaying)
        {
            audioSource.volume = targetVolume;
            audioSource.Play();
        }
    }
    
    /// <summary>
    /// Démarre la lecture avec un fade in
    /// </summary>
    public void PlayWithFadeIn()
    {
        if (ambientClip != null)
        {
            audioSource.volume = 0f;
            audioSource.Play();
            isFading = true;
        }
    }
    
    /// <summary>
    /// Arrête la lecture du son
    /// </summary>
    public void Stop()
    {
        audioSource.Stop();
        isFading = false;
    }
    
    /// <summary>
    /// Met en pause la lecture
    /// </summary>
    public void Pause()
    {
        audioSource.Pause();
    }
    
    /// <summary>
    /// Reprend la lecture
    /// </summary>
    public void Resume()
    {
        audioSource.UnPause();
    }
    
    /// <summary>
    /// Change le volume du son ambiant
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        targetVolume = volume;
        if (!isFading)
        {
            audioSource.volume = volume;
        }
    }
    
    /// <summary>
    /// Change le clip audio
    /// </summary>
    public void SetAmbientClip(AudioClip newClip)
    {
        bool wasPlaying = audioSource.isPlaying;
        audioSource.Stop();
        ambientClip = newClip;
        audioSource.clip = newClip;
        
        if (wasPlaying)
        {
            Play();
        }
    }
}
