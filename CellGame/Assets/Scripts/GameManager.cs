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
    public GameObject[] cellPrefabs; // Tus 6 prefabs de aliens
    public Camera mainCamera;
    public SpriteRenderer backgroundRenderer; // Para cambiar el color de fondo en cada ronda/partida

    [Header("Parámetros del Juego")]
    public float roundDuration = 10f;
    private float currentTimer;
    private int score = 0;
    private int currentRound = 1;
    private bool isRoundActive = false;

    private List<CellAgent> activeCells = new List<CellAgent>();
    private AdaptiveMLSystem mlSystem;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        mlSystem = gameObject.AddComponent<AdaptiveMLSystem>();
    }

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        UpdateScoreUI();
        StartNewRound();
    }

    void Update()
    {
        if (!isRoundActive) return;

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

        if (roundText != null) roundText.text = "Ronda: " + currentRound;

        // Opcional: Cambiar el color del fondo al azar en cada ronda (o cada X rondas) para probar la adaptación del ML
        ChangeBackgroundColor();

        SpawnCellsForRound();
    }

    void ChangeBackgroundColor()
    {
        if (backgroundRenderer != null)
        {
            // Cambia el color del fondo de la pantalla al azar
            backgroundRenderer.color = new Color(Random.value, Random.value, Random.value, 1f);
        }
    }

    void SpawnCellsForRound()
    {
        ClearActiveCells();

        // Cantidad aleatoria de células por ronda
        int cellCount = Random.Range(4, 8);

        for (int i = 0; i < cellCount; i++)
        {
            // Seleccionar uno de los 6 prefabs de aliens al azar
            GameObject selectedPrefab = cellPrefabs[Random.Range(0, cellPrefabs.Length)];

            Vector3 spawnPosition = GetRandomScreenPosition();
            GameObject newCellObj = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

            CellAgent agent = newCellObj.GetComponent<CellAgent>();
            if (agent == null)
            {
                agent = newCellObj.AddComponent<CellAgent>();
            }

            // El algoritmo asigna color y tamaño según su aprendizaje a ciegas
            CellTraits predictedTraits = mlSystem.PredictBestTraits();
            agent.ApplyTraits(predictedTraits.size, predictedTraits.color);

            activeCells.Add(agent);
        }
    }

    void EndRound()
    {
        isRoundActive = false;

        // Registrar SOLO las células que sobrevivieron
        foreach (var cell in activeCells)
        {
            if (cell != null && cell.survived)
            {
                // Usamos RecordSurvivor en lugar de RecordExperience
                mlSystem.RecordSurvivor(cell.size, cell.color);
            }
        }

        currentRound++;
        StartNewRound();
    }

    public void OnCellClicked(CellAgent cell)
    {
        score += 10;
        UpdateScoreUI();

        cell.survived = false; 
        activeCells.Remove(cell);
    }

    Vector3 GetRandomScreenPosition()
    {
        float viewX = Random.Range(0.1f, 0.9f);
        float viewY = Random.Range(0.1f, 0.9f);
        Vector3 worldPos = mainCamera.ViewportToWorldPoint(new Vector3(viewX, viewY, 10f));
        worldPos.z = 0f;
        return worldPos;
    }

    void ClearActiveCells()
    {
        foreach (var cell in activeCells)
        {
            if (cell != null) Destroy(cell.gameObject);
        }
        activeCells.Clear();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Puntos: " + score;
    }
}