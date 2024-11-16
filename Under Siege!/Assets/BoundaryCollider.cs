using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(BoxCollider2D))]
public class BoundaryCollider : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private BoxCollider2D boxCollider;

    void Start()
    {
        // Get LineRenderer and BoxCollider2D components
        lineRenderer = GetComponent<LineRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        // Check if there are at least 2 points in the LineRenderer
        if (lineRenderer.positionCount >= 2)
        {
            // Calculate the boundary based on the first and second points in the LineRenderer
            Vector3 startPoint = lineRenderer.GetPosition(0);
            Vector3 endPoint = lineRenderer.GetPosition(1);

            // Calculate the center point and size for the BoxCollider2D
            Vector3 center = (startPoint + endPoint) / 2;
            boxCollider.offset = transform.InverseTransformPoint(center); // Set center of collider

            // Calculate the size by finding the distance between points in the X or Y direction
            float width = Mathf.Abs(endPoint.x - startPoint.x);
            float height = Mathf.Abs(endPoint.y - startPoint.y);

            // Set the size of the BoxCollider based on width or height
            boxCollider.size = new Vector2(width > 0 ? width : 0.1f, height > 0 ? height : 0.1f); // Set to a thin line if width/height is zero
        }
    }
}
