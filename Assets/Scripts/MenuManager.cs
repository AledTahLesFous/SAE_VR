using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Références des boutons")]
    [SerializeField] private Button startButton;
    
    [Header("Texte de feedback (optionnel)")]
    [SerializeField] private TextMeshProUGUI feedbackText;
    
    [Header("Configuration")]
    [Tooltip("Index de la scène dans Build Settings (0 = Menu, 1 = Space, etc.)")]
    [SerializeField] private int sceneIndex = 2; // Changez à 2 pour tester avec une scène vide
    
    private bool isLoading = false;
    
    private void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }
    
    public void OnStartButtonClicked()
    {
        if (isLoading) return;
        
        isLoading = true;
        
        if (startButton != null)
        {
            startButton.interactable = false;
        }
        
        // Changer le texte du bouton
        var buttonText = startButton?.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = "Chargement...";
        }
        
        // Lancer le chargement avec un délai
        StartCoroutine(LoadSceneWithDelay());
    }
    
    private IEnumerator LoadSceneWithDelay()
    {
        // Attendre 0.5 seconde pour que l'UI se mette à jour
        yield return new WaitForSeconds(0.5f);
        
        var buttonText = startButton?.GetComponentInChildren<TextMeshProUGUI>();
        
        // Vérifier que l'index est valide
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            if (buttonText != null)
            {
                buttonText.text = "ERREUR: Index " + sceneIndex + " invalide!";
            }
            isLoading = false;
            yield break;
        }
        
        if (buttonText != null)
        {
            buttonText.text = "Chargement...";
        }
        
        // Charger par index (plus fiable que par nom)
        SceneManager.LoadScene(sceneIndex);
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
