using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

public class DoorXRKey : MonoBehaviour
{
    public Animator animator;
    public string openTriggerName = "Open";

    [Header("XR Socket")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor keySocket;
    public GameObject requiredKey;
    
    [Header("Collision Settings")]
    [Tooltip("Collider de la porte à désactiver quand elle s'ouvre")]
    public Collider doorCollider;
    
    [Header("Key Orientation")]
    [Tooltip("Rotation à ajouter à la clé (en degrés) pour la redresser")]
    [SerializeField] private Vector3 keyRotationOffset = Vector3.zero;
    [Tooltip("Position à ajouter à la clé (en mètres) pour l'ajuster")]
    [SerializeField] private Vector3 keyPositionOffset = Vector3.zero;

    bool opened = false;

    void Start()
    {
        if (!animator) animator = GetComponent<Animator>();

        // Abonnement aux évènements du socket
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
        
        // NE PAS repositionner la clé - le socket l'a déjà bien placée
        // Juste attendre une frame pour que le socket finisse son placement
        StartCoroutine(LockKeyInPlace());
    }
    
    private IEnumerator LockKeyInPlace()
    {
        // Attendre que le socket finisse de positionner la clé
        yield return new WaitForEndOfFrame();
        
        // Forcer la position exacte du socket (attach point ou socket lui-même)
        Transform socketAttach = keySocket.attachTransform != null ? keySocket.attachTransform : keySocket.transform;
        requiredKey.transform.rotation = socketAttach.rotation * Quaternion.Euler(keyRotationOffset);
        requiredKey.transform.position = socketAttach.position + socketAttach.TransformDirection(keyPositionOffset);
        
        // Empêcher de retirer la clé - désactiver l'interactable
        XRGrabInteractable grabInteractable = requiredKey.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }
        
        // Fixer la clé en place (désactiver la physique)
        Rigidbody keyRigidbody = requiredKey.GetComponent<Rigidbody>();
        if (keyRigidbody != null)
        {
            keyRigidbody.isKinematic = true;
            keyRigidbody.useGravity = false;
        }
        
        // Attacher la clé au cadenas (elle suivra le mouvement de la porte)
        requiredKey.transform.SetParent(keySocket.transform, true);
        
        UnlockDoor();
    }

    void UnlockDoor()
    {
        animator.SetTrigger(openTriggerName);
        
        // Désactiver la collision de la porte
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }
        
        // Désactiver aussi le collider de la clé
        if (requiredKey != null)
        {
            Collider keyCollider = requiredKey.GetComponent<Collider>();
            if (keyCollider != null)
            {
                keyCollider.enabled = false;
            }
        }
    }
}
