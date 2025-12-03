using UnityEngine;

/// <summary>
/// Handles the setup of the game map and level state.
/// </summary>
public class LevelHandler : MonoBehaviour
{
    [Header("Mapa del Archipiélago")]
    public Sprite archipelagoMapSprite; // Asigna aquí la imagen del mapa desde el Inspector.

    [Header("Balizas y Anclajes")]
    public GameObject beaconAnchorPrefab; // El prefab para los puntos de anclaje (el collider invisible).
    public GameObject beaconPrefab;       // El prefab para la baliza (el objeto visual).
    // Define las 4 coordenadas donde se pueden colocar las balizas.
    public Vector2[] beaconLocations = new Vector2[4];

    [Header("Audio")]
    public AudioSource pipeRotationAudio; // Se mantendrá por si se necesita en el futuro
    public AudioSource placeBeaconAudio;
    public AudioSource winningAudio;

    [Header("Configuración de Nivel (Debug)")]
    [SerializeField][Range(1, 15)] int levelNum = 1;
    [SerializeField] bool arcadeMode = false;

    void Start()
    {
        // Cambiamos el color de fondo de la cámara para que coincida con los menús
        Camera.main.backgroundColor = new Color32(23, 32, 42, 255);

        // Establece valores por defecto si no se inician desde el menú principal
        if (LevelData.LevelNumber == 0)
        {
            LevelData.IsArcadeMode = arcadeMode;
            LevelData.LevelNumber = levelNum;
        }

        // Creamos el fondo con el mapa del archipiélago
        CreateArchipelagoBackground();

        // Creamos los puntos de anclaje para las balizas
        CreateBeaconAnchors();

        // Creamos el AudioSource para el sonido de colocar baliza.
        placeBeaconAudio = MenuHandler.GenerateAudioSource("Sounds/click1", "Audio Place Beacon Source");
    }

    /// <summary>
    /// Crea un objeto en la escena para mostrar el mapa del archipiélago.
    /// </summary>
    void CreateArchipelagoBackground()
    {
        if (archipelagoMapSprite == null)
        {
            Debug.LogError("¡No se ha asignado un sprite para el mapa del archipiélago en el LevelHandler!");
            return;
        }

        GameObject backgroundGO = new GameObject("ArchipelagoBackground");
        backgroundGO.transform.SetParent(this.transform);
        
        SpriteRenderer renderer = backgroundGO.AddComponent<SpriteRenderer>();
        renderer.sprite = archipelagoMapSprite;
        renderer.sortingOrder = -10; // Para asegurarnos de que esté detrás de todo.

        // Centramos el mapa y ajustamos la cámara para que se vea bien.
        backgroundGO.transform.position = Vector3.zero;
        Camera.main.orthographicSize = archipelagoMapSprite.bounds.size.y / 2;
    }

    /// <summary>
    /// Crea los puntos de anclaje invisibles en las ubicaciones definidas.
    /// </summary>
    void CreateBeaconAnchors()
    {
        if (beaconAnchorPrefab == null)
        {
            Debug.LogError("¡No se ha asignado el Prefab 'Beacon Anchor' en el LevelHandler!");
            return;
        }

        for (int i = 0; i < beaconLocations.Length; i++)
        {
            // Instanciamos el anclaje en la posición definida y lo hacemos hijo del Grid.
            GameObject anchorGO = Instantiate(beaconAnchorPrefab, beaconLocations[i], Quaternion.identity, this.transform);
            anchorGO.name = $"Beacon Anchor {i}";

            // Le pasamos la referencia del prefab de la baliza al script del anclaje.
            BeaconAnchor anchorScript = anchorGO.GetComponent<BeaconAnchor>();
            if (anchorScript != null) {
                anchorScript.beaconPrefab = this.beaconPrefab;
            }
        }
    }

    /// <summary>
    /// Reproduce el sonido de rotación.
    /// </summary>
    public void PlayPipeRotationAudio()
    {
        if (pipeRotationAudio != null)
            pipeRotationAudio.Play();
    }

    /// <summary>
    /// Reproduce el sonido al colocar una baliza.
    /// </summary>
    public void PlayPlaceBeaconAudio()
    {
        if (placeBeaconAudio != null)
            placeBeaconAudio.Play();
    }

    /// <summary>
    /// Reproduce el sonido de victoria.
    /// </summary>
    public void PlayWinningAudio()
    {
        if (winningAudio != null)
            winningAudio.Play();
    }

    // --- Métodos que ya no se usan pero podrían ser adaptados en el futuro ---
    public void ResetLevel() { /* Lógica de reinicio para el nuevo juego */ }
    public void StoreGamePieces() { /* Lógica para almacenar las balizas */ }

#if UNITY_EDITOR
    // Dibuja las posiciones de los anclajes en el editor para facilitar su colocación.
    void OnDrawGizmos()
    {
        if (beaconLocations == null) return;

        for (int i = 0; i < beaconLocations.Length; i++)
        {
            // Dibuja una esfera verde semitransparente en la posición de cada anclaje.
            Gizmos.color = new Color(0, 1, 0, 0.5f); // Verde semitransparente
            Gizmos.DrawSphere(beaconLocations[i], 0.5f);

            // Dibuja una etiqueta para identificar cada anclaje.
            UnityEditor.Handles.Label(beaconLocations[i] + Vector2.up * 0.6f, $"Anchor {i}");
        }
    }
#endif
}
