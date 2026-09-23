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
    public AudioSource popAudioSource;

    [Header("Parámetros del Juego")]
    public float roundDuration = 10f;

    [Header("Generación")]
    public int minSimilarCells = 3;
    public int maxSimilarCells = 4;
    public int independentCells = 2;

    public int maxCellsPerRound = 8;

    private float currentTimer;
    private int score = 0;
    private int currentRound = 1;
    private bool isRoundActive = false;

    private List<CellAgent> activeCells =
        new List<CellAgent>();

    // Características de las células que sobrevivieron
    // en la ronda anterior.
    private List<CellTraits> previousSurvivors =
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
                Mathf.CeilToInt(
                    currentTimer
                ).ToString() +
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

        SpawnCellsForRound();
    }

    private void SpawnCellsForRound()
    {
        ClearActiveCells();

        if (previousSurvivors.Count == 0)
        {
            int amount = Random.Range(4, 8);

            if (amount > maxCellsPerRound)
            {
                amount = maxCellsPerRound;
            }

            for (int i = 0; i < amount; i++)
            {
                CreateIndependentCell();
            }

            return;
        }

        for (int i = 0; i < previousSurvivors.Count; i++)
        {
            int amount = Random.Range(minSimilarCells, maxSimilarCells + 1);

            for (int j = 0; j < amount; j++)
            {
                if (activeCells.Count >= maxCellsPerRound)
                {
                    break;
                }

                CreateSimilarCell(previousSurvivors[i]);
            }

            if (activeCells.Count >= maxCellsPerRound)
            {
                break;
            }
        }

        for (int i = 0; i < independentCells; i++)
        {
            if (activeCells.Count >= maxCellsPerRound)
            {
                break;
            }

            CreateIndependentCell();
        }
    }

    void CreateIndependentCell()
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

        float randomSize =
            Random.Range(
                0.8f,
                2.0f
            );

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

        activeCells.Add(agent);
    }

    void CreateSimilarCell(
        CellTraits parent)
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

        float newSize;
        Color newColor;

        if (AdaptiveMLSystem.Instance != null)
        {
            AdaptiveMLSystem.Instance.MutateTraits(
                parent.size,
                parent.color,
                out newSize,
                out newColor
            );
        }
        else
        {
            newSize =
                Mathf.Clamp(
                    parent.size +
                    Random.Range(
                        -0.15f,
                        0.15f
                    ),
                    0.4f,
                    2.5f
                );

            newColor =
                new Color(
                    Mathf.Clamp01(
                        parent.color.r +
                        Random.Range(
                            -0.10f,
                            0.10f
                        )
                    ),
                    Mathf.Clamp01(
                        parent.color.g +
                        Random.Range(
                            -0.10f,
                            0.10f
                        )
                    ),
                    Mathf.Clamp01(
                        parent.color.b +
                        Random.Range(
                            -0.10f,
                            0.10f
                        )
                    ),
                    1f
                );
        }

        agent.ApplyTraits(
            newSize,
            newColor
        );

        activeCells.Add(agent);
    }

    void EndRound()
    {
        isRoundActive = false;

        previousSurvivors.Clear();

        foreach (
            CellAgent cell
            in activeCells
        )
        {
            if (
                cell != null &&
                cell.survived
            )
            {
                CellTraits survivor =
                    new CellTraits(
                        cell.size,
                        cell.color,
                        10f
                    );

                previousSurvivors.Add(
                    survivor
                );


                // Informar al sistema de aprendizaje.
                if (
                    AdaptiveMLSystem.Instance != null
                )
                {
                    AdaptiveMLSystem.Instance.RecordSurvivor(
                        cell.size,
                        cell.color
                    );
                }
            }
        }


        Debug.Log(
            "RONDA " +
            currentRound +
            " TERMINADA."
        );

        Debug.Log(
            "Supervivientes: " +
            previousSurvivors.Count
        );


        currentRound++;

        StartNewRound();
    }


    public void OnCellClicked(
        CellAgent cell)
    {
        if (!isRoundActive)
        {
            return;
        }

        if (cell == null)
        {
            return;
        }
        if (popAudioSource != null)
        {
            popAudioSource.Play();
        }
        // La célula fue eliminada.
        cell.survived = false;

        score += 10;

        UpdateScoreUI();

        activeCells.Remove(cell);


        // Registrar el fracaso en el sistema
        // de aprendizaje.
        if (
            AdaptiveMLSystem.Instance != null
        )
        {
            AdaptiveMLSystem.Instance.RecordFailure(
                cell.size,
                cell.color
            );
        }

        Debug.Log(
            "CÉLULA ELIMINADA -> " +
            "Tamaño: " +
            cell.size +
            " | Color: " +
            cell.color
        );
    }

    Vector3 GetRandomScreenPosition()
    {
        float viewX =
            Random.Range(
                0.1f,
                0.9f
            );

        float viewY =
            Random.Range(
                0.1f,
                0.9f
            );

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
        foreach (
            CellAgent cell
            in activeCells
        )
        {
            if (cell != null)
            {
                Destroy(
                    cell.gameObject
                );
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
