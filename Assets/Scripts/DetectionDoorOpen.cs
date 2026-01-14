using UnityEngine;

public class OpenDoorOnPoint : MonoBehaviour
{
    public Animator doorAnimator;

    private void OnTriggerEnter(Collider other)
    {
        doorAnimator.SetTrigger("Open");
    }
}
