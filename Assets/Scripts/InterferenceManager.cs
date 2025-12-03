using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gestiona el evento de Interferencia Magnética que desestabiliza balizas aleatoriamente.
/// </summary>
public class InterferenceManager : MonoBehaviour
{
    [Header("Configuración de Interferencia")]
    [Tooltip("El tiempo mínimo en segundos entre eventos de interferencia.")]
    [SerializeField] private float minTimeBetweenEvents = 15f;
    [Tooltip("El tiempo máximo en segundos entre eventos de interferencia.")]
    [SerializeField] private float maxTimeBetweenEvents = 25f;

    void Start()
    {
        // Inicia el ciclo de interferencias.
        StartCoroutine(InterferenceCoroutine());
    }

    private IEnumerator InterferenceCoroutine()
    {
        // Bucle infinito que se ejecuta durante todo el juego.
        while (true)
        {
            // Espera un tiempo aleatorio antes de la siguiente interferencia.
            float waitTime = Random.Range(minTimeBetweenEvents, maxTimeBetweenEvents);
            yield return new WaitForSeconds(waitTime);

            // Busca todas las balizas que ya han sido colocadas.
            BeaconAnchor[] placedAnchors = FindObjectsOfType<BeaconAnchor>()
                                           .Where(a => a.isOccupied && a.isStable)
                                           .ToArray();

            if (placedAnchors.Length > 0)
            {
                // Selecciona una baliza al azar para desestabilizarla.
                int randomIndex = Random.Range(0, placedAnchors.Length);
                BeaconAnchor anchorToDisrupt = placedAnchors[randomIndex];

                Debug.Log($"¡Interferencia Magnética! Desestabilizando {anchorToDisrupt.name}");
                anchorToDisrupt.SetStability(false);
            }
        }
    }
}
