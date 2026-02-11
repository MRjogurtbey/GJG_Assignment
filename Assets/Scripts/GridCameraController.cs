using UnityEngine;

/// <summary>
/// Kamerayı ızgara boyutlarına göre dinamik olarak konumlandırır.
/// </summary>
public class GridCameraController : MonoBehaviour
{
    private Camera _cam;

    private void Awake() => _cam = GetComponent<Camera>();

    public void Adjust(int width, int height)
    {
        float centerX = (width - 1) / 2f;
        float centerY = (height - 1) / 2f;
        transform.position = new Vector3(centerX, centerY, -10f);

        float aspect = (float)Screen.width / Screen.height;
        float padding = 1.5f;
        float vSize = (height / 2f) + padding;
        float hSize = ((width / 2f) / aspect) + padding;

        _cam.orthographicSize = Mathf.Max(vSize, hSize);
    }
}