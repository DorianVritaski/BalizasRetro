using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona el mini-juego de estabilización de frecuencia con un slider.
/// </summary>
public class StabilizationMinigame : MonoBehaviour
{
    public static StabilizationMinigame Instance { get; private set; }

    [Header("UI Components")]
    public GameObject minigamePanel;
    public Slider frequencySlider;
    public Button confirmButton;
    public Image targetZoneImage; // La imagen que mostrará la zona correcta.

    [Header("Game Logic")]
    [Tooltip("Qué tan cerca debe estar el jugador del valor objetivo (ej: 0.1 = 10% de la barra).")]
    [SerializeField] private float tolerance = 0.1f;
    [Tooltip("La velocidad a la que se mueve el control del slider.")]
    [SerializeField] private float sliderSpeed = 1.5f;

    private BeaconAnchor currentAnchor;
    private float targetFrequency;
    private bool isMinigameActive = false;
    private int sliderDirection = 1;

    void Awake()
    {
        // Configuración del Singleton
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }

        confirmButton.onClick.AddListener(LockFrequency);
        minigamePanel.SetActive(false); // El panel empieza oculto.
    }

    void Update()
    {
        if (!isMinigameActive) return;

        MoveSlider();
    }

    /// <summary>
    /// Inicia el mini-juego para un anclaje específico.
    /// </summary>
    public void StartMinigame(BeaconAnchor anchor)
    {
        currentAnchor = anchor;
        // Genera una frecuencia objetivo aleatoria para que no sea siempre en el mismo sitio.
        targetFrequency = Random.Range(0.1f, 0.9f); // Evitamos los bordes.
        Debug.Log($"Minijuego iniciado para {anchor.name}. Frecuencia objetivo: {targetFrequency}");

        // El slider empieza en el borde izquierdo.
        frequencySlider.value = 0;
        sliderDirection = 1;

        // Actualizamos la pista visual para mostrar la nueva zona objetivo.
        UpdateTargetZoneVisual();
        // Mostramos el panel y pausamos el juego para que el jugador se concentre.
        minigamePanel.SetActive(true);
        Time.timeScale = 0f;
        isMinigameActive = true; // ¡Esta es la línea que faltaba!
        PauseControl.GameIsPaused = true;
    }

    /// <summary>
    /// Se llama al pulsar el botón "Confirmar". Detiene el slider y comprueba la frecuencia.
    /// </summary>
    private void LockFrequency()
    {
        if (!isMinigameActive) return;

        isMinigameActive = false; // Detenemos el movimiento.
        CheckResult();
    }

    private void CheckResult()
    {
        float playerFrequency = frequencySlider.value;
        Debug.Log($"Frecuencia del jugador: {playerFrequency}");

        // Comprueba si el valor del jugador está dentro del rango de tolerancia.
        if (Mathf.Abs(playerFrequency - targetFrequency) <= tolerance)
        {
            Debug.Log("¡Frecuencia estabilizada con éxito!");
            currentAnchor.SetStability(true);
        }
        else
        {
            Debug.Log("Fallo al estabilizar. Inténtalo de nuevo.");
            // Opcional: Añadir una penalización, como un sonido de error.
        }

        // Cierra el mini-juego independientemente del resultado.
        CloseMinigame();
    }

    private void CloseMinigame()
    {
        minigamePanel.SetActive(false);
        isMinigameActive = false;
        Time.timeScale = 1f;
        PauseControl.GameIsPaused = false;
        currentAnchor = null;
    }

    /// <summary>
    /// Ajusta la posición y el tamaño de la imagen de la zona objetivo.
    /// </summary>
    private void UpdateTargetZoneVisual()
    {
        if (targetZoneImage == null) return;

        RectTransform rect = targetZoneImage.GetComponent<RectTransform>();

        // Usamos los anchors para posicionar la zona como un porcentaje del slider.
        // El anchor izquierdo se posiciona en el inicio de la zona de tolerancia.
        rect.anchorMin = new Vector2(targetFrequency - tolerance, 0);
        // El anchor derecho se posiciona al final de la zona de tolerancia.
        rect.anchorMax = new Vector2(targetFrequency + tolerance, 1);
    }

    /// <summary>
    /// Mueve el control del slider de un lado a otro.
    /// </summary>
    private void MoveSlider()
    {
        frequencySlider.value += sliderSpeed * sliderDirection * Time.unscaledDeltaTime;

        if (frequencySlider.value >= 1f || frequencySlider.value <= 0f)
        {
            // Si llega a un extremo, cambia de dirección.
            sliderDirection *= -1;
        }
    }
}
