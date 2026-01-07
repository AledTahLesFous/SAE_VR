using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class LockSocket : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;
    public bool lockWhenInserted = true;

    void Awake()
    {
        socket.selectEntered.AddListener(OnKeyInserted);
        socket.selectExited.AddListener(OnKeyRemoved);
    }

    void OnKeyInserted(SelectEnterEventArgs args)
    {
        GameObject key = args.interactableObject.transform.gameObject;
        StartCoroutine(LockKeyAfterSnap(key));
    }

    IEnumerator LockKeyAfterSnap(GameObject key)
    {
        // Attend la fin du frame pour que le socket ait fini le snap
        yield return new WaitForEndOfFrame();

        // Parent la clé au socket
        key.transform.SetParent(socket.transform, true);

        // Rends la clé kinematic
        Rigidbody rb = key.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        // Désactive le grab si nécessaire
        if (lockWhenInserted)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab = key.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab != null)
                grab.enabled = false;
        }

        Debug.Log("Clé insérée et verrouillée");
    }

    void OnKeyRemoved(SelectExitEventArgs args)
    {
        GameObject key = args.interactableObject.transform.gameObject;

        // Retire le parent
        key.transform.SetParent(null, true);

        // Réactive la physique
        Rigidbody rb = key.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = false;

    }
}
