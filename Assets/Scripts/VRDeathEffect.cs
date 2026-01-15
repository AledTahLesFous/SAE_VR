using UnityEngine;
using System.Collections;

public class VRDeathEffect : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera mainCamera;
    
    [Header("Death Effect Settings")]
    public float fadeToBlackDuration = 2f;
    public float cameraFallSpeed = 1.5f;
    public float cameraFallDistance = 1.5f;
    public AudioClip deathSound;
    
    [Header("Respawn Settings")]
    public Transform respawnPoint;
    public float respawnDelay = 3f;
    public bool autoRespawn = true;
    
    private bool isDead = false;
    private Material fadeMaterial;
    private GameObject fadeQuad;
    private Vector3 cameraStartPosition;
    private Quaternion cameraStartRotation;
    private AudioSource audioSource;

    void Start()
    {
        // Trouver la caméra
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // Créer l'audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
        
        // Créer le fade overlay
        CreateFadeQuad();
        
        Debug.Log("✅ VRDeathEffect initialisé");
    }

    void CreateFadeQuad()
    {
        if (mainCamera == null) return;
        
        fadeQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fadeQuad.name = "DeathFadeOverlay";
        Destroy(fadeQuad.GetComponent<Collider>());
        
        fadeQuad.transform.SetParent(mainCamera.transform);
        fadeQuad.transform.localPosition = new Vector3(0, 0, 0.1f);
        fadeQuad.transform.localRotation = Quaternion.identity;
        fadeQuad.transform.localScale = new Vector3(2, 2, 1);
        
        fadeMaterial = new Material(Shader.Find("Unlit/Color"));
        fadeMaterial.color = new Color(0, 0, 0, 0);
        fadeQuad.GetComponent<Renderer>().material = fadeMaterial;
        fadeQuad.layer = 5;
    }

    public void TriggerDeath()
    {
        if (isDead) return;
        
        Debug.Log("💀 MORT DU JOUEUR !");
        isDead = true;
        
        cameraStartPosition = mainCamera.transform.position;
        cameraStartRotation = mainCamera.transform.rotation;
        
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        DisablePlayerMovement();
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        float elapsedTime = 0f;
        Vector3 targetPosition = cameraStartPosition - Vector3.up * cameraFallDistance;
        Quaternion targetRotation = cameraStartRotation * Quaternion.Euler(15, 0, 10);
        
        while (elapsedTime < fadeToBlackDuration)
        {
            elapsedTime += Time.deltaTime;
            float fadeProgress = elapsedTime / fadeToBlackDuration;
            
            if (fadeMaterial != null)
            {
                Color fadeColor = fadeMaterial.color;
                fadeColor.a = Mathf.Lerp(0, 1, fadeProgress);
                fadeMaterial.color = fadeColor;
            }
            
            if (mainCamera != null)
            {
                mainCamera.transform.position = Vector3.Lerp(cameraStartPosition, targetPosition, fadeProgress * cameraFallSpeed);
                mainCamera.transform.rotation = Quaternion.Lerp(cameraStartRotation, targetRotation, fadeProgress);
            }
            
            yield return null;
        }
        
        Debug.Log("☠️ Écran noir complet");
        yield return new WaitForSeconds(respawnDelay);
        
        if (autoRespawn)
        {
            RespawnPlayer();
        }
    }

    void DisablePlayerMovement()
    {
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            string name = script.GetType().Name;
            if (name.Contains("Movement") || name.Contains("Locomotion") || name.Contains("Teleport"))
            {
                script.enabled = false;
            }
        }
    }

    void EnablePlayerMovement()
    {
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = true;
        }
        
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            string name = script.GetType().Name;
            if (name.Contains("Movement") || name.Contains("Locomotion") || name.Contains("Teleport"))
            {
                script.enabled = true;
            }
        }
    }

    public void RespawnPlayer()
    {
        Debug.Log("🔄 RESPAWN du joueur");
        
        isDead = false;
        
        if (fadeMaterial != null)
        {
            fadeMaterial.color = new Color(0, 0, 0, 0);
        }
        
        if (mainCamera != null)
        {
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.identity;
        }
        
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
        }
        
        EnablePlayerMovement();
    }

    public bool IsDead()
    {
        return isDead;
    }

    void OnDestroy()
    {
        if (fadeQuad != null)
        {
            Destroy(fadeQuad);
        }
        if (fadeMaterial != null)
        {
            Destroy(fadeMaterial);
        }
    }
}