using UnityEngine;

public class MapCam : MonoBehaviour
{
    [SerializeField]
    private Transform swivel = default;
    [SerializeField]
    private Transform stick = default;
    [SerializeField]
    private float stickMinZoom = default;
    [SerializeField]
    private float stickMaxZoom = default;
    [SerializeField]
    private float swivelMinZoom = default;
    [SerializeField]
    private float swivelMaxZoom = default;

    private float zoom = 1f;

    private void Update()
    {
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        if (zoomDelta != 0f)
            AdjustZoom(zoomDelta);
    }

    private void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);

        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }
}
