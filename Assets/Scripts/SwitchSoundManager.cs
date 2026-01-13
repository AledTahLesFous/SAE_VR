using UnityEngine;

/// <summary>
/// Gère les effets sonores des interrupteurs (activation/désactivation)
/// </summary>
public class SwitchSoundManager : MonoBehaviour
{
    [Header("Configuration Audio")]
    [Tooltip("Son lorsque l'interrupteur est activé (ON)")]
    [SerializeField] private AudioClip switchOnSound;
    
    [Tooltip("Son lorsque l'interrupteur est désactivé (OFF)")]
    [SerializeField] private AudioClip switchOffSound;
    
    [Header("Paramètres Audio")]
    [Tooltip("Volume des sons d'interrupteur (0 à 1)")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.7f;
    
    [Tooltip("Distance minimale pour entendre les sons en 3D")]
    [SerializeField] private float minDistance = 1f;
    
    [Tooltip("Distance maximale pour entendre les sons en 3D")]
    [SerializeField] private float maxDistance = 10f;
    
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
    /// Joue le son d'activation (ON)
    /// </summary>
    public void PlaySwitchOnSound()
    {
        PlaySound(switchOnSound);
    }
    
    /// <summary>
    /// Joue le son de désactivation (OFF)
    /// </summary>
    public void PlaySwitchOffSound()
    {
        PlaySound(switchOffSound);
    }
    
    /// <summary>
    /// Joue le son approprié selon l'état
    /// </summary>
    /// <param name="isOn">True si l'interrupteur est activé, False sinon</param>
    public void PlaySwitchSound(bool isOn)
    {
        if (isOn)
        {
            PlaySwitchOnSound();
        }
        else
        {
            PlaySwitchOffSound();
        }
    }
    
    /// <summary>
    /// Joue un clip audio spécifique
    /// </summary>
    /// <param name="clip">Le clip audio à jouer</param>
    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning($"[SwitchSoundManager] Aucun clip audio assigné sur {gameObject.name}");
            return;
        }
        
        if (audioSource == null)
        {
            Debug.LogError($"[SwitchSoundManager] AudioSource manquant sur {gameObject.name}");
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
