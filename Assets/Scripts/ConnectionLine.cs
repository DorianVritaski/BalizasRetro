using UnityEngine;

/// <summary>
/// Gestiona una línea de conexión individual, permitiendo su eliminación.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ConnectionLine : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private MeshCollider meshCollider;

    // Los dos anclajes que esta línea conecta.
    public BeaconAnchor anchorA;
    public BeaconAnchor anchorB;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }

    void Update()
    {
        // Actualiza constantemente el MeshCollider para que coincida con la forma de la línea.
        // Esto es necesario por si las balizas se movieran en el futuro.
        UpdateColliderShape();
    }

    private void UpdateColliderShape()
    {
        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, true);
        meshCollider.sharedMesh = mesh;
    }

    // Se llama cuando el puntero del mouse hace clic sobre el collider.
    void OnMouseOver()
    {
        // Detecta el clic derecho (botón 1).
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log($"Eliminando conexión: {name}");

            // Elimina la referencia de la conexión en ambos anclajes.
            if (anchorA != null) anchorA.connections.Remove(anchorB);
            if (anchorB != null) anchorB.connections.Remove(anchorA);

            // Destruye el objeto de la línea.
            Destroy(gameObject);
        }
    }
}
