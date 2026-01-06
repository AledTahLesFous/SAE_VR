using UnityEngine;


public class XRSwitch : MonoBehaviour
{
    public GameObject onVisual;
    public GameObject offVisual;
    private bool isOn = false;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        interactable.selectEntered.AddListener(_ => Toggle());
        UpdateVisual();
    }

    void Toggle()
    {
        isOn = !isOn;
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (onVisual != null) onVisual.SetActive(isOn);
        if (offVisual != null) offVisual.SetActive(!isOn);
    }
}
