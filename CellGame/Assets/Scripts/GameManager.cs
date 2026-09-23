using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Referencias de UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI roundText;

    [Header("Configuración de la Escena")]
    public GameObject[] cellPrefabs;
    public Camera mainCamera;
    public SpriteRenderer backgroundRenderer;

    [Header("Parámetros del Juego")]
    public float roundDuration = 10f;

    private float currentTimer;
    private int score = 0;
    private int currentRound = 1;
    private bool isRoundActive = false;

    private List<CellAgent> activeCells =
        new List<CellAgent>();

    private List<float> successfulSizes =
        new List<float>();

    private List<Color> successfulColors =
        new List<Color>();

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

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        UpdateScoreUI();
        StartNewRound();
    }

    void Update()
    {
        if (!isRoundActive)
        {
            return;
        }

        currentTimer -= Time.deltaTime;

        if (timerText != null)
        {
            timerText.text =
                "Tiempo: " +
                Mathf.CeilToInt(currentTimer).ToString() +
                "s";
        }

        if (currentTimer <= 0f)
        {
            EndRound();
        }
    }

    void StartNewRound()
    {
        currentTimer = roundDuration;
        isRoundActive = true;

        if (roundText != null)
        {
            roundText.text =
                "Ronda: " +
                currentRound;
        }

        ChangeBackgroundColor();
        SpawnCellsForRound();
    }

    void ChangeBackgroundColor()
    {
        if (backgroundRenderer == null)
        {
            return;
        }

        int environment =
            (currentRound - 1) % 4;

        if (environment == 0)
        {
            backgroundRenderer.color =
                new Color(0.15f, 0.45f, 0.20f);
        }
        else if (environment == 1)
        {
            backgroundRenderer.color =
                new Color(0.15f, 0.30f, 0.55f);
        }
        else if (environment == 2)
        {
            backgroundRenderer.color =
                new Color(0.45f, 0.45f, 0.45f);
        }
        else
        {
            backgroundRenderer.color =
                new Color(0.45f, 0.20f, 0.50f);
        }
    }

    void SpawnCellsForRound()
    {
        ClearActiveCells();

        int cellCount =
            Random.Range(4, 8);

        for (int i = 0; i < cellCount; i++)
        {
            GameObject selectedPrefab =
                cellPrefabs[
                    Random.Range(
                        0,
                        cellPrefabs.Length
                    )
                ];

            Vector3 spawnPosition =
                GetRandomScreenPosition();

            GameObject newCellObj =
                Instantiate(
                    selectedPrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            CellAgent agent =
                newCellObj.GetComponent<CellAgent>();

            if (agent == null)
            {
                agent =
                    newCellObj.AddComponent<CellAgent>();
            }

            bool createFromSuccessfulCell =
                false;

            if (successfulSizes.Count > 0)
            {
                float chance = Random.value;

                if (chance < 0.7f)
                {
                    createFromSuccessfulCell = true;
                }
            }

            if (createFromSuccessfulCell)
            {
                CreateFromSuccessfulCell(agent);
            }
            else
            {
                CreateRandomCell(agent);
            }

            activeCells.Add(agent);
        }
    }

    void CreateRandomCell(CellAgent agent)
    {
        float randomSize =
            Random.Range(0.8f, 2.0f);

        Color randomColor =
            new Color(
                Random.value,
                Random.value,
                Random.value,
                1f
            );

        agent.ApplyTraits(
            randomSize,
            randomColor
        );
    }

    void CreateFromSuccessfulCell(CellAgent agent)
    {
        int index =
            Random.Range(
                0,
                successfulSizes.Count
            );

        float parentSize =
            successfulSizes[index];

        Color parentColor =
            successfulColors[index];

        float sizeVariation =
            Random.Range(-0.15f, 0.15f);

        float newSize =
            Mathf.Clamp(
                parentSize + sizeVariation,
                0.4f,
                2.5f
            );

        float redVariation =
            Random.Range(-0.10f, 0.10f);

        float greenVariation =
            Random.Range(-0.10f, 0.10f);

        float blueVariation =
            Random.Range(-0.10f, 0.10f);

        Color newColor =
            new Color(
                Mathf.Clamp01(
                    parentColor.r + redVariation
                ),
                Mathf.Clamp01(
                    parentColor.g + greenVariation
                ),
                Mathf.Clamp01(
                    parentColor.b + blueVariation
                ),
                1f
            );

        agent.ApplyTraits(
            newSize,
            newColor
        );
    }

    void EndRound()
    {
        isRoundActive = false;

        successfulSizes.Clear();
        successfulColors.Clear();

        int survivedCells = 0;

        foreach (CellAgent cell in activeCells)
        {
            if (cell != null && cell.survived)
            {
                survivedCells++;

                successfulSizes.Add(cell.size);
                successfulColors.Add(cell.color);
            }
        }

        Debug.Log(
            "Ronda " +
            currentRound +
            " terminada. Células sobrevivientes: " +
            survivedCells
        );

        currentRound++;

        StartNewRound();
    }

    public void OnCellClicked(CellAgent cell)
    {
        if (!isRoundActive)
        {
            return;
        }

        score += 10;

        UpdateScoreUI();

        cell.survived = false;

        activeCells.Remove(cell);
    }

    Vector3 GetRandomScreenPosition()
    {
        float viewX =
            Random.Range(0.1f, 0.9f);

        float viewY =
            Random.Range(0.1f, 0.9f);

        Vector3 worldPos =
            mainCamera.ViewportToWorldPoint(
                new Vector3(
                    viewX,
                    viewY,
                    10f
                )
            );

        worldPos.z = 0f;

        return worldPos;
    }

    void ClearActiveCells()
    {
        foreach (CellAgent cell in activeCells)
        {
            if (cell != null)
            {
                Destroy(cell.gameObject);
            }
        }

        activeCells.Clear();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text =
                "Puntos: " +
                score;
        }
    }
}