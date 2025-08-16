using UnityEngine;

public class CabinCameraZoomAndMove : MonoBehaviour
{
    public Camera mainCamera;

    // FOV settings
    public float zoomFOV = 30f;
    public float normalFOV = 60f;

    //11 positions relative to player
    public Vector3 zoomLocalPos = new Vector3(0, 3f, -5f);
    public Vector3 normalLocalPos = new Vector3(0, 10f, -15f);

    public float transitionSpeed = 5f; // smooth transition speed

    private bool isZoomed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            isZoomed = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            isZoomed = false;
    }

    private void Update()
    {
        // Smoothly change FOV
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView,
                                            isZoomed ? zoomFOV : normalFOV,
                                            Time.deltaTime * transitionSpeed);

        // Smoothly chhnage localPosition
        mainCamera.transform.localPosition = Vector3.Lerp(mainCamera.transform.localPosition,
                                                          isZoomed ? zoomLocalPos : normalLocalPos,
                                                          Time.deltaTime * transitionSpeed);
    }
}
