using UnityEngine;

[System.Serializable]
public class CellLineage
{
    public int id;
    public int successfulGenerations;
    public float size;
    public Color color;

    public CellLineage(int newId, float newSize, Color newColor)
    {
        id = newId;
        successfulGenerations = 0;
        size = newSize;
        color = newColor;
    }

    public CellLineage(int newId, int generations, float newSize, Color newColor)
    {
        id = newId;
        successfulGenerations = generations;
        size = newSize;
        color = newColor;
    }
}