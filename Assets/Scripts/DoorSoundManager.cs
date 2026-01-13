using UnityEngine;

/// <summary>
/// Gère les effets sonores des portes (ouverture, fermeture, verrouillage, etc.)
/// </summary>
public class DoorSoundManager : MonoBehaviour
{
    [Header("Configuration Audio")]
    [Tooltip("Son d'ouverture de la porte")]
    [SerializeField] private AudioClip openSound;
    
    [Tooltip("Son de fermeture de la porte")]
    [SerializeField] private AudioClip closeSound;
    
    [Tooltip("Son de déverrouillage (clé insérée)")]
    [SerializeField] private AudioClip unlockSound;
    
    [Tooltip("Son de la porte verrouillée (tentative d'ouverture sans clé)")]
    [SerializeField] private AudioClip lockedSound;
    
    [Header("Paramètres Audio")]
    [Tooltip("Volume des sons de porte (0 à 1)")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.8f;
    
    [Tooltip("Distance minimale pour entendre les sons en 3D")]
    [SerializeField] private float minDistance = 1f;
    
    [Tooltip("Distance maximale pour entendre les sons en 3D")]
    [SerializeField] private float maxDistance = 15f;
    
    [Tooltip("Utiliser le son spatial (3D)")]
    [SerializeField] private bool use3DSound = true;
    
    private AudioSource audioSource;
    
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
    }
    
    /// <summary>
    /// Configure les paramètres de l'AudioSource
    /// </summary>
    private void ConfigureAudioSource()
    {
        audioSource.loop = false;
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
            audioSource.spatialBlend = 0f; // 0 = 2D
        }
    }
    
    /// <summary>
    /// Joue le son d'ouverture de la porte
    /// </summary>
    public void PlayOpenSound()
    {
        PlaySound(openSound);
    }
    
    /// <summary>
    /// Joue le son de fermeture de la porte
    /// </summary>
    public void PlayCloseSound()
    {
        PlaySound(closeSound);
    }
    
    /// <summary>
    /// Joue le son de déverrouillage
    /// </summary>
    public void PlayUnlockSound()
    {
        PlaySound(unlockSound);
    }
    
    /// <summary>
    /// Joue le son de porte verrouillée
    /// </summary>
    public void PlayLockedSound()
    {
        PlaySound(lockedSound);
    }
    
    /// <summary>
    /// Joue un clip audio spécifique
    /// </summary>
    /// <param name="clip">Le clip audio à jouer</param>
    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning($"[DoorSoundManager] Aucun clip audio assigné sur {gameObject.name}");
            return;
        }
        
        if (audioSource == null)
        {
            Debug.LogError($"[DoorSoundManager] AudioSource manquant sur {gameObject.name}");
            return;
        }
        
        audioSource.PlayOneShot(clip, volume);
    }
    
    /// <summary>
    /// Arrête tous les sons en cours
    /// </summary>
    public void StopAllSounds()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    /// <summary>
    /// Change le volume des sons
    /// </summary>
    /// <param name="newVolume">Nouveau volume (0 à 1)</param>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
    
    /// <summary>
    /// Obtenir le volume actuel
    /// </summary>
    public float GetVolume()
    {
        return volume;
    }
}
