using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// NPC qui donne le code via des sous-titres VR (attachés au casque)
/// </summary>
public class NPCCodeGiver : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string code = "1234";
    [SerializeField] private string message = "Le code est : ";
    
    [Header("Sous-titres VR")]
    [Tooltip("Durée d'affichage des sous-titres")]
    [SerializeField] private float displayDuration = 15f;
    [Tooltip("Distance des sous-titres devant le joueur")]
    [SerializeField] private float subtitleDistance = 0.5f;
    [Tooltip("Position verticale (0 = au niveau des yeux)")]
    [SerializeField] private float subtitleVerticalOffset = -0.3f;
    [Tooltip("Taille du texte")]
    [SerializeField] private float fontSize = 0.15f;
    
    [Header("Détection VR")]
    [Tooltip("Distance pour déclencher l'interaction (en mètres)")]
    [SerializeField] private float interactionDistance = 0.25f;
    [Tooltip("Hauteur du point de détection sur le NPC")]
    [SerializeField] private float detectionHeight = 1.2f;
    [Tooltip("Cooldown avant réaffichage")]
    [SerializeField] private float cooldownTime = 3f;
    [Tooltip("Vitesse d'écriture du texte")]
    [SerializeField] private float typingSpeed = 0.05f;
    
    [Header("Audio (optionnel)")]
    [SerializeField] private AudioClip voiceClip;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private AudioSource audioSource;
    private float lastInteractionTime = -999f;
    private Transform playerHead;
    private List<Transform> vrControllers = new List<Transform>();
    private bool wasInRange = false;
    
    // Sous-titres
    private GameObject subtitleCanvas;
    private TextMeshProUGUI subtitleText;
    private Coroutine typingCoroutine;
    
    void Start()
    {
        // Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && voiceClip != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Trouver les contrôleurs VR
        FindVRControllers();
        
        // Créer le système de sous-titres
        CreateSubtitleSystem();
        
        if (showDebugLogs)
        {
            Debug.Log($"[NPCCodeGiver] Initialisé - {vrControllers.Count} contrôleurs trouvés");
        }
    }
    
    void CreateSubtitleSystem()
    {
        // Trouver la caméra VR
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("[NPCCodeGiver] Pas de caméra principale trouvée!");
            return;
        }
        playerHead = mainCam.transform;
        
        // Créer le Canvas pour les sous-titres
        subtitleCanvas = new GameObject("NPC_Subtitles");
        Canvas canvas = subtitleCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        // Taille du canvas
        RectTransform canvasRect = subtitleCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(800, 200);
        canvasRect.localScale = new Vector3(0.002f, 0.002f, 0.002f);
        
        // Créer le texte
        GameObject textObj = new GameObject("SubtitleText");
        textObj.transform.SetParent(subtitleCanvas.transform, false);
        subtitleText = textObj.AddComponent<TextMeshProUGUI>();
        subtitleText.text = "";
        subtitleText.fontSize = 16;
        subtitleText.color = Color.white;
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.fontStyle = FontStyles.Bold;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Cacher au départ
        subtitleCanvas.SetActive(false);
        
        if (showDebugLogs) Debug.Log("[NPCCodeGiver] Système de sous-titres créé");
    }
    
    void FindVRControllers()
    {
        vrControllers.Clear();
        
        // Chercher tous les interactors XR dans la scène
        XRBaseInteractor[] interactors = FindObjectsByType<XRBaseInteractor>(FindObjectsSortMode.None);
        
        foreach (var interactor in interactors)
        {
            vrControllers.Add(interactor.transform);
            
            if (showDebugLogs)
            {
                Debug.Log($"[NPCCodeGiver] Contrôleur trouvé: {interactor.name}");
            }
        }
        
        // Fallback par nom
        if (vrControllers.Count == 0)
        {
            GameObject leftCtrl = GameObject.Find("Left Controller");
            GameObject rightCtrl = GameObject.Find("Right Controller");
            
            if (leftCtrl != null) vrControllers.Add(leftCtrl.transform);
            if (rightCtrl != null) vrControllers.Add(rightCtrl.transform);
        }
    }
    
    void Update()
    {
        // Re-chercher les contrôleurs si pas trouvés
        if (vrControllers.Count == 0)
        {
            FindVRControllers();
        }
        
        // Vérifier la distance avec chaque contrôleur
        CheckControllerProximity();
        
        // Positionner les sous-titres devant le joueur
        UpdateSubtitlePosition();
    }
    
    void UpdateSubtitlePosition()
    {
        if (subtitleCanvas == null || playerHead == null) return;
        if (!subtitleCanvas.activeSelf) return;
        
        // Position devant le joueur, légèrement en bas
        Vector3 forward = playerHead.forward;
        forward.y = 0;
        forward.Normalize();
        
        Vector3 position = playerHead.position + forward * subtitleDistance;
        position.y = playerHead.position.y + subtitleVerticalOffset;
        
        subtitleCanvas.transform.position = position;
        
        // Regarder vers le joueur
        subtitleCanvas.transform.LookAt(playerHead);
        subtitleCanvas.transform.Rotate(0, 180, 0);
    }
    
    void CheckControllerProximity()
    {
        Vector3 npcCenter = transform.position + Vector3.up * detectionHeight;
        
        bool isInRange = false;
        
        foreach (Transform controller in vrControllers)
        {
            if (controller == null) continue;
            
            float distance = Vector3.Distance(controller.position, npcCenter);
            
            if (distance <= interactionDistance)
            {
                isInRange = true;
                break;
            }
        }
        
        // Déclencher uniquement quand on ENTRE dans la zone
        if (isInRange && !wasInRange)
        {
            if (Time.time - lastInteractionTime >= cooldownTime)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[NPCCodeGiver] Entrée dans la zone - INTERACTION!");
                }
                GiveCode();
            }
        }
        
        wasInRange = isInRange;
    }
    
    // === MÉTHODES POUR XR SIMPLE INTERACTABLE ===
    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (showDebugLogs) Debug.Log("[NPCCodeGiver] XR Hover!");
        if (Time.time - lastInteractionTime >= cooldownTime) GiveCode();
    }
    
    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (showDebugLogs) Debug.Log("[NPCCodeGiver] XR Select!");
        if (Time.time - lastInteractionTime >= cooldownTime) GiveCode();
    }
    
    // === AFFICHAGE ===
    void GiveCode()
    {
        lastInteractionTime = Time.time;
        
        if (showDebugLogs) Debug.Log($"[NPCCodeGiver] ★★★ CODE: {code} ★★★");
        
        if (subtitleText != null)
        {
            // Afficher le canvas
            subtitleCanvas.SetActive(true);
            
            // Démarrer l'effet machine à écrire
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(message + code));
        }
        
        if (audioSource != null && voiceClip != null)
        {
            audioSource.PlayOneShot(voiceClip);
        }
    }
    
    IEnumerator TypeText(string fullText)
    {
        subtitleText.text = "";
        
        foreach (char letter in fullText)
        {
            subtitleText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        // Attendre avant de cacher
        yield return new WaitForSeconds(displayDuration);
        HideSubtitles();
    }
    
    void HideSubtitles()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        
        if (subtitleCanvas != null)
            subtitleCanvas.SetActive(false);
    }
    
    public void ResetCodeGiver()
    {
        lastInteractionTime = -999f;
        wasInRange = false;
        HideSubtitles();
        if (showDebugLogs) Debug.Log("[NPCCodeGiver] Reset");
    }
    
    void OnDestroy()
    {
        if (subtitleCanvas != null)
            Destroy(subtitleCanvas);
    }
    
    // Visualisation dans l'éditeur
    void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position + Vector3.up * detectionHeight;
        
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawWireSphere(center, interactionDistance);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(center, 0.05f);
    }
}
