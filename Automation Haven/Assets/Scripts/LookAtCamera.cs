using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour {
    private enum Mode {
        LookAt,
        LookAtInverted,
        CameraForward,
        CameraForwardInverted,
    }

    [SerializeField] private Mode mode;

    private float minXRotation = -25;
    private float maxXRotation = 25;


    private void LateUpdate() {
        switch (mode) {
            case Mode.LookAt:
                transform.LookAt(Camera.main.transform);
                break;
            case Mode.LookAtInverted:
                Vector3 directionToCamera = Camera.main.transform.position - transform.position;
                Quaternion lookRotation = Quaternion.LookRotation(-directionToCamera);

                // Convert to Euler angles to constrain the x-axis rotation
                Vector3 eulerRotation = lookRotation.eulerAngles;
                eulerRotation.x = Mathf.Clamp(eulerRotation.x, minXRotation, maxXRotation); // Set your desired min and max x values

                // Convert back to Quaternion and apply the rotation
                transform.rotation = Quaternion.Euler(eulerRotation);
                break;
            case Mode.CameraForward:
                transform.forward = Camera.main.transform.forward;
                break;

            case Mode.CameraForwardInverted:
                transform.forward = -Camera.main.transform.forward;
                break;
        }

    }
}
