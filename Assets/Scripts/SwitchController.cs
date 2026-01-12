using UnityEngine;


public class XRSwitch : MonoBehaviour
{
    [Header("Switch Visuals")]
    public GameObject onVisual;    // enfant On
    public GameObject offVisual;   // enfant Off

    [Header("Lights to Toggle")]
    public Light[] targetLights;   // Tableau de lumières à contrôler

    private bool isOn = false;     // état du switch
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    void Awake()
    {
        // Récupère le composant XR Interactable
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

        if (interactable != null)
        {
            // Ajoute l'événement de sélection VR
            interactable.selectEntered.AddListener(_ => Toggle());
        }

        // Met à jour l'état visuel et lumières au démarrage
        UpdateVisuals();
    }

    // Change l'état du switch
    void Toggle()
    {
        isOn = !isOn;
        UpdateVisuals();
    }

    // Active le visuel correspondant et toutes les lumières
    void UpdateVisuals()
    {
        if (onVisual != null) onVisual.SetActive(isOn);
        if (offVisual != null) offVisual.SetActive(!isOn);

        // Active ou désactive TOUTES les lumières du tableau
        foreach (Light light in targetLights)
        {
            if (light != null)
            {
                light.enabled = isOn;
            }
        }
    }
}