using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles Pause mechanic in the game as well as Pause menu and buttons
/// </summary>
public class PauseControl : MonoBehaviour
{
    public GameObject pauseMenu;
    public Button pauseBtn;

    public GameObject endGameMenu;

    GUIHandler guiHandler;

    public static bool GameIsPaused { get; set; }

    // Fired at the very start of script loading
    void Awake()
    {
        pauseBtn.onClick.AddListener(PauseGame);
    }

    void Start()
    {
        guiHandler = GetComponent<GUIHandler>();
        ApplyPauseMenuStyle();
    }

    void Update()
    {
        #pragma warning disable CS0618 // Type or member is obsolete
        // Determine if the endGameMenu is visible, if not then the game can be paused
        if (!endGameMenu.active)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }
        }
        #pragma warning restore CS0618 // Type or member is obsolete
    }

    /// <summary>
    /// Default pausing function that freezes all interactions, sets/unsets GameIsPaused flag
    /// and makes the PauseMenu visible
    /// </summary>
    public void PauseGame()
    {
        GameIsPaused = !GameIsPaused;

        if (GameIsPaused)
        {
            Time.timeScale = 0f;
            pauseBtn.enabled = false;
            pauseMenu.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            pauseBtn.enabled = true;
            pauseMenu.SetActive(false);
        }
    }

    void OnDestroy()
    {
        pauseBtn.onClick.RemoveListener(PauseGame);
    }

    /// <summary>
    /// Aplica el estilo visual al menú de pausa y sus botones.
    /// </summary>
    void ApplyPauseMenuStyle()
    {
        // Desactivamos todas las imágenes existentes en el menú de pausa para empezar de cero.
        Image[] existingImages = pauseMenu.GetComponentsInChildren<Image>(true);
        foreach (Image img in existingImages)
        {
            img.enabled = false;
        }

        // Ahora, añadimos y configuramos nuestro propio fondo de color sólido.
        Image background = pauseMenu.GetComponent<Image>() ?? pauseMenu.AddComponent<Image>();
        background.enabled = true;
        background.color = new Color32(23, 32, 42, 180); // Un fondo oscuro y translúcido

        // Creamos el texto "Juego en pausa"
        CreatePauseTitle();

        // Creamos el botón para continuar
        CreateResumeButton();
    }

    /// <summary>
    /// Crea el texto principal del menú de pausa.
    /// </summary>
    void CreatePauseTitle()
    {
        GameObject textGO = new GameObject("PauseTitle");
        textGO.transform.SetParent(pauseMenu.transform, false);
        textGO.layer = pauseMenu.layer;

        TMPro.TextMeshProUGUI textComp = textGO.AddComponent<TMPro.TextMeshProUGUI>();
        textComp.text = "Juego en pausa";
        textComp.fontSize = 80;
        textComp.fontStyle = TMPro.FontStyles.Bold;
        textComp.alignment = TMPro.TextAlignmentOptions.Center;
        textComp.color = new Color32(0, 255, 255, 255); // Color cian brillante

        // Centramos el texto en el menú
        RectTransform rectTransform = textComp.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
    }

    /// <summary>
    /// Crea el botón para reanudar el juego desde el menú de pausa.
    /// </summary>
    void CreateResumeButton()
    {
        GameObject buttonGO = new GameObject("ResumeButton");
        buttonGO.transform.SetParent(pauseMenu.transform, false);
        buttonGO.layer = pauseMenu.layer;

        // Componente Image para el fondo del botón
        Image buttonImg = buttonGO.AddComponent<Image>();
        buttonImg.sprite = null; // Color sólido

        // Componente Button para la funcionalidad
        Button buttonComp = buttonGO.AddComponent<Button>();
        buttonComp.targetGraphic = buttonImg;
        buttonComp.onClick.AddListener(PauseGame); // Llama a la misma función para reanudar

        // Estilo de colores
        ColorBlock cb = buttonComp.colors;
        cb.normalColor = new Color32(45, 52, 54, 255);
        cb.highlightedColor = new Color32(99, 110, 114, 255);
        cb.pressedColor = new Color32(0, 184, 148, 255);
        buttonComp.colors = cb;

        // Posición y tamaño
        RectTransform transform = buttonGO.GetComponent<RectTransform>();
        transform.anchoredPosition = new Vector2(0, -150); // Debajo del título
        transform.sizeDelta = new Vector2(400, 80);

        // Texto del botón
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);

        TMPro.TextMeshProUGUI textComp = textGO.AddComponent<TMPro.TextMeshProUGUI>();
        textComp.text = "Continuar";
        textComp.fontSize = 40;
        textComp.fontStyle = TMPro.FontStyles.Bold;
        textComp.alignment = TMPro.TextAlignmentOptions.Center;
        textComp.color = Color.white;
    }
}
