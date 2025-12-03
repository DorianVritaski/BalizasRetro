using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Gestiona un punto de anclaje en el mapa donde se puede colocar una baliza.
/// </summary>
public class BeaconAnchor : MonoBehaviour
{
    public bool isOccupied = false; // Para saber si ya hay una baliza aquí.
    public bool isStable = true;    // Para saber si la baliza está estabilizada.
    public GameObject beaconPrefab; // Referencia al prefab de la baliza que se va a instanciar.

    // Lista de anclajes a los que está conectado directamente.
    public List<BeaconAnchor> connections = new List<BeaconAnchor>();

    private SpriteRenderer beaconSpriteRenderer; // Referencia al sprite de la baliza creada.
    private Color originalColor = Color.white; // Color original de la baliza.
    private LevelHandler levelHandler;

    void Start()
    {
        // Obtenemos la referencia al LevelHandler para poder reproducir sonidos.
        levelHandler = FindObjectOfType<LevelHandler>();
    }

    // Este método se llama cuando se hace clic sobre el objeto con el mouse.
    void OnMouseDown()
    {
        // Solo reacciona si el juego no está pausado o en la pantalla final.
        if (PauseControl.GameIsPaused || GUIHandler.IsEndGame) return;

        if (isOccupied)
        {
            if (!isStable)
            {
                Debug.Log($"Intentando estabilizar la baliza en {transform.position}.");
                // Inicia el mini-juego de estabilización.
                StabilizationMinigame.Instance.StartMinigame(this);
            }
            else {
                // Si la baliza está estable, intentamos crear una conexión.
                ConnectionManager.Instance.OnBeaconClicked(this);
            }
        }
        else
        {
            Debug.Log($"Punto de anclaje vacío clickeado en {transform.position}. ¡Colocando baliza!");
            if (beaconPrefab != null)
            {
                // Creamos la baliza y guardamos una referencia a su SpriteRenderer.
                GameObject beaconGO = Instantiate(beaconPrefab, transform.position, Quaternion.identity, this.transform);
                beaconSpriteRenderer = beaconGO.GetComponent<SpriteRenderer>();
                if (beaconSpriteRenderer != null)
                {
                    originalColor = beaconSpriteRenderer.color;
                }
                isOccupied = true;
                isStable = true;

                // Reproducimos el sonido de colocación.
                if (levelHandler != null)
                {
                    levelHandler.PlayPlaceBeaconAudio();
                }
            }
        }
    }

    /// <summary>
    /// Cambia el color de la baliza para resaltarla o devolverla a su estado original.
    /// </summary>
    /// <param name="isHighlighted">True para resaltar, false para el color original.</param>
    public void SetHighlight(bool isHighlighted)
    {
        if (beaconSpriteRenderer == null) return;

        beaconSpriteRenderer.color = isHighlighted ? Color.yellow : originalColor;
    }

    /// <summary>
    /// Cambia el estado de estabilidad de la baliza y actualiza su color.
    /// </summary>
    /// <param name="stable">True si la baliza es estable, false si es inestable.</param>
    public void SetStability(bool stable)
    {
        isStable = stable;
        if (beaconSpriteRenderer == null) return;

        // Si es inestable, se pone roja. Si se estabiliza, vuelve a su color original.
        originalColor = Color.white; // Aseguramos el color base.
        beaconSpriteRenderer.color = isStable ? originalColor : Color.red;

        // Si la baliza se vuelve inestable, rompemos todas sus conexiones.
        if (!stable)
        {
            BreakAllConnections();
        }
    }

    private void BreakAllConnections()
    {
        // Usamos ToList() para crear una copia de la lista, porque vamos a modificarla mientras la recorremos.
        foreach (var otherAnchor in connections.ToList())
        {
            // Buscamos la línea que conecta este anclaje con el otro y la destruimos.
            ConnectionManager.Instance.RemoveConnection(this, otherAnchor);
        }
    }
}
