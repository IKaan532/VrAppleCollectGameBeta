using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AppleSc : MonoBehaviour
{
    private XRGrabInteractable grab;
    private Rigidbody rb;

    private float holdTime = 0f;
    private bool isBeingHeld = false;

    void Start()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void Update()
    {
        if (isBeingHeld)
        {
            holdTime += Time.deltaTime;
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isBeingHeld = true;
        holdTime = 0f;

        if (rb != null)
        {
            rb.useGravity = false; 
        }

        Debug.Log("Elma tutuldu.");
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isBeingHeld = false;

        if (rb != null)
        {
            rb.useGravity = true; 
        }

        Debug.Log("Elma býrakýldý. Tutma süresi: " + holdTime + " saniye");
    }
}
