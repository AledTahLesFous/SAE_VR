using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestionnaire du keypad - même structure que MenuManager
/// Assigner les boutons dans l'Inspector
/// </summary>
public class KeypadManager : MonoBehaviour
{
    [Header("Références des boutons numériques")]
    [SerializeField] private Button button0;
    [SerializeField] private Button button1;
    [SerializeField] private Button button2;
    [SerializeField] private Button button3;
    [SerializeField] private Button button4;
    [SerializeField] private Button button5;
    [SerializeField] private Button button6;
    [SerializeField] private Button button7;
    [SerializeField] private Button button8;
    [SerializeField] private Button button9;
    
    [Header("Boutons spéciaux")]
    [SerializeField] private Button buttonClear;
    [SerializeField] private Button buttonOK;
    
    [Header("Affichage")]
    [SerializeField] private TextMeshProUGUI displayText;
    
    [Header("Configuration")]
    [SerializeField] private string correctCode = "1234";
    [SerializeField] private int codeLength = 4;
    
    [Header("Events")]
    [SerializeField] private UnityEngine.Events.UnityEvent onCodeCorrect;
    [SerializeField] private UnityEngine.Events.UnityEvent onCodeIncorrect;
    
    private string currentInput = "";
    
    void Start()
    {
        // Assigner les listeners aux boutons numériques
        if (button0 != null) button0.onClick.AddListener(() => OnNumberPressed(0));
        if (button1 != null) button1.onClick.AddListener(() => OnNumberPressed(1));
        if (button2 != null) button2.onClick.AddListener(() => OnNumberPressed(2));
        if (button3 != null) button3.onClick.AddListener(() => OnNumberPressed(3));
        if (button4 != null) button4.onClick.AddListener(() => OnNumberPressed(4));
        if (button5 != null) button5.onClick.AddListener(() => OnNumberPressed(5));
        if (button6 != null) button6.onClick.AddListener(() => OnNumberPressed(6));
        if (button7 != null) button7.onClick.AddListener(() => OnNumberPressed(7));
        if (button8 != null) button8.onClick.AddListener(() => OnNumberPressed(8));
        if (button9 != null) button9.onClick.AddListener(() => OnNumberPressed(9));
        
        // Boutons spéciaux
        if (buttonClear != null) buttonClear.onClick.AddListener(OnClearPressed);
        if (buttonOK != null) buttonOK.onClick.AddListener(OnOKPressed);
        
        UpdateDisplay();
    }
    
    public void OnNumberPressed(int number)
    {
        if (currentInput.Length < codeLength)
        {
            currentInput += number.ToString();
            UpdateDisplay();
        }
    }
    
    public void OnClearPressed()
    {
        currentInput = "";
        UpdateDisplay();
    }
    
    public void OnOKPressed()
    {
        CheckCode();
    }
    
    private void CheckCode()
    {
        if (currentInput.Length != codeLength) return;
        
        if (currentInput == correctCode)
        {
            if (displayText != null)
            {
                displayText.text = "CORRECT";
                displayText.color = Color.green;
            }
            onCodeCorrect?.Invoke();
        }
        else
        {
            if (displayText != null)
            {
                displayText.text = "ERREUR";
                displayText.color = Color.red;
            }
            onCodeIncorrect?.Invoke();
        }
        
        Invoke(nameof(OnClearPressed), 2f);
    }
    
    private void UpdateDisplay()
    {
        if (displayText == null) return;
        
        string display = "";
        for (int i = 0; i < codeLength; i++)
        {
            display += i < currentInput.Length ? currentInput[i].ToString() : "_";
            if (i < codeLength - 1) display += " ";
        }
        
        displayText.text = display;
        displayText.color = Color.white;
    }
}
