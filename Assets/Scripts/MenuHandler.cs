using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles Main Menu GUI components construction and management
/// </summary>
public class MenuHandler : MonoBehaviour
{
    public TMP_Dropdown difficultyDropdown;

    private GameObject canvasGO;
    private List<AudioSource> audioSources = new List<AudioSource>();

    void Start()
    {
        audioSources.Add(GenerateAudioSource("Sounds/click1", "Audio Click Source"));
        audioSources.Add(GenerateAudioSource("Sounds/rollover1", "Audio Enter Source"));

        canvasGO = GenerateCanvasGO("Main Menu Canvas");

        GenerateBackgroundPanel();

        // Reorganizamos los botones
        // Centramos el botón principal y ocultamos los otros modos de juego
        GenerateMenuBtn("Jugar", 0, -150, () => SceneHandler.LoadLevelSelectScene(isFreeWorldMode: false));
        //GenerateMenuBtn("Arcade", 0, -150, SceneHandler.LoadArcadeGameScene);
        //GenerateMenuBtn("Free World", 350, -150, () => SceneHandler.LoadLevelSelectScene(isFreeWorldMode: true));

        // Ocultamos el selector de dificultad y la establecemos por defecto en "Easy"
        //ConfigureDropdownGO();
        LevelData.Difficulty = (Difficulty)0; // 0 = Easy

        GenerateTitleText();
    }

    /// <summary>
    /// Generate GUI Canvas where the Menu GUI components will be placed on
    /// </summary>
    public static GameObject GenerateCanvasGO(string canvasName)
    {
        GameObject tempCanvasGO = new GameObject();
        tempCanvasGO.name = canvasName;
        tempCanvasGO.layer = 5; // The layer is Unity's default GUI layer index = 5

        // Rendering options
        Canvas canvas = tempCanvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        CanvasScaler cs = tempCanvasGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        cs.matchWidthOrHeight = 0.5f;
        tempCanvasGO.AddComponent<GraphicRaycaster>();

        return tempCanvasGO;
    }

    /// <summary>
    /// Generate MainMenu button with a delegate function
    /// </summary>
    void GenerateMenuBtn(string txt, int posX, int posY, UnityAction onClickFunc)
    {
        GameObject buttonGO = new GameObject();
        buttonGO.transform.parent = canvasGO.transform;
        buttonGO.layer = canvasGO.layer;
        buttonGO.name = txt.Replace(" ", "");

        // Image
        Button buttonComp = buttonGO.AddComponent<Button>();
        buttonGO.AddComponent<CanvasRenderer>();
        Image buttonImg = buttonGO.AddComponent<Image>();
        buttonImg.sprite = Resources.Load<Sprite>("UI/tileBlue_01"); // Puedes cambiar "UI/tileBlue_01" por otro sprite que tengas
        buttonImg.color = new Color32(45, 52, 54, 255); // Un color gris oscuro
        buttonComp.targetGraphic = buttonImg;

        // Button Component position
        RectTransform transform = buttonComp.GetComponent<RectTransform>();
        transform.localPosition = new Vector3(posX, posY, 0); // Posición ajustada en Start()
        transform.sizeDelta = new Vector2(160, 30);
        transform.localScale = new Vector3(4.5f, 4.5f, 0);

        ConfigureButtonSounds(ref buttonComp, () =>
        {
            audioSources[0].Play();
            onClickFunc();
        }, audioSources[1].Play);

        // Button Text
        GameObject textGO = new GameObject();
        textGO.transform.parent = buttonGO.transform;
        textGO.layer = buttonGO.layer;
        textGO.name = "Text";

        TextMeshProUGUI textComp = textGO.AddComponent<TextMeshProUGUI>();
        textComp.text = txt;
        textComp.fontSize = 22; // Fuente un poco más grande
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.enableWordWrapping = false;
        textComp.color = Color.white; // Texto blanco para mejor contraste

        // Button Text Component relative position
        transform = textComp.GetComponent<RectTransform>();
        transform.localPosition = new Vector3(0, 0, 0);
        transform.sizeDelta = new Vector2(0, 0);
        transform.localScale = new Vector3(1, 1, 1);
    }

    void GenerateTitleText()
    {
        // Title Text
        GameObject titleGO = new GameObject();
        titleGO.transform.parent = canvasGO.transform;
        titleGO.layer = canvasGO.layer;
        titleGO.name = "Balizas Retro";

        TextMeshProUGUI textComp = titleGO.AddComponent<TextMeshProUGUI>();
        textComp.text = "Balizas Retro";
        textComp.font = (TMP_FontAsset)Resources.Load("UI/Electronic Highway Sign SDF");
        textComp.fontSize = 50; // Título más grande
        textComp.fontStyle = FontStyles.Bold;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.enableWordWrapping = false;
        textComp.color = new Color32(0, 255, 255, 255); // Color cian brillante

        // Title text position
        RectTransform transform = textComp.GetComponent<RectTransform>();
        transform.localPosition = new Vector3(0, 380, 0);
        transform.sizeDelta = new Vector2(200, 50);
        transform.localScale = new Vector3(2.5f, 2.5f, 0);
    }

    void GenerateBackgroundPanel()
    {
        GameObject bgGO = new GameObject("BackgroundPanel");
        bgGO.transform.SetParent(canvasGO.transform, false);
        bgGO.layer = canvasGO.layer;

        Image bgImage = bgGO.AddComponent<Image>();
        // Un color de fondo gris oscuro azulado
        bgImage.color = new Color32(23, 32, 42, 255);

        RectTransform rectTransform = bgGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = new Vector2(0, 0);
        rectTransform.offsetMax = new Vector2(0, 0);

        // Asegurarse de que el fondo esté detrás de todos los demás elementos de la UI
        rectTransform.SetAsFirstSibling();
    }

    /// <summary>
    /// Configure button sounds. All buttons share the same onClick and onMouseEnter sounds in all Scenes.
    /// </summary>
    /// <param name="btn">Button component reference</param>
    /// <param name="onClickFunc">onClick (mouse left-click) delegate</param>
    /// <param name="onMouseEnterFunc">onMouseEnter (mouse enters the corresponding button area) delegate</param>
    public static void ConfigureButtonSounds(ref Button btn, UnityAction onClickFunc, UnityAction onMouseEnterFunc)
    {
        // Listener
        btn.onClick.AddListener(onClickFunc);

        // Event Trigger - Mouse enter
        GameObject btnGO = btn.gameObject;
        addEventTrigger(ref btnGO, EventTriggerType.PointerEnter, onMouseEnterFunc);
    }

    private static void addEventTrigger(ref GameObject gameObject, EventTriggerType eventTriggerType, UnityAction callback) {
        EventTrigger trigger = gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry triggerEntry = new EventTrigger.Entry();
        triggerEntry.eventID = eventTriggerType;
        triggerEntry.callback.AddListener((_) => callback());
        trigger.triggers.Add(triggerEntry);
    }

    /// <summary>
    /// Generate AudioSource GO using AudioClip loaded from Resources folder
    /// </summary>
    /// <param name="audioPath">Relative Resources path to the AudioClip</param>
    /// <param name="GOName">Custom name of the AudioSource GO</param>
    public static AudioSource GenerateAudioSource(string audioPath, string GOName)
    {
        GameObject audioGO = new GameObject();
        audioGO.name = GOName;

        AudioSource audioSrc = audioGO.AddComponent<AudioSource>();
        audioSrc.clip = (AudioClip)Resources.Load(audioPath);

        return audioSrc;
    }

    void ConfigureDropdownGO()
    {
        var difficultyDropdownGO = difficultyDropdown.gameObject;

        difficultyDropdownGO.transform.SetParent(canvasGO.transform);
        difficultyDropdownGO.layer = canvasGO.layer;

        RectTransform transform = difficultyDropdownGO.GetComponent<RectTransform>();
        transform.localPosition = new Vector3(0, -350, 0); // Lo subimos un poco
        transform.sizeDelta = new Vector2(160, 30);
        transform.localScale = new Vector3(3.5f, 3.5f, 0); // Lo hacemos un poco más pequeño

        // Event Trigger - Mouse enter
        addEventTrigger(ref difficultyDropdownGO, EventTriggerType.PointerEnter, audioSources[1].Play);

        // Event Trigger - Mouse click
        addEventTrigger(ref difficultyDropdownGO, EventTriggerType.PointerClick, audioSources[0].Play);

        difficultyDropdown.onValueChanged.AddListener((int val) =>
        {
            audioSources[0].Play();
            LevelData.Difficulty = (Difficulty)val;
            Debug.Log("Difficulty: " + LevelData.Difficulty);
        });

        LevelData.Difficulty = (Difficulty)difficultyDropdown.value;
    }
}
