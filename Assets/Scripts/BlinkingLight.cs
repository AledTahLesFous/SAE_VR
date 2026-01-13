using UnityEngine;
using System.Collections;

/// <summary>
/// Fait clignoter une ou plusieurs lumières avec différents modes et paramètres
/// </summary>
public class BlinkingLight : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Les lumières à faire clignoter")]
    [SerializeField] private Light[] lights;
    
    [Tooltip("Mode de clignotement")]
    [SerializeField] private BlinkMode mode = BlinkMode.OnOff;
    
    [Header("Paramètres de clignotement")]
    [Tooltip("Durée d'allumage (en secondes)")]
    [Range(0.05f, 5f)]
    [SerializeField] private float onDuration = 0.5f;
    
    [Tooltip("Durée d'extinction (en secondes)")]
    [Range(0.05f, 5f)]
    [SerializeField] private float offDuration = 0.5f;
    
    [Tooltip("Ajouter une variation aléatoire aux durées")]
    [SerializeField] private bool randomVariation = false;
    
    [Tooltip("Variation aléatoire maximum (en pourcentage)")]
    [Range(0f, 0.5f)]
    [SerializeField] private float variationAmount = 0.2f;
    
    [Header("Paramètres d'intensité (mode Flicker)")]
    [Tooltip("Intensité minimale")]
    [Range(0f, 8f)]
    [SerializeField] private float minIntensity = 0.3f;
    
    [Tooltip("Intensité maximale")]
    [Range(0f, 8f)]
    [SerializeField] private float maxIntensity = 1f;
    
    [Tooltip("Vitesse du clignotement (mode Flicker)")]
    [Range(0.01f, 1f)]
    [SerializeField] private float flickerSpeed = 0.1f;
    
    [Header("Contrôle")]
    [Tooltip("Démarrer le clignotement automatiquement")]
    [SerializeField] private bool startOnAwake = true;
    
    private bool isBlinking = false;
    private float[] originalIntensities;
    
    public enum BlinkMode
    {
        OnOff,          // Allume/éteint complètement
        Flicker,        // Vacillement (variation d'intensité)
        Pulse,          // Pulsation douce
        Random,         // Clignotement aléatoire
        Realistic       // Lumière défectueuse réaliste (périodes noires + reprise progressive)
    }
    
    void Awake()
    {
        // Si aucune lumière n'est assignée, chercher sur le GameObject
        if (lights == null || lights.Length == 0)
        {
            Light light = GetComponent<Light>();
            if (light != null)
            {
                lights = new Light[] { light };
            }
        }
        
        // Sauvegarder les intensités originales
        if (lights != null && lights.Length > 0)
        {
            originalIntensities = new float[lights.Length];
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    originalIntensities[i] = lights[i].intensity;
                }
            }
        }
    }
    
    void Start()
    {
        if (startOnAwake)
        {
            StartBlinking();
        }
    }
    
    /// <summary>
    /// Démarre le clignotement
    /// </summary>
    public void StartBlinking()
    {
        if (!isBlinking)
        {
            isBlinking = true;
            
            switch (mode)
            {
                case BlinkMode.OnOff:
                    StartCoroutine(BlinkOnOff());
                    break;
                case BlinkMode.Flicker:
                    StartCoroutine(FlickerEffect());
                    break;
                case BlinkMode.Pulse:
                    StartCoroutine(PulseEffect());
                    break;
                case BlinkMode.Random:
                    StartCoroutine(RandomBlink());
                    break;
                case BlinkMode.Realistic:
                    StartCoroutine(RealisticFlicker());
                    break;
            }
        }
    }
    
    /// <summary>
    /// Arrête le clignotement
    /// </summary>
    public void StopBlinking()
    {
        isBlinking = false;
        StopAllCoroutines();
        
        // Restaurer les intensités originales
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
            {
                lights[i].enabled = true;
                lights[i].intensity = originalIntensities[i];
            }
        }
    }
    
    /// <summary>
    /// Mode On/Off - Allume et éteint complètement
    /// </summary>
    private IEnumerator BlinkOnOff()
    {
        while (isBlinking)
        {
            // Allumer
            SetLightsEnabled(true);
            yield return new WaitForSeconds(GetDuration(onDuration));
            
            // Éteindre
            SetLightsEnabled(false);
            yield return new WaitForSeconds(GetDuration(offDuration));
        }
    }
    
    /// <summary>
    /// Mode Flicker - Vacillement rapide de l'intensité
    /// </summary>
    private IEnumerator FlickerEffect()
    {
        while (isBlinking)
        {
            float targetIntensity = Random.Range(minIntensity, maxIntensity);
            
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    lights[i].enabled = true;
                    lights[i].intensity = targetIntensity * originalIntensities[i];
                }
            }
            
            yield return new WaitForSeconds(flickerSpeed);
        }
    }
    
    /// <summary>
    /// Mode Pulse - Pulsation douce
    /// </summary>
    private IEnumerator PulseEffect()
    {
        float time = 0f;
        
        while (isBlinking)
        {
            time += Time.deltaTime;
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, 
                (Mathf.Sin(time * Mathf.PI / onDuration) + 1f) * 0.5f);
            
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    lights[i].enabled = true;
                    lights[i].intensity = intensity * originalIntensities[i];
                }
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Mode Random - Clignotement aléatoire
    /// </summary>
    private IEnumerator RandomBlink()
    {
        while (isBlinking)
        {
            bool turnOn = Random.value > 0.5f;
            SetLightsEnabled(turnOn);
            
            float duration = Random.Range(offDuration * 0.5f, onDuration * 1.5f);
            yield return new WaitForSeconds(duration);
        }
    }
    
    /// <summary>
    /// Active ou désactive toutes les lumières
    /// </summary>
    private void SetLightsEnabled(bool enabled)
    {
        foreach (Light light in lights)
        {
            if (light != null)
            {
                light.enabled = enabled;
            }
        }
    }
    
    /// <summary>
    /// Obtient une durée avec variation aléatoire si activée
    /// </summary>
    private float GetDuration(float baseDuration)
    {
        if (randomVariation)
        {
            float variation = baseDuration * variationAmount;
            return baseDuration + Random.Range(-variation, variation);
        }
        return baseDuration;
    }
    
    /// <summary>
    /// Change le mode de clignotement
    /// </summary>
    public void SetBlinkMode(BlinkMode newMode)
    {
        if (mode != newMode)
        {
            bool wasBlinking = isBlinking;
            if (wasBlinking)
            {
                StopBlinking();
            }
            
            mode = newMode;
            
            if (wasBlinking)
            {
                StartBlinking();
            }
        }
    }
    
    /// <summary>
    /// Mode Realistic - Simule une lumière défectueuse réaliste
    /// Avec des périodes d'extinction et une reprise brutale
    /// </summary>
    private IEnumerator RealisticFlicker()
    {
        while (isBlinking)
        {
            // Phase stable (lumière allumée normalement)
            float stableDuration = Random.Range(1.5f, 4f);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    lights[i].enabled = true;
                    lights[i].intensity = originalIntensities[i];
                }
            }
            yield return new WaitForSeconds(stableDuration);
            
            // Début du dysfonctionnement - quelques vacillements rapides
            for (int flicker = 0; flicker < Random.Range(2, 4); flicker++)
            {
                float flickerIntensity = Random.Range(0.4f, 0.8f);
                for (int i = 0; i < lights.Length; i++)
                {
                    if (lights[i] != null)
                    {
                        lights[i].intensity = originalIntensities[i] * flickerIntensity;
                    }
                }
                yield return new WaitForSeconds(Random.Range(0.03f, 0.1f));
            }
            
            // Extinction brutale (moment noir)
            SetLightsEnabled(false);
            float blackoutDuration = Random.Range(0.5f, 2f);
            yield return new WaitForSeconds(blackoutDuration);
            
            // Tentatives de rallumage (plusieurs essais)
            int attempts = Random.Range(2, 3);
            for (int attempt = 0; attempt < attempts; attempt++)
            {
                // Brève tentative de rallumage
                for (int i = 0; i < lights.Length; i++)
                {
                    if (lights[i] != null)
                    {
                        lights[i].enabled = true;
                        lights[i].intensity = originalIntensities[i] * Random.Range(0.3f, 0.6f);
                    }
                }
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
                
                // Retombe brutalement
                SetLightsEnabled(false);
                yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
            }
            
            // Rallumage brutal (d'un coup)
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    lights[i].enabled = true;
                    lights[i].intensity = originalIntensities[i];
                }
            }
        }
    }
    
    void OnDisable()
    {
        StopBlinking();
    }
}
