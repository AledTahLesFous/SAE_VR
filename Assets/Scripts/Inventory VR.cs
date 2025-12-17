using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryVR : MonoBehaviour
{
    public GameObject inventory;
    public float distanceFromCamera = 1.0f;
    public Vector3 offsetEuler = new Vector3(15f, 0f, 0f);

    public InputActionReference toggleInventoryAction;

    private Camera xrCamera;
    private bool uiActive;

    private void OnEnable()
    {
        if (toggleInventoryAction != null)
        {
            toggleInventoryAction.action.Enable();
            toggleInventoryAction.action.performed += OnToggleInventory;
        }
    }

    private void OnDisable()
    {
        if (toggleInventoryAction != null)
        {
            toggleInventoryAction.action.performed -= OnToggleInventory;
            toggleInventoryAction. action.Disable();
        }
    }

    void Start()
    {
        xrCamera = Camera.main;

        // Inventaire visible au démarrage pour tester
        uiActive = true;
        inventory.SetActive(true);
    }

    // Toggle l'inventaire à chaque pression sur B
    private void OnToggleInventory(InputAction.CallbackContext context)
    {        
        uiActive = !uiActive;
        inventory.SetActive(uiActive);
        
        // Repositionner l'inventaire devant la caméra quand il s'ouvre
        if (uiActive && xrCamera != null)
        {
            UpdateInventoryPosition();
        }
    }

    void LateUpdate()
    {
        if (!uiActive || xrCamera == null) return;

        UpdateInventoryPosition();
    }

    private void UpdateInventoryPosition()
    {
        // Position : devant le regard
        inventory.transform.position =
            xrCamera.transform.position +
            xrCamera.transform.forward * distanceFromCamera;

        // Rotation :  face au regard
        inventory.transform.rotation =
            Quaternion.LookRotation(
                inventory.transform.position - xrCamera.transform.position,
                Vector3.up
            ) * Quaternion.Euler(offsetEuler);
    }
}