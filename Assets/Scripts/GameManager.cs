using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Manages the core game logic, including path validation and win/loss conditions.
/// </summary>
public class GameManager : MonoBehaviour
{
    // --- Propiedades del antiguo juego, se mantienen por si se reutilizan, pero no se usan ahora ---
    bool flowsStarted;
    bool isWon;

    // List of boolean flags to keep track of whether each flow has finished.
    List<bool> flowsFinished;

    // The LevelHandler for the current level.
    LevelHandler lh;

    // The GUIHandler for the current level.
    GUIHandler GUIHandler;

    void Start()
    {
        lh = GameObject.FindObjectOfType<LevelHandler>();
        GUIHandler = GetComponent<GUIHandler>();
        isWon = false;
        flowsStarted = false;
    }

    /// <summary>
    /// Inicia el proceso de validación de la ruta de balizas.
    /// </summary>
    public void ValidateBeaconPath()
    {
        Debug.Log("Iniciando validación de la ruta...");

        // 1. Encontrar todos los anclajes en la escena.
        BeaconAnchor[] allAnchors = FindObjectsOfType<BeaconAnchor>();

        // 2. Verificar si se han colocado las 4 balizas.
        int placedBeacons = 0;
        foreach (var anchor in allAnchors)
        {
            if (anchor.isOccupied)
            {
                placedBeacons++;
            }
        }

        if (placedBeacons < 4)
        {
            Debug.LogWarning("No se han colocado las 4 balizas. Faltan " + (4 - placedBeacons));
            // Opcional: Mostrar un mensaje al jugador.
            isWon = false;
            GUIHandler.ShowEndGameMenu(isWon);
            return;
        }

        // 3. Usar Búsqueda en Amplitud (BFS) para ver cuántas balizas están conectadas.
        HashSet<BeaconAnchor> visited = new HashSet<BeaconAnchor>();
        Queue<BeaconAnchor> queue = new Queue<BeaconAnchor>();

        // Empezamos desde la primera baliza que encontremos.
        BeaconAnchor startNode = allAnchors.First(a => a.isOccupied);
        queue.Enqueue(startNode);
        visited.Add(startNode);

        while (queue.Count > 0)
        {
            BeaconAnchor current = queue.Dequeue();

            foreach (BeaconAnchor neighbor in current.connections)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        // 4. Comprobar si el número de balizas visitadas es igual a 4.
        bool allConnected = visited.Count == 4;

        // 5. Comprobar si todas las balizas conectadas están estables.
        bool allStable = true;
        foreach (var anchor in visited) {
            if (!anchor.isStable) {
                allStable = false;
                break;
            }
        }

        isWon = allConnected && allStable;

        Debug.Log($"Validación completada. Balizas conectadas: {visited.Count}. Resultado: {(isWon ? "¡GANASTE!" : "PERDISTE")}");

        // 6. Mostrar el menú de fin de juego.
        GUIHandler.ShowEndGameMenu(isWon);
    }
}