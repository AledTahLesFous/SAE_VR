using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DoorXRKey : MonoBehaviour
{
    public Animator animator;
    public string openTriggerName = "Open";

    [Header("XR Socket")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor keySocket;
    public GameObject requiredKey;

    bool opened = false;

void Start()
{
    if (!animator) animator = GetComponent<Animator>();

    // Abonnement à l'évènement du socket
    keySocket.selectEntered.AddListener(OnKeyInserted);

    // Vérifie si un objet est déjà dans le socket
    if (keySocket.firstInteractableSelected != null &&
        keySocket.firstInteractableSelected.transform.gameObject == requiredKey)
    {
        UnlockDoor();
    }
}


    void OnDestroy()
    {
        // Toujours se désabonner proprement
        keySocket.selectEntered.RemoveListener(OnKeyInserted);
    }

    void OnKeyInserted(SelectEnterEventArgs args)
    {
        // Vérifie que l'objet inséré est bien la bonne clé
        if (opened) return;
        if (args.interactableObject.transform.gameObject != requiredKey) return;

        opened = true;
        UnlockDoor();
    }

    void UnlockDoor()
    {
        animator.SetTrigger(openTriggerName);
    }
}
