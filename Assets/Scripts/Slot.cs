using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Slot : MonoBehaviour
{
    public GameObject currentItem;
    public Image slotImage;
    public InputActionReference releaseAction;

    private Color originalColor;

    void Start()
    {
        slotImage = GetComponentInChildren<Image>();
        originalColor = slotImage.color;
    }

    private void OnEnable()
    {
        releaseAction.action.Enable();
    }

    private void OnDisable()
    {
        releaseAction.action.Disable();
    }

    private void OnTriggerStay(Collider other)
    {
        if (currentItem != null) return;

        Item item = other.GetComponent<Item>();
        if (item == null || item.inSlot) return;

        if (releaseAction.action.WasReleasedThisFrame())
        {
            InsertItem(other.gameObject);
        }
    }

    void InsertItem(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        obj.transform.SetParent(transform, true);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localEulerAngles = obj.GetComponent<Item>().slotRotation;

        Item item = obj.GetComponent<Item>();
        item.inSlot = true;
        item.currentSlot = this;

        currentItem = obj;
        slotImage.color = Color.green;
    }

    public void ResetColor()
    {
        slotImage.color = originalColor;
    }
}
