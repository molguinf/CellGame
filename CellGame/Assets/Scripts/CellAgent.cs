
using UnityEngine;

public class CellAgent : MonoBehaviour
{
    [Header("Parámetros de la célula")]
    public float size = 1f;
    public Color color = Color.white;
    public bool survived = true;

    private SpriteRenderer spriteRenderer;


    void Awake()
    {
        spriteRenderer =
            GetComponent<SpriteRenderer>();
    }


    // ==================================================
    // APLICAR CARACTERÍSTICAS
    // ==================================================

    public void ApplyTraits(
        float newSize,
        Color newColor)
    {
        size =
            Mathf.Clamp(
                newSize,
                0.4f,
                2.5f
            );

        color = newColor;

        transform.localScale =
            new Vector3(
                size,
                size,
                1f
            );

        if (spriteRenderer != null)
        {
            spriteRenderer.color =
                color;
        }

        survived = true;
    }


    // ==================================================
    // CLIC DEL JUGADOR
    // ==================================================

    void OnMouseDown()
    {
        survived = false;

        if (
            GameManager.Instance != null
        )
        {
            GameManager.Instance.OnCellClicked(
                this
            );
        }

        Destroy(gameObject);
    }
}
