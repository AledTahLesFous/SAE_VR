using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Zone de sortie qui charge une nouvelle scène quand le joueur entre dedans
/// </summary>
public class ExitZone : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Index de la scène à charger (Build Settings)")]
    public int sceneIndex = 0;
    
    [Tooltip("Délai avant de charger la scène (en secondes)")]
    public float delayBeforeLoad = 0.5f;
    
    [Header("Effets visuels (optionnel)")]
    [Tooltip("Particules à jouer quand le joueur entre")]
    public ParticleSystem exitParticles;
    
    [Tooltip("Son à jouer quand le joueur entre")]
    public AudioClip exitSound;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    private bool hasTriggered = false;
    private AudioSource audioSource;
    
    void Start()
    {
        // Désactiver le rendu pour rendre la zone invisible
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
        
        // Vérifier que le GameObject a un Collider avec Is Trigger activé
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("[ExitZone] Pas de Collider trouvé ! Ajoutez un Box Collider ou autre.");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning("[ExitZone] Le Collider doit avoir 'Is Trigger' activé !");
            col.isTrigger = true;
        }
        
        // Créer un AudioSource si nécessaire
        if (exitSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = exitSound;
            audioSource.playOnAwake = false;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[ExitZone] Initialisé - Chargera la scène index {sceneIndex} après {delayBeforeLoad}s");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Vérifier si c'est le joueur (Main Camera ou XR Origin)
        if (hasTriggered) return;
        
        bool isPlayer = other.CompareTag("MainCamera") || 
                       other.CompareTag("Player") || 
                       other.name.Contains("XR Origin") ||
                       other.name.Contains("Main Camera");
        
        if (isPlayer)
        {
            hasTriggered = true;
            
            if (showDebugLogs)
            {
                Debug.Log($"[ExitZone] 🚪 Joueur détecté ! Chargement de la scène {sceneIndex} dans {delayBeforeLoad}s...");
            }
            
            // Effets visuels/sonores
            if (exitParticles != null)
            {
                exitParticles.Play();
            }
            
            if (audioSource != null && exitSound != null)
            {
                audioSource.Play();
            }
            
            // Charger la scène après le délai
            Invoke(nameof(LoadScene), delayBeforeLoad);
        }
    }
    
    void LoadScene()
    {
        if (showDebugLogs)
        {
            Debug.Log($"[ExitZone] 🎬 Chargement de la scène index {sceneIndex}...");
        }
        
        // Vérifier que l'index est valide
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"[ExitZone] ❌ ERREUR : L'index {sceneIndex} n'est pas valide !");
            Debug.LogError($"[ExitZone] Index valides : 0 à {SceneManager.sceneCountInBuildSettings - 1}");
            return;
        }
        
        SceneManager.LoadScene(sceneIndex);
    }
    
    // Visualisation dans l'éditeur
    void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (col is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }
        
        // Flèche indiquant la sortie
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
        Gizmos.DrawSphere(transform.position + transform.forward * 2f, 0.2f);
    }
}
