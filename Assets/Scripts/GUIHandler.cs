using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles Game scene GUI Components, End Game Menu, Total Score calculaction 
/// and Timer and its mechanism
/// </summary>
public class GUIHandler : MonoBehaviour
{
    /// <summary>
    /// Used for Total Score calculation, can be changed for custom calculations
    /// </summary>
    public const int MAXIMUM_SCORE = 10000;

    public GameObject endGameMenu;
    public GameObject endGameText;
    public TMP_Text totalScore;
    public LevelHandler levelHandler;

    public Button restartButton;
    public Button quitButton;
    public Button nextLevelButton;
    public Button pauseButton;
    public Button skipButton; // Este podría eliminarse en el futuro.
    public Button validatePathButton;

    public TMP_Text timerText;

    // After the Flow starts EndGame starts
    public static bool IsEndGame { get; set; } = false;

    public bool isDebug;
    [SerializeField]
    [Range(5, 30)]
    int defaultTimeLimit = 20; // To be edited in Editor
    int currentTime;

    GameManager gm;

    void Awake()
    {
        gm = GetComponent<GameManager>();
        levelHandler = FindObjectOfType<LevelHandler>(); // Añadimos esta línea para encontrar el LevelHandler.
        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(GetBackToMainMenu);
        nextLevelButton.onClick.AddListener(GoToNextLevel);
        // La funcionalidad de skip y start flow se reemplaza por la validación de ruta.
        validatePathButton.onClick.AddListener(gm.ValidateBeaconPath);
    }

    void Start()
    {
        if (isDebug)
            LevelData.TimeLimit = defaultTimeLimit;

        bool isLastLevel = LevelData.LevelNumber == 
            (LevelData.IsFreeWorldMode ? SceneHandler.FreeWorldLevelCount : SceneHandler.LevelSelectLevelCount);
        if (LevelData.IsArcadeMode || isLastLevel)
            nextLevelButton.gameObject.SetActive(false);

        // Aplicamos el estilo a los elementos de la UI
        ApplyGUIStyle();

        timerText.text = LevelData.TimeLimit.ToString();
        StartCoroutine("CountdownTimer");
    }

    /// <summary>
    /// Aplica el estilo visual a los botones y textos de la UI del juego.
    /// </summary>
    void ApplyGUIStyle()
    {
        // Estilo para los botones principales
        StyleButton(restartButton);
        StyleButton(quitButton);
        StyleButton(nextLevelButton);
        StyleButton(pauseButton);
        StyleButton(validatePathButton);
        AddTextToButton(validatePathButton, "Probar Ruta", 20);
        AddTextToButton(pauseButton, "Pausar", 20);

        // Color del texto del temporizador
        timerText.color = new Color32(0, 255, 255, 255);

        // Estilo para el panel del temporizador
        // Asumimos que el panel es el padre del objeto de texto del temporizador
        Image timerPanelImage = timerText.transform.parent.GetComponent<Image>();
        if (timerPanelImage != null)
        {
            timerPanelImage.sprite = null; // Quitamos la imagen para usar un color sólido
            timerPanelImage.color = new Color32(45, 52, 54, 255); // Asignamos el color gris oscuro
        }

        // Color del texto del menú de fin de juego
        endGameText.GetComponent<TMP_Text>().color = new Color32(0, 255, 255, 255);

        // Estilo para el panel del menú de fin de juego
        // Primero, desactivamos todas las imágenes existentes para eliminar marcos antiguos.
        Image[] existingImages = endGameMenu.GetComponentsInChildren<Image>(true);
        foreach (Image img in existingImages)
        {
            img.enabled = false;
        }

        // Ahora, creamos nuestro propio fondo de color sólido.
        Image background = endGameMenu.GetComponent<Image>() ?? endGameMenu.AddComponent<Image>();
        background.enabled = true;
        background.sprite = null;
        background.color = new Color32(23, 32, 42, 220); // Fondo oscuro y semi-translúcido

        // Re-estilizamos los botones del menú final para asegurarnos de que sean visibles
        StyleButton(restartButton);
        StyleButton(quitButton);
        StyleButton(nextLevelButton);


        // Estilo para el texto del puntaje
        if (totalScore != null)
        {
            totalScore.color = Color.white;
        }

        // Aseguramos que el texto de los botones del menú final sea visible
        SetButtonText(restartButton, "Reiniciar");
        SetButtonText(quitButton, "Volver al Menú");
        SetButtonText(nextLevelButton, "Siguiente Nivel");

        // El botón de skip empieza oculto, pero lo estilizamos igualmente
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Coroutine that starts counting down the Timer every second until it reaches 0 
    /// after which it's Game Over
    /// </summary>
    IEnumerator CountdownTimer()
    {
        currentTime = LevelData.TimeLimit;
        while (currentTime != 0)
        {
            yield return new WaitForSeconds(1);
            currentTime -= 1;
            timerText.text = currentTime.ToString();
        }
        ShowEndGameMenu(isWon: false);
        StopCoroutine("CountdownTimer");
    }

    void OnDestroy()
    {
        restartButton.onClick.RemoveListener(RestartGame);
        quitButton.onClick.RemoveListener(GetBackToMainMenu);
        validatePathButton.onClick.RemoveListener(gm.ValidateBeaconPath);
    }

    /// <summary>
    /// A popup GUI that shows the End Game menu with the Total Score, Restart, Quit and Next Level buttons
    /// </summary>
    /// <param name="isWon">Used to determine if the player won or lost the game. 
    /// The End Game Menu changes accordingly.</param>
    public void ShowEndGameMenu(bool isWon)
    {
        // Nos aseguramos de que el tiempo vuelva a la normalidad antes de hacer nada.
        Time.timeScale = 1f;
        gm.StopAllCoroutines();

        if (isWon)
        {
            endGameText.name = "You Won";
            endGameText.GetComponent<TMP_Text>().text = "¡GANASTE!";
            totalScore.text = CalculateTotalScore();
            // Primero reproducimos el sonido, mientras el tiempo aún corre.
            levelHandler.PlayWinningAudio();
        }
        else
        {
            endGameText.name = "You Lost";
            endGameText.GetComponent<TMP_Text>().text = "¡PERDISTE!";
            totalScore.text = "0"; // If the player looses the remaining Timer is unnecessary
        }

        pauseButton.enabled = false;
        // Pauses the game to prevent GUI interaction and player Input
        Time.timeScale = 0f;
        PauseControl.GameIsPaused = true;

        endGameMenu.SetActive(true);
    }

    /// <summary>
    /// Resumes the Game, restores defaults, restarts and shuffles the current level 
    /// </summary>
    public void RestartGame()
    {
        // La forma más robusta de reiniciar es simplemente recargar la escena actual.
        // Esto asegura que todas las balizas, conexiones y estados se restablezcan a su valor inicial.
        Time.timeScale = 1; // Nos aseguramos de que el tiempo vuelva a la normalidad.
        PauseControl.GameIsPaused = false;
        IsEndGame = false;
        
        SceneHandler.LoadLevel(LevelData.LevelNumber);
    }

    /// <summary>
    /// Resumes the Game, restores defaults and changes scene to MainMenu
    /// </summary>
    void GetBackToMainMenu()
    {
        if (PauseControl.GameIsPaused)
        {
            PauseControl.GameIsPaused = false;
            Time.timeScale = 1;
        }
        endGameMenu.SetActive(false);

        SceneHandler.LoadMainMenuScene();
    }

    /// <summary>
    /// Navigation to the next level
    /// </summary>
    void GoToNextLevel()
    {
        if (PauseControl.GameIsPaused)
        {
            PauseControl.GameIsPaused = false;
            Time.timeScale = 1;
        }
        endGameMenu.SetActive(false);

        SceneHandler.LoadLevel(LevelData.LevelNumber + 1);
    }

    /// <summary>
    /// Setup the EndGame routine and sets IsEndGame
    /// </summary>
    public void SetEndGameScene()
    {
        IsEndGame = true;
        // StopCoroutine("CountdownTimer"); // El temporizador podría seguir siendo útil.
        validatePathButton.enabled = false;
    }

    /// <summary>
    /// Basic Total Score calculation using set maximum metric
    /// </summary>
    /// <returns>Total Score string</returns>
    string CalculateTotalScore()
    {
        // Minimum Score: 0
        // Maximum Score: 10000
        double weight = LevelData.IsArcadeMode ? 1.0 : LevelSelectHandler.MaxTimeLimit / (double)LevelData.TimeLimit;
        double notNormalizedScore = currentTime / (double)LevelData.TimeLimit / weight;
        int score = Mathf.RoundToInt((float)(notNormalizedScore * MAXIMUM_SCORE));
        return score.ToString();
    }

    /// <summary>
    /// Used for flow acceleration using the "Skip" button
    /// </summary>
    void AccelerateFlow()
    {
        Time.timeScale = 4;
    }

    /// <summary>
    /// Aplica un estilo visual consistente a un botón de la UI.
    /// </summary>
    /// <param name="btn">El botón a estilizar.</param>
    void StyleButton(Button btn)
    {
        if (btn == null) return;

        Image btnImage = btn.GetComponent<Image>();
        if (btnImage != null)
        {
            // Nos aseguramos de que la imagen del botón esté activa
            btnImage.enabled = true;

            // Quitamos la imagen de fondo para usar un color sólido
            btnImage.sprite = null;

            // Color base del botón
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color32(45, 52, 54, 255);
            cb.highlightedColor = new Color32(99, 110, 114, 255);
            cb.pressedColor = new Color32(0, 184, 148, 255);
            cb.selectedColor = cb.normalColor;
            btn.colors = cb;
        }
    }

    /// <summary>
    /// Añade un objeto de texto a un botón.
    /// </summary>
    void AddTextToButton(Button btn, string text, float fontSize)
    {
        if (btn == null) return;

        // Evita duplicar el texto si ya existe
        if (btn.GetComponentInChildren<TextMeshProUGUI>() != null) return;

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btn.transform, false);
        textGO.layer = btn.gameObject.layer;

        TextMeshProUGUI textComp = textGO.AddComponent<TextMeshProUGUI>();
        textComp.text = text;
        textComp.fontSize = fontSize;
        textComp.fontStyle = FontStyles.Bold;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.color = Color.white;

        RectTransform rectTransform = textGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
    }

    /// <summary>
    /// Asigna texto a un botón, creándolo si no existe, y se asegura de que sea visible.
    /// </summary>
    void SetButtonText(Button btn, string text)
    {
        if (btn == null) return;

        // Busca si ya existe un texto, si no, lo crea.
        TextMeshProUGUI textComp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (textComp == null)
        {
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(btn.transform, false);
            textGO.layer = btn.gameObject.layer;
            textComp = textGO.AddComponent<TextMeshProUGUI>();

            // Estilo por defecto para el nuevo texto
            textComp.fontSize = 30;
            textComp.fontStyle = FontStyles.Bold;
            textComp.alignment = TextAlignmentOptions.Center;

            // Estirar el RectTransform para que ocupe el botón
            RectTransform rectTransform = textGO.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
        }
        textComp.text = text;
        textComp.color = Color.white;
    }
}
