using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FlatTargetCollider : MonoBehaviour
{
    void Start()
    {
        SetupFlatCollider();
    }
    
    void SetupFlatCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        
        // Делаем коллайдер тонким как мишень
        Vector3 size = boxCollider.size;
        size.z = 0.05f; // Толщина
        boxCollider.size = size;
        
        // Центрируем
        boxCollider.center = Vector3.zero;
    }
    
    void OnDrawGizmosSelected()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
            
            // Показываем область попадания (круг внутри квадрата)
            Gizmos.color = Color.yellow;
            float radius = Mathf.Min(box.size.x, box.size.y) / 2f;
            DrawWireCircle(Vector3.zero, radius, 32);
        }
    }
    
    void DrawWireCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 nextPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0
            );
            
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
}