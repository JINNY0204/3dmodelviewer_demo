using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    public Texture2D grabCursor;
    Transform Pivot;

    public float rotationSpeed = 2;
    public float smoothFactor = 0.1f;
    public float distanceToCam;

    private bool isDragging = false;
    private Vector3 lastMousePosition;
    private Quaternion targetRotation;

    void Start()
    {
        CreatePivot();
        targetRotation = Pivot.rotation;
    }

    void OnEnable()
    {
        AdjustDistanceToCamera();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;

            Cursor.SetCursor(grabCursor, Vector2.zero, CursorMode.ForceSoftware);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
        }

        if (isDragging)
        {
            //float scaleFactor = Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);
            //float rotationSpeed = baseRotationSpeed / scaleFactor;

            Vector3 deltaMousePosition = Input.mousePosition - lastMousePosition;
            float rotationX = deltaMousePosition.y * rotationSpeed;
            float rotationY = -deltaMousePosition.x * rotationSpeed;

            Quaternion deltaRotationX = Quaternion.AngleAxis(rotationX, Vector3.right);
            Quaternion deltaRotationY = Quaternion.AngleAxis(rotationY, Vector3.up);
            targetRotation = deltaRotationY * deltaRotationX * targetRotation;

            lastMousePosition = Input.mousePosition;
        }

        Pivot.rotation = Quaternion.Slerp(Pivot.rotation, targetRotation, Time.deltaTime / smoothFactor);
    }

    void CreatePivot()
    {
        Pivot = new GameObject(transform.name).transform;
        Pivot.position = GetBounds().center;
        transform.SetParent(Pivot);
    }

    void AdjustDistanceToCamera()
    {
        Bounds bounds = GetBounds();
        float maxBoundsSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float distance = distanceToCam * maxBoundsSize;

        Vector3 targetPosition = Camera.main.transform.position + Camera.main.transform.forward * distance;
        transform.position = targetPosition;
    }

    Bounds GetBounds()
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        bool hasBounds = false;

        Renderer[] rends = GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            foreach (Renderer render in rends)
            {
                if (!render.enabled) continue;

                if (hasBounds)
                {
                    bounds.Encapsulate(render.bounds);
                }
                else
                {
                    bounds = render.bounds;
                    hasBounds = true;
                }
            }
            return bounds;
        }
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            return collider.bounds;
        }

        return new Bounds(transform.position, Vector3.zero);
    }
}
