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

    private List<CellAgent> activeCells = new List<CellAgent>();

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
            timerText.text = "Tiempo: " + Mathf.CeilToInt(currentTimer).ToString() + "s";
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
            roundText.text = "Ronda: " + currentRound;
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

        int environment = (currentRound - 1) % 4;

        if (environment == 0)
        {
            backgroundRenderer.color = new Color(0.15f, 0.45f, 0.20f);
        }
        else if (environment == 1)
        {
            backgroundRenderer.color = new Color(0.15f, 0.30f, 0.55f);
        }
        else if (environment == 2)
        {
            backgroundRenderer.color = new Color(0.45f, 0.45f, 0.45f);
        }
        else
        {
            backgroundRenderer.color = new Color(0.45f, 0.20f, 0.50f);
        }
    }

    void SpawnCellsForRound()
    {
        ClearActiveCells();

        int cellCount = Random.Range(4, 8);

        for (int i = 0; i < cellCount; i++)
        {
            GameObject selectedPrefab = cellPrefabs[Random.Range(0, cellPrefabs.Length)];

            Vector3 spawnPosition = GetRandomScreenPosition();

            GameObject newCellObj = Instantiate(
                selectedPrefab,
                spawnPosition,
                Quaternion.identity
            );

            CellAgent agent = newCellObj.GetComponent<CellAgent>();

            if (agent == null)
            {
                agent = newCellObj.AddComponent<CellAgent>();
            }

            float randomSize = Random.Range(0.8f, 2.0f);

            Color randomColor = new Color(
                Random.value,
                Random.value,
                Random.value,
                1f
            );

            agent.ApplyTraits(randomSize, randomColor);

            activeCells.Add(agent);
        }
    }

    void EndRound()
    {
        isRoundActive = false;

        int survivedCells = 0;

        foreach (CellAgent cell in activeCells)
        {
            if (cell != null && cell.survived)
            {
                survivedCells++;
            }
        }

        Debug.Log(
            "Ronda " + currentRound +
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
        float viewX = Random.Range(0.1f, 0.9f);
        float viewY = Random.Range(0.1f, 0.9f);

        Vector3 worldPos = mainCamera.ViewportToWorldPoint(
            new Vector3(viewX, viewY, 10f)
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
            scoreText.text = "Puntos: " + score;
        }
    }
}