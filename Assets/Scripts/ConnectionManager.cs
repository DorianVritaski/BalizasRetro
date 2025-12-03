using UnityEngine;

/// <summary>
/// Gestiona la creación de conexiones (líneas) entre balizas.
/// </summary>
public class ConnectionManager : MonoBehaviour
{
    // Singleton para un fácil acceso desde otros scripts.
    public static ConnectionManager Instance { get; private set; }

    [Header("Configuración de Línea")]
    public GameObject linePrefab; // Asignaremos un prefab con un LineRenderer.

    private BeaconAnchor startAnchor; // El punto de inicio de la conexión.

    void Awake()
    {
        // Configuración del Singleton.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Este método es llamado por una baliza cuando se hace clic en ella.
    /// </summary>
    public void OnBeaconClicked(BeaconAnchor clickedAnchor)
    {
        if (startAnchor == null)
        {
            // Si no hay una conexión iniciada, esta baliza es el punto de partida.
            startAnchor = clickedAnchor;
            Debug.Log($"Iniciando conexión desde: {startAnchor.name}");
            startAnchor.SetHighlight(true); // Resaltamos la baliza seleccionada.
        }
        else if (startAnchor != clickedAnchor)
        {
            // Si ya había una baliza de inicio, y la nueva es diferente, creamos la conexión.
            Debug.Log($"Completando conexión hacia: {clickedAnchor.name}");
            CreateConnection(startAnchor, clickedAnchor);

            // Reseteamos para poder crear una nueva conexión.
            startAnchor.SetHighlight(false); // Devolvemos la baliza a su color original.
            startAnchor = null;
        }
        else
        {
            // Si se vuelve a hacer clic en la misma baliza, se cancela la conexión.
            Debug.Log("Conexión cancelada.");
            startAnchor.SetHighlight(false); // Devolvemos la baliza a su color original.
            startAnchor = null;
        }
    }

    void CreateConnection(BeaconAnchor from, BeaconAnchor to)
    {
        if (linePrefab == null)
        {
            Debug.LogError("¡Falta el prefab de la línea en ConnectionManager!");
            return;
        }

        // Creamos una instancia de nuestra línea.
        GameObject lineGO = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        lineGO.name = $"Connection_{from.name}-{to.name}";

        // Obtenemos el componente LineRenderer y establecemos los puntos.
        LineRenderer lr = lineGO.GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, from.transform.position);
        lr.SetPosition(1, to.transform.position);

        // Asignamos los anclajes al script de la línea para poder borrarlos después.
        ConnectionLine lineScript = lineGO.GetComponent<ConnectionLine>();
        lineScript.anchorA = from;
        lineScript.anchorB = to;

        // Registramos la conexión en ambos anclajes.
        from.connections.Add(to);
        to.connections.Add(from);
    }

    /// <summary>
    /// Busca y elimina la línea de conexión entre dos anclajes.
    /// </summary>
    public void RemoveConnection(BeaconAnchor anchor1, BeaconAnchor anchor2)
    {
        ConnectionLine[] allLines = FindObjectsOfType<ConnectionLine>();
        foreach (ConnectionLine line in allLines)
        {
            // Comprueba si la línea conecta los dos anclajes, sin importar el orden.
            bool isMatch = (line.anchorA == anchor1 && line.anchorB == anchor2) ||
                           (line.anchorA == anchor2 && line.anchorB == anchor1);

            if (isMatch)
            {
                // Elimina la referencia de la conexión en ambos anclajes.
                anchor1.connections.Remove(anchor2);
                anchor2.connections.Remove(anchor1);
                Destroy(line.gameObject);
                break; // Salimos del bucle una vez encontrada y destruida la línea.
            }
        }
    }
}
