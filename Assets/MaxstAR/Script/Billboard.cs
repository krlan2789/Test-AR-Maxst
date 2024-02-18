using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform arCameraTransform;

    void Update()
    {
        if (arCameraTransform != null)
        {
            Vector3 savedAngles = new(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);
            transform.LookAt(arCameraTransform, transform.up);
            transform.localEulerAngles = new Vector3(savedAngles.x, transform.localEulerAngles.y, savedAngles.z);
        }
    }
}
