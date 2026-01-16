using UnityEngine;

/// <summary>
/// Porte de garage qui s'ouvre verticalement
/// À attacher sur la porte
/// </summary>
public class GarageDoor : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float openHeight = 3f; // Hauteur d'ouverture (en mètres)
    [SerializeField] private float openSpeed = 1f; // Vitesse d'ouverture
    [SerializeField] private bool startOpen = false;
    
    [Header("Audio (optionnel)")]
    [SerializeField] private AudioClip openSound;
    
    private AudioSource audioSource;
    
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;
    private bool isMoving = false;
    
    void Start()
    {
        // Créer un Audio Source automatiquement si on a un son
        if (openSound != null)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Sauvegarder la position fermée
        closedPosition = transform.localPosition;
        // Calculer la position ouverte (vers le haut)
        openPosition = closedPosition + Vector3.up * openHeight;
        
        if (startOpen)
        {
            transform.localPosition = openPosition;
            isOpen = true;
        }
    }
    
    /// <summary>
    /// Ouvre la porte - à appeler depuis le KeypadManager
    /// </summary>
    public void Open()
    {
        if (!isOpen && !isMoving)
        {
            StopAllCoroutines();
            StartCoroutine(MoveDoor(openPosition, true));
        }
    }
    
    /// <summary>
    /// Ferme la porte
    /// </summary>
    public void Close()
    {
        if (isOpen && !isMoving)
        {
            StopAllCoroutines();
            StartCoroutine(MoveDoor(closedPosition, false));
        }
    }
    
    /// <summary>
    /// Toggle (ouvre si fermée, ferme si ouverte)
    /// </summary>
    public void Toggle()
    {
        if (isOpen)
            Close();
        else
            Open();
    }
    
    private System.Collections.IEnumerator MoveDoor(Vector3 targetPosition, bool opening)
    {
        isMoving = true;
        
        // Jouer le son d'ouverture
        if (opening && audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }
        
        Vector3 startPosition = transform.localPosition;
        float elapsed = 0f;
        float duration = Vector3.Distance(startPosition, targetPosition) / openSpeed;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Courbe d'ease-in-out pour un mouvement plus naturel
            t = Mathf.SmoothStep(0f, 1f, t);
            
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
        
        transform.localPosition = targetPosition;
        isOpen = opening;
        isMoving = false;
    }
}
