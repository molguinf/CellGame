using System.Collections.Generic;
using UnityEngine;


// ======================================================
// CARACTERÍSTICAS DE UNA EXPERIENCIA
// ======================================================

[System.Serializable]
public class CellTraits
{
    public float size;
    public Color color;
    public float fitness;


    public CellTraits(
        float newSize,
        Color newColor,
        float newFitness)
    {
        size = newSize;
        color = newColor;
        fitness = newFitness;
    }
}


// ======================================================
// SISTEMA DE APRENDIZAJE
// ======================================================

public class AdaptiveMLSystem : MonoBehaviour
{
    public static AdaptiveMLSystem Instance;


    [Header("Exploración")]
    public float explorationRate = 0.30f;


    [Header("Límites de tamaño")]
    public float minSize = 0.4f;
    public float maxSize = 2.5f;


    [Header("Memoria")]
    public int maxMemory = 20;


    // Experiencias positivas.
    private List<CellTraits> survivorsHistory =
        new List<CellTraits>();


    // Experiencias negativas.
    private List<CellTraits> failuresHistory =
        new List<CellTraits>();


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // ==================================================
    // REGISTRAR SUPERVIVIENTE
    // ==================================================

    public void RecordSurvivor(
        float size,
        Color color)
    {
        CellTraits experience =
            new CellTraits(
                size,
                color,
                10f
            );

        survivorsHistory.Add(
            experience
        );


        // La exploración disminuye lentamente
        // conforme se acumulan experiencias.
        explorationRate =
            Mathf.Max(
                0.05f,
                explorationRate - 0.02f
            );


        if (
            survivorsHistory.Count >
            maxMemory
        )
        {
            survivorsHistory.RemoveAt(0);
        }


        Debug.Log(
            "ML ÉXITO -> " +
            "Tamaño: " +
            size +
            " | Color: " +
            color +
            " | Recompensa: +10"
        );
    }


    // ==================================================
    // REGISTRAR FRACASO
    // ==================================================

    public void RecordFailure(
        float size,
        Color color)
    {
        CellTraits experience =
            new CellTraits(
                size,
                color,
                -5f
            );

        failuresHistory.Add(
            experience
        );


        if (
            failuresHistory.Count >
            maxMemory
        )
        {
            failuresHistory.RemoveAt(0);
        }


        Debug.Log(
            "ML FALLO -> " +
            "Tamaño: " +
            size +
            " | Color: " +
            color +
            " | Penalización: -5"
        );
    }


    // ==================================================
    // MUTAR CARACTERÍSTICAS DE UN SUPERVIVIENTE
    // ==================================================

    public void MutateTraits(
        float parentSize,
        Color parentColor,
        out float newSize,
        out Color newColor)
    {
        // ----------------------------------------------
        // MUTACIÓN DEL TAMAÑO
        // ----------------------------------------------

        float sizeVariation =
            Random.Range(
                -0.15f,
                0.15f
            );

        newSize =
            Mathf.Clamp(
                parentSize +
                sizeVariation,
                minSize,
                maxSize
            );


        // ----------------------------------------------
        // MUTACIÓN DEL COLOR
        // ----------------------------------------------

        float colorVariation =
            0.10f;

        float red =
            parentColor.r +
            Random.Range(
                -colorVariation,
                colorVariation
            );

        float green =
            parentColor.g +
            Random.Range(
                -colorVariation,
                colorVariation
            );

        float blue =
            parentColor.b +
            Random.Range(
                -colorVariation,
                colorVariation
            );


        newColor =
            new Color(
                Mathf.Clamp01(red),
                Mathf.Clamp01(green),
                Mathf.Clamp01(blue),
                1f
            );


        Debug.Log(
            "ML MUTACIÓN -> " +
            "Características similares " +
            "al superviviente."
        );
    }


    // ==================================================
    // OBTENER UNA EXPERIENCIA APRENDIDA
    // ==================================================

    public CellTraits GetBestExperience()
    {
        if (
            survivorsHistory.Count == 0
        )
        {
            return null;
        }


        // ----------------------------------------------
        // EXPLORACIÓN
        // ----------------------------------------------

        if (
            Random.value <
            explorationRate
        )
        {
            Debug.Log(
                "ML EXPLORACIÓN -> " +
                "Se probarán características nuevas."
            );

            return null;
        }


        // ----------------------------------------------
        // BUSCAR MEJOR EXPERIENCIA
        // ----------------------------------------------

        int bestIndex = 0;


        for (
            int i = 1;
            i < survivorsHistory.Count;
            i++
        )
        {
            if (
                survivorsHistory[i].fitness >
                survivorsHistory[bestIndex].fitness
            )
            {
                bestIndex = i;
            }
        }


        CellTraits bestExperience =
            survivorsHistory[bestIndex];


        Debug.Log(
            "ML APRENDIZAJE -> " +
            "Se utilizará una experiencia exitosa."
        );


        return bestExperience;
    }


    // ==================================================
    // GENERAR CARACTERÍSTICAS INDEPENDIENTES
    // ==================================================

    public CellTraits GenerateIndependentTraits()
    {
        float randomSize =
            Random.Range(
                minSize,
                maxSize
            );


        Color randomColor =
            new Color(
                Random.value,
                Random.value,
                Random.value,
                1f
            );


        Debug.Log(
            "ML EXPLORACIÓN -> " +
            "Nueva célula independiente."
        );


        return new CellTraits(
            randomSize,
            randomColor,
            0f
        );
    }


    // ==================================================
    // DISTANCIA ENTRE DOS COLORES
    // ==================================================

    public float GetColorDistance(
        Color first,
        Color second)
    {
        float redDifference =
            first.r -
            second.r;

        float greenDifference =
            first.g -
            second.g;

        float blueDifference =
            first.b -
            second.b;


        float distance =
            Mathf.Sqrt(
                redDifference *
                redDifference +

                greenDifference *
                greenDifference +

                blueDifference *
                blueDifference
            );


        return distance;
    }


    // ==================================================
    // OBTENER CANTIDAD DE EXPERIENCIAS
    // ==================================================

    public int GetSurvivorMemoryCount()
    {
        return survivorsHistory.Count;
    }


    public int GetFailureMemoryCount()
    {
        return failuresHistory.Count;
    }
}