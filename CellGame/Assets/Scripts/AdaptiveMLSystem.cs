using System.Collections.Generic;
using UnityEngine;

public class AdaptiveMLSystem : MonoBehaviour
{
    [Header("Aprendizaje")]
    public float explorationChance = 0.15f;

    private List<int> colorStates =
        new List<int>();

    private List<int> sizeStates =
        new List<int>();

    private List<float> rewards =
        new List<float>();

    // 0 = rojo
    // 1 = verde
    // 2 = azul
    // 3 = amarillo
    // 4 = morado
    // 5 = gris
    // 6 = cian
    // 7 = naranja

    Color[] possibleColors =
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        new Color(0.6f, 0.1f, 0.8f),
        Color.gray,
        Color.cyan,
        new Color(1f, 0.5f, 0f)
    };

    // 0 = pequeña
    // 1 = mediana
    // 2 = grande

    float[] possibleSizes =
    {
        0.7f,
        1.2f,
        1.8f
    };

    public void RegisterSuccess(
        float size,
        Color color)
    {
        int colorIndex =
            GetClosestColorIndex(color);

        int sizeIndex =
            GetClosestSizeIndex(size);

        int experienceIndex =
            FindExperience(
                colorIndex,
                sizeIndex
            );

        if (experienceIndex == -1)
        {
            colorStates.Add(colorIndex);
            sizeStates.Add(sizeIndex);
            rewards.Add(3f);
        }
        else
        {
            rewards[experienceIndex] += 3f;
        }

        Debug.Log(
            "ML ÉXITO -> Color: " +
            GetColorName(colorIndex) +
            " | Tamaño: " +
            GetSizeName(sizeIndex) +
            " | Recompensa: " +
            GetExperienceReward(
                colorIndex,
                sizeIndex
            )
        );
    }

    public void RegisterFailure(
        float size,
        Color color)
    {
        int colorIndex =
            GetClosestColorIndex(color);

        int sizeIndex =
            GetClosestSizeIndex(size);

        int experienceIndex =
            FindExperience(
                colorIndex,
                sizeIndex
            );

        if (experienceIndex == -1)
        {
            colorStates.Add(colorIndex);
            sizeStates.Add(sizeIndex);
            rewards.Add(-1f);
        }
        else
        {
            rewards[experienceIndex] -= 1f;

            if (rewards[experienceIndex] < -5f)
            {
                rewards[experienceIndex] = -5f;
            }
        }

        Debug.Log(
            "ML FALLO -> Color: " +
            GetColorName(colorIndex) +
            " | Tamaño: " +
            GetSizeName(sizeIndex) +
            " | Recompensa: " +
            GetExperienceReward(
                colorIndex,
                sizeIndex
            )
        );
    }

    public void GetNextTraits(
        out float newSize,
        out Color newColor)
    {
        // Al principio o durante la exploración,
        // prueba una combinación nueva.
        if (
            colorStates.Count == 0 ||
            Random.value < explorationChance
        )
        {
            int randomColor =
                Random.Range(
                    0,
                    possibleColors.Length
                );

            int randomSize =
                Random.Range(
                    0,
                    possibleSizes.Length
                );

            newColor =
                possibleColors[randomColor];

            newSize =
                possibleSizes[randomSize];

            Debug.Log(
                "ML EXPLORACIÓN -> " +
                GetColorName(randomColor) +
                " + " +
                GetSizeName(randomSize)
            );

            return;
        }

        int bestExperience =
            GetBestExperience();

        int bestColor =
            colorStates[bestExperience];

        int bestSize =
            sizeStates[bestExperience];

        newColor =
            possibleColors[bestColor];

        newSize =
            possibleSizes[bestSize];

        Debug.Log(
            "ML APRENDIZAJE -> " +
            GetColorName(bestColor) +
            " + " +
            GetSizeName(bestSize) +
            " | Recompensa: " +
            rewards[bestExperience]
        );
    }

    int GetBestExperience()
    {
        int bestIndex = 0;

        for (
            int i = 1;
            i < rewards.Count;
            i++
        )
        {
            if (
                rewards[i] >
                rewards[bestIndex]
            )
            {
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    int FindExperience(
        int colorIndex,
        int sizeIndex)
    {
        for (
            int i = 0;
            i < colorStates.Count;
            i++
        )
        {
            if (
                colorStates[i] == colorIndex &&
                sizeStates[i] == sizeIndex
            )
            {
                return i;
            }
        }

        return -1;
    }

    float GetExperienceReward(
        int colorIndex,
        int sizeIndex)
    {
        int index =
            FindExperience(
                colorIndex,
                sizeIndex
            );

        if (index == -1)
        {
            return 0f;
        }

        return rewards[index];
    }

    int GetClosestColorIndex(Color color)
    {
        int closestIndex = 0;

        float smallestDistance =
            GetColorDistance(
                color,
                possibleColors[0]
            );

        for (
            int i = 1;
            i < possibleColors.Length;
            i++
        )
        {
            float distance =
                GetColorDistance(
                    color,
                    possibleColors[i]
                );

            if (
                distance <
                smallestDistance
            )
            {
                smallestDistance =
                    distance;

                closestIndex = i;
            }
        }

        return closestIndex;
    }

    float GetColorDistance(
        Color first,
        Color second)
    {
        float red =
            Mathf.Abs(
                first.r -
                second.r
            );

        float green =
            Mathf.Abs(
                first.g -
                second.g
            );

        float blue =
            Mathf.Abs(
                first.b -
                second.b
            );

        return red + green + blue;
    }

    int GetClosestSizeIndex(
        float size)
    {
        int closestIndex = 0;

        float smallestDistance =
            Mathf.Abs(
                size -
                possibleSizes[0]
            );

        for (
            int i = 1;
            i < possibleSizes.Length;
            i++
        )
        {
            float distance =
                Mathf.Abs(
                    size -
                    possibleSizes[i]
                );

            if (
                distance <
                smallestDistance
            )
            {
                smallestDistance =
                    distance;

                closestIndex = i;
            }
        }

        return closestIndex;
    }

    string GetColorName(
        int colorIndex)
    {
        if (colorIndex == 0)
        {
            return "ROJO";
        }

        if (colorIndex == 1)
        {
            return "VERDE";
        }

        if (colorIndex == 2)
        {
            return "AZUL";
        }

        if (colorIndex == 3)
        {
            return "AMARILLO";
        }

        if (colorIndex == 4)
        {
            return "MORADO";
        }

        if (colorIndex == 5)
        {
            return "GRIS";
        }

        if (colorIndex == 6)
        {
            return "CIAN";
        }

        return "NARANJA";
    }

    string GetSizeName(
        int sizeIndex)
    {
        if (sizeIndex == 0)
        {
            return "PEQUEÑA";
        }

        if (sizeIndex == 1)
        {
            return "MEDIANA";
        }

        return "GRANDE";
    }

    public int GetExperienceCount()
    {
        return colorStates.Count;
    }
}