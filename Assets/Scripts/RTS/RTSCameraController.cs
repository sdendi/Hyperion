using UnityEngine;

namespace RTS
{
    public class RTSCameraController : MonoBehaviour
    {
        [Header("Pan Settings")]
        [SerializeField] private float panSpeed = 20f;
        [SerializeField] private float panBorderThickness = 10f;
        [SerializeField] private bool enableScreenEdgePan = false;
        [SerializeField] private Vector2 panLimitX = new Vector2(-40f, 40f);
        [SerializeField] private Vector2 panLimitZ = new Vector2(-40f, 40f);

        [Header("Zoom Settings")]
        [SerializeField] private float zoomSpeed = 25f;
        [SerializeField] private float minHeight = 5f;
        [SerializeField] private float maxHeight = 30f;

        private void Update()
        {
            Vector3 pos = transform.position;

            // Keyboard Pan
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            // Edge Pan (optional)
            if (enableScreenEdgePan)
            {
                if (Input.mousePosition.y >= Screen.height - panBorderThickness) v += 1;
                if (Input.mousePosition.y <= panBorderThickness) v -= 1;
                if (Input.mousePosition.x >= Screen.width - panBorderThickness) h += 1;
                if (Input.mousePosition.x <= panBorderThickness) h -= 1;
            }

            Vector3 forward = transform.forward;
            forward.y = 0;
            forward.Normalize();
            Vector3 right = transform.right;
            right.y = 0;
            right.Normalize();

            Vector3 move = (forward * v + right * h).normalized;
            pos += move * (panSpeed * Time.deltaTime);

            // Zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            pos.y -= scroll * zoomSpeed * 100f * Time.deltaTime;
            pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);

            pos.x = Mathf.Clamp(pos.x, panLimitX.x, panLimitX.y);
            pos.z = Mathf.Clamp(pos.z, panLimitZ.x, panLimitZ.y);

            transform.position = pos;
        }
    }
}
