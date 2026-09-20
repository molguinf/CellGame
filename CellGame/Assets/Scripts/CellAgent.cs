using UnityEngine;

public class CellAgent : MonoBehaviour
{
    [Header("Parámetros del Agente")]
    public float size = 1f;
    public Color color = Color.white;
    public bool survived = true;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Aplica las características propuestas por el algoritmo de ML
    public void ApplyTraits(float newSize, Color newColor)
    {
        size = Mathf.Clamp(newSize, 0.4f, 2.5f);
        color = newColor;

        transform.localScale = new Vector3(size, size, 1f);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
        survived = true;
    }

    // El jugador hace clic en la célula para eliminarla
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