using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles Level Select menu GUI components, their construction and management including pagination buttons
/// and page counting
/// </summary>
public class LevelSelectHandler : MonoBehaviour
{
    public static int MaxTimeLimit { get; } = 20;

    public Button previousPageBtn;
    public Button nextPageBtn;
    public Sprite levelSelectBtnBG;
    public Sprite freeWorldBtnBG;
    public Sprite[] levelSelectNumbers;
    public Sprite[] freeWorldNumbers;

    private GameObject canvasGO;
    private List<AudioSource> audioSources = new List<AudioSource>();

    [SerializeField]
    private List<GameObject> levelPagesGO = new List<GameObject>();

    [SerializeField]
    private int currentPage;
    private Button previousBtn;
    private Button nextBtn;

    void Start()
    {
        audioSources.Add(MenuHandler.GenerateAudioSource("Sounds/click1", "Audio Click Source"));
        audioSources.Add(MenuHandler.GenerateAudioSource("Sounds/rollover1", "Audio Enter Source"));

        canvasGO = MenuHandler.GenerateCanvasGO("Level Select Canvas");

        GenerateBackgroundPanel();

        GenerateTitleText();

        // Generamos únicamente el Nivel 1 y ocultamos el resto
        int pageID = 0;
        GenerateLevelsPage(pageID);

        // Solo creamos el botón para el nivel 1
        GenerateLevelButton(1, pageID);

        ConfigureMainMenuButton();
        // Ya no necesitamos los botones de paginación
        //ConfigurePrevNextButtons();

        levelPagesGO[0].SetActive(true);
        currentPage = 0;
    }

    /// <summary>
    /// Used to generate and configure title in LevelSelect scene
    /// </summary>
    void GenerateTitleText()
    {
        // Title Text
        GameObject titleGO = new GameObject();
        titleGO.transform.parent = canvasGO.transform;
        titleGO.layer = canvasGO.layer;
        titleGO.name = LevelData.IsFreeWorldMode ? "Free World" : "Level Select";

        TextMeshProUGUI textComp = titleGO.AddComponent<TextMeshProUGUI>();
        textComp.text = LevelData.IsFreeWorldMode ? "FREE WORLD" : "LEVEL SELECT";
        textComp.font = (TMP_FontAsset)Resources.Load("UI/Electronic Highway Sign SDF");
        textComp.fontSize = 100;
        textComp.fontStyle = FontStyles.Bold;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.color = new Color32(0, 255, 255, 255); // Color cian brillante

        // Title text position
        RectTransform transform = textComp.GetComponent<RectTransform>();
        transform.anchorMin = new Vector2(0, 0);
        transform.anchorMax = new Vector2(1, 1);
        transform.SetLeft(487);
        transform.SetTop(42);
        transform.SetRight(487);
        transform.SetBottom(842);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
        transform.localScale = new Vector3(1, 1, 1);
    }

    void GenerateBackgroundPanel()
    {
        GameObject bgGO = new GameObject("BackgroundPanel");
        bgGO.transform.SetParent(canvasGO.transform, false);
        bgGO.layer = canvasGO.layer;

        Image bgImage = bgGO.AddComponent<Image>();
        // Un color de fondo gris oscuro azulado, igual que en el menú principal
        bgImage.color = new Color32(23, 32, 42, 255);

        RectTransform rectTransform = bgGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = new Vector2(0, 0);
        rectTransform.offsetMax = new Vector2(0, 0);

        // Asegurarse de que el fondo esté detrás de todos los demás elementos de la UI
        rectTransform.SetAsFirstSibling();
    }

    void GenerateLevelsPage(int pageID)
    {
        // Grid GO
        GameObject gridGO = new GameObject();
        gridGO.transform.parent = canvasGO.transform;
        gridGO.layer = canvasGO.layer;
        gridGO.name = "Page " + pageID;

        var grid = gridGO.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(200, 200);
        grid.spacing = new Vector2(150, 150);
        grid.childAlignment = TextAnchor.MiddleCenter;

        RectTransform transform = grid.GetComponent<RectTransform>();
        transform.anchorMin = new Vector2(0, 0);
        transform.anchorMax = new Vector2(1, 1);
        transform.SetLeft(315);
        transform.SetTop(289);
        transform.SetRight(315);
        transform.SetBottom(59);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
        transform.localScale = new Vector3(1, 1, 1);

        gridGO.SetActive(false);
        levelPagesGO.Add(gridGO);
    }

    void GenerateLevelButton(int levelNumber, int pageID)
    {
        GameObject buttonGO = new GameObject();
        buttonGO.transform.parent = levelPagesGO[pageID].transform;
        buttonGO.layer = levelPagesGO[pageID].layer;
        buttonGO.name = levelNumber.ToString();

        // Image
        Image buttonImg = buttonGO.AddComponent<Image>();
        buttonImg.sprite = LevelData.IsFreeWorldMode ? freeWorldBtnBG : levelSelectBtnBG;
        buttonImg.color = new Color32(45, 52, 54, 255); // Color gris oscuro

        // Button
        Button buttonComp = buttonGO.AddComponent<Button>();
        buttonComp.targetGraphic = buttonImg;

        // Añadir interactividad de color
        ColorBlock cb = buttonComp.colors;
        cb.normalColor = new Color32(45, 52, 54, 255);
        cb.highlightedColor = new Color32(99, 110, 114, 255);
        cb.pressedColor = new Color32(0, 184, 148, 255);
        cb.selectedColor = cb.normalColor;
        buttonComp.colors = cb;

        // Button Component position
        RectTransform transform = buttonComp.GetComponent<RectTransform>();
        transform.localScale = new Vector3(1, 1, 1);

        MenuHandler.ConfigureButtonSounds(ref buttonComp, audioSources[0].Play, audioSources[1].Play);
        buttonComp.onClick.AddListener(() => SceneHandler.LoadLevel(levelNumber));

        // Horizontal Layout Group (for multiple number sprites)
        GameObject horizontalGO = new GameObject();
        horizontalGO.transform.parent = buttonGO.transform;
        horizontalGO.layer = buttonGO.layer;
        horizontalGO.name = "Number" + levelNumber;

        HorizontalLayoutGroup horizontalComp = horizontalGO.AddComponent<HorizontalLayoutGroup>();
        horizontalComp.childAlignment = TextAnchor.MiddleCenter;
        horizontalComp.childControlHeight = true;
        horizontalComp.childControlWidth = true;
        horizontalComp.childForceExpandHeight = true;
        horizontalComp.childForceExpandHeight = true;

        // Horizontal Component relative position
        transform = horizontalComp.GetComponent<RectTransform>();
        transform.localPosition = new Vector3(0, 0, 0);
        transform.sizeDelta = new Vector2(159, 159);
        transform.localScale = new Vector3(1, 1, 1);

        // Iterate through number literals to generate individual Number sprites
        // Numbers are organized in Horizontal manner to simulate multi-digits
        foreach (char literal in levelNumber.ToString())
        {
            // Button Image Number
            GameObject imageNumberGO = new GameObject();
            imageNumberGO.transform.parent = horizontalComp.transform;
            imageNumberGO.layer = horizontalGO.layer;
            imageNumberGO.name = literal.ToString();

            Image numberImg = imageNumberGO.AddComponent<Image>();
            numberImg.sprite = LevelData.IsFreeWorldMode ? 
                freeWorldNumbers[literal - '0'] : levelSelectNumbers[literal - '0'];
            numberImg.color = Color.white;

            // Button Text Component relative position
            transform = numberImg.GetComponent<RectTransform>();
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    /// <summary>
    /// Generate Previous and Next buttons to be used in Pagination
    /// </summary>
    void ConfigurePrevNextButtons()
    {
        previousBtn = Instantiate(previousPageBtn, canvasGO.transform);
        nextBtn = Instantiate(nextPageBtn, canvasGO.transform);

        Image prevImg = previousBtn.GetComponent<Image>();
        prevImg.sprite = LevelData.IsFreeWorldMode ? freeWorldBtnBG : levelSelectBtnBG;
        prevImg.color = new Color32(45, 52, 54, 255);

        Image nextImg = nextBtn.GetComponent<Image>();
        nextImg.sprite = LevelData.IsFreeWorldMode ? freeWorldBtnBG : levelSelectBtnBG;
        nextImg.color = new Color32(45, 52, 54, 255);

        // Añadir interactividad de color a los botones de paginación
        ColorBlock cb = previousBtn.colors;
        cb.normalColor = new Color32(45, 52, 54, 255);
        cb.highlightedColor = new Color32(99, 110, 114, 255);
        cb.pressedColor = new Color32(0, 184, 148, 255);
        cb.selectedColor = cb.normalColor;

        previousBtn.colors = cb;
        nextBtn.colors = cb;


        SetButtonTransform(previousBtn, 115, 65);
        SetButtonTransform(nextBtn, -115, 65);

        // If there's only 1 Page, no button should be interactable
        if (levelPagesGO.Count != 1)
            nextBtn.interactable = true;

        // Add Button sounds
        MenuHandler.ConfigureButtonSounds(ref previousBtn, audioSources[0].Play, audioSources[1].Play);
        MenuHandler.ConfigureButtonSounds(ref nextBtn, audioSources[0].Play, audioSources[1].Play);

        // Add Page handling delegates
        previousBtn.onClick.AddListener(() => { 
            PreviousPage();
            HandleFirstLastPage();
        });
        nextBtn.onClick.AddListener(() => { 
            NextPage();
            HandleFirstLastPage();
        });
    }

    void SetButtonTransform(Button btn, float posX, float posY)
    {
        RectTransform transform = btn.GetComponent<RectTransform>();
        transform.anchoredPosition = new Vector3(posX, posY, 0);
        transform.localScale = new Vector3(1, 1, 1);
    }

    /// <summary>
    /// Previous Page button method used in delegate for pagination
    /// </summary>
    void PreviousPage()
    {
        levelPagesGO[currentPage].SetActive(false);
        currentPage--;
        levelPagesGO[currentPage].SetActive(true);
    }

    /// <summary>
    /// Next Page button method used in delegate for pagination
    /// </summary>
    void NextPage()
    {
        levelPagesGO[currentPage].SetActive(false);
        currentPage++;
        levelPagesGO[currentPage].SetActive(true);
    }

    /// <summary>
    /// If the currentPage is First Page or Last Page, make the respective buttons interactable/not-interactable
    /// </summary>
    void HandleFirstLastPage()
    {
        previousBtn.interactable = true;
        nextBtn.interactable = true;
        if (currentPage == 0)
        {
            previousBtn.interactable = false;
        }
        if (currentPage == levelPagesGO.Count - 1)
        {
            nextBtn.interactable = false;
        }
    }

    void ConfigureMainMenuButton()
    {
        GameObject buttonGO = new GameObject("MainMenuButton");
        buttonGO.transform.SetParent(canvasGO.transform, false);
        buttonGO.layer = canvasGO.layer;

        // Componente Image para el fondo del botón
        Image buttonImg = buttonGO.AddComponent<Image>();
        // No le asignamos ningún sprite para que sea un color sólido
        buttonImg.color = new Color32(45, 52, 54, 255);

        // Componente Button para la funcionalidad
        Button buttonComp = buttonGO.AddComponent<Button>();
        buttonComp.targetGraphic = buttonImg;

        // Añadir interactividad de color
        ColorBlock cb = buttonComp.colors;
        cb.normalColor = new Color32(45, 52, 54, 255);
        cb.highlightedColor = new Color32(99, 110, 114, 255);
        cb.pressedColor = new Color32(0, 184, 148, 255);
        cb.selectedColor = cb.normalColor;
        buttonComp.colors = cb;

        // Posición y tamaño del botón
        RectTransform transform = buttonGO.GetComponent<RectTransform>();
        transform.anchoredPosition = new Vector2(0, -450); // Centrado y en la parte inferior
        transform.sizeDelta = new Vector2(500, 100);
        transform.localScale = Vector3.one;

        // Texto del botón
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        textGO.layer = buttonGO.layer;

        TextMeshProUGUI textComp = textGO.AddComponent<TextMeshProUGUI>();
        textComp.text = "Menú Principal";
        textComp.fontSize = 40;
        textComp.fontStyle = FontStyles.Bold;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.color = Color.white;

        // Configurar sonidos y acción de clic
        MenuHandler.ConfigureButtonSounds(ref buttonComp, () =>
        {
            audioSources[0].Play();
            SceneHandler.LoadMainMenuScene();
        }, audioSources[1].Play);
    }
}
