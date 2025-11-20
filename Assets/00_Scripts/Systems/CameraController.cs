// Assets/00_Scripts/Systems/CameraController.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Camera Movement")]
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 10f;

    [Header("Camera Bounds")]
    [SerializeField] private Vector2 mapSize = new Vector2(50f, 50f);

    private Vector3 dragOrigin;
    private bool isDragging = false;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!IsPointerOverUI())
        {
            HandlePan();
            HandleZoom();
        }
        ClampCameraPosition();
    }

    private void HandlePan()
    {
        Mouse mouse = Mouse.current;

        // Начало перетаскивания
        if (mouse.leftButton.wasPressedThisFrame)
        {
            dragOrigin = GetMouseWorldPosition();
            isDragging = true;
        }

        // Перетаскивание
        if (mouse.leftButton.isPressed && isDragging)
        {
            Vector3 difference = dragOrigin - GetMouseWorldPosition();
            transform.position += difference * panSpeed * Time.deltaTime;
        }

        // Конец перетаскивания
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }

    private void HandleZoom()
    {
        Mouse mouse = Mouse.current;
        float scroll = mouse.scroll.ReadValue().y;

        if (scroll != 0)
        {
            mainCamera.orthographicSize = Mathf.Clamp(
                mainCamera.orthographicSize - scroll * zoomSpeed * Time.deltaTime,
                minZoom,
                maxZoom
            );
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = -transform.position.z;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    private void ClampCameraPosition()
    {
        if (mainCamera == null) return;

        float vertExtent = mainCamera.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        float minX = horzExtent - mapSize.x / 2f;
        float maxX = mapSize.x / 2f - horzExtent;
        float minY = vertExtent - mapSize.y / 2f;
        float maxY = mapSize.y / 2f - vertExtent;

        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        transform.position = clampedPosition;
    }

    private bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
}