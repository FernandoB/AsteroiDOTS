using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class MainGame : MonoBehaviour
{
    public static MainGame Instance = null;

    public GameObject gameStartUI;

    private EntityManager entityManager;

    private bool gameRunning = false;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Instance = this;
    }

    void Start()
    {
        gameStartUI.SetActive(true);
    }

    void Update()
    {
        if (!gameRunning)
        {
            if (Input.anyKeyDown)
            {
                OnStartGame();
            }
        }
    }

    public void OnStartGame()
    {
        gameStartUI.SetActive(false);

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity entity = entityManager.CreateEntity();
        entityManager.AddComponent<GameStateStart>(entity);

        gameRunning = true;
    }

    public void GameEnd()
    {
        gameStartUI.SetActive(true);

        gameRunning = false;
    }

    public void PlayerReadyToRes()
    {
        gameStartUI.SetActive(true);
    }

    public void PlayerRes()
    {
        gameStartUI.SetActive(false);
    }

    public void SetPlayerLives(int lives)
    {
        Debug.Log("Player Lives: " + lives);
    }
}
