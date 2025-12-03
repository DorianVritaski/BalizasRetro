using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player Input interaction with the game. Game Controls.
/// Pointer movement: Arrows, WASD.
/// Rotate Active Pipe: R.
/// Start Flow: F.
/// </summary>
public class Player : MonoBehaviour
{
    private GameManager gm;
    private LevelHandler levelHandler;

    void Start()
    {
        gm = gameObject.GetComponent<GameManager>();
        levelHandler = GameObject.FindObjectOfType<LevelHandler>();
    }

    void Update()
    {
        if (!PauseControl.GameIsPaused && !GUIHandler.IsEndGame)
        {
            // La lógica de control del jugador se reescribirá aquí para el nuevo sistema de balizas.
            // Por ahora, lo dejamos vacío para evitar errores de compilación.
        }
    }
}
