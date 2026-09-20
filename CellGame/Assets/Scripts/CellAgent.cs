using UnityEngine;

public class CellAgent : MonoBehaviour
{
    [Header("Parámetros de la célula")]
    public float size = 1f;
    public Color color = Color.white;
    public bool survived = true;

    [Header("Linaje")]
    public int lineageId;
    public int successfulGenerations;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void ApplyTraits(
        float newSize,
        Color newColor,
        int newLineageId,
        int newSuccessfulGenerations)
    {
        size = Mathf.Clamp(newSize, 0.4f, 2.5f);
        color = newColor;

        lineageId = newLineageId;
        successfulGenerations = newSuccessfulGenerations;

        transform.localScale = new Vector3(size, size, 1f);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }

        survived = true;
    }

    public CellLineage GetLineage()
    {
        return new CellLineage(
            lineageId,
            successfulGenerations,
            size,
            color
        );
    }

    void OnMouseDown()
    {
        survived = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCellClicked(this);
        }

        Destroy(gameObject);
    }
}