using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CellTraits
{
    public float size;
    public Color color;
    public float fitness; // Puntaje de rendimiento

    public CellTraits(float s, Color c, float f)
    {
        size = s;
        color = c;
        fitness = f;
    }
}

public class AdaptiveMLSystem : MonoBehaviour
{
    // Memoria donde guardamos las mejores células que han sobrevivido
    private List<CellTraits> survivorsHistory = new List<CellTraits>();

    // Parámetros de evolución
    private float explorationRate = 0.8f; // Empieza explorando mucho y luego baja
    private const int MAX_MEMORY = 15;

    // Registra cuando una célula sobrevive con éxito a la ronda completa
    public void RecordSurvivor(float size, Color color)
    {
        // Agregamos el rasgo exitoso con recompensa
        survivorsHistory.Add(new CellTraits(size, color, 10f));

        // Reducimos progresivamente la aleatoriedad para que el modelo "concentre" su aprendizaje
        explorationRate = Mathf.Max(0.05f, explorationRate - 0.03f);

        // Limitar el tamaño de la memoria para mantener solo los sobrevivientes más recientes
        if (survivorsHistory.Count > MAX_MEMORY)
        {
            survivorsHistory.RemoveAt(0);
        }
    }

    // Predice la siguiente combinación basada en la experiencia acumulada
    public CellTraits PredictBestTraits()
    {
        // Si no hay sobrevivientes o toca explorar (con probabilidad decreciente)
        if (survivorsHistory.Count == 0 || Random.value < explorationRate)
        {
            // Genera rasgos aleatorios para probar
            float randomSize = Random.Range(0.8f, 2.0f);
            Color randomColor = new Color(Random.value, Random.value, Random.value, 1f);
            return new CellTraits(randomSize, randomColor, 0f);
        }

        // EXPLOTACIÓN: Elegir a uno de los sobrevivientes exitosos del historial
        CellTraits parent = survivorsHistory[Random.Range(0, survivorsHistory.Count)];

        // MUTACIÓN CONTROLADA: Hacer la célula un poco más pequeña y variar LEVEMENTE el color
        float mutatedSize = Mathf.Clamp(parent.size - Random.Range(0.02f, 0.08f), 0.4f, 2.2f);

        // La variación de color ahora es súper sutil (+/- 4%) para afinar el camuflaje
        float colorDelta = 0.04f;
        Color mutatedColor = new Color(
            Mathf.Clamp01(parent.color.r + Random.Range(-colorDelta, colorDelta)),
            Mathf.Clamp01(parent.color.g + Random.Range(-colorDelta, colorDelta)),
            Mathf.Clamp01(parent.color.b + Random.Range(-colorDelta, colorDelta)),
            1f
        );

        return new CellTraits(mutatedSize, mutatedColor, 0f);
    }

    // Si el fondo cambia drásticamente, reiniciamos la memoria para que vuelva a aprender
    public void ResetMemoryOnBackgroundChange()
    {
        survivorsHistory.Clear();
        explorationRate = 0.8f; // Volver a explorar para el nuevo entorno
    }
}