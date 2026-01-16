using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gère l'écran de victoire avec les options de rejouer, retourner au menu ou quitter
/// </summary>
public class VictoryManager : MonoBehaviour
{
    [Header("Références des boutons")]
    [SerializeField] private Button replayButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button quitButton;
    
    [Header("Texte de victoire")]
    [SerializeField] private TextMeshProUGUI victoryTitle;
    [SerializeField] private TextMeshProUGUI victoryMessage;
    [SerializeField] private TextMeshProUGUI creditsText;
    
    [Header("Messages")]
    [SerializeField] private string congratsMessage = "Félicitations, vous avez réussi à vous échapper !";
    [SerializeField] private string credits = "Réalisé par\nClément Muzard, Maïro Frebourg & Thomas Roland\nIUT Béziers";
    
    [Header("Configuration des scènes")]
    [Tooltip("Index de la scène de jeu dans Build Settings")]
    [SerializeField] private int gameSceneIndex = 2;
    
    [Tooltip("Index de la scène du menu dans Build Settings")]
    [SerializeField] private int menuSceneIndex = 1;
    
    [Header("Animation (optionnel)")]
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Audio (optionnel)")]
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip buttonClickSound;
    private AudioSource audioSource;
    
    private void Start()
    {
        // Initialiser les textes
        if (victoryMessage != null)
        {
            victoryMessage.text = congratsMessage;
        }
        
        if (creditsText != null)
        {
            creditsText.text = credits;
        }
        
        // Configuration audio
        if (victorySound != null || buttonClickSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            
            if (victorySound != null)
            {
                audioSource.PlayOneShot(victorySound);
            }
        }
        
        // Configuration des boutons
        if (replayButton != null)
        {
            replayButton.onClick.AddListener(OnReplayClicked);
        }
        
        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuClicked);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
        
        // Animation de fade in (optionnel)
        if (canvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
        
        Debug.Log("[VictoryManager] Écran de victoire initialisé");
    }
    
    private System.Collections.IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0;
        float elapsedTime = 0;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1;
    }
    
    /// <summary>
    /// Rejouer le jeu depuis le début
    /// </summary>
    public void OnReplayClicked()
    {
        PlayButtonSound();
        Debug.Log("[VictoryManager] Relancement du jeu...");
        SceneManager.LoadScene(gameSceneIndex);
    }
    
    /// <summary>
    /// Retourner au menu principal
    /// </summary>
    public void OnMenuClicked()
    {
        PlayButtonSound();
        Debug.Log("[VictoryManager] Retour au menu...");
        SceneManager.LoadScene(menuSceneIndex);
    }
    
    /// <summary>
    /// Quitter le jeu
    /// </summary>
    public void OnQuitClicked()
    {
        PlayButtonSound();
        Debug.Log("[VictoryManager] Fermeture du jeu...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
}
