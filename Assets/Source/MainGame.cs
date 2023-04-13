using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class MainGame : MonoBehaviour
{
    public static MainGame Instance = null;

    public GameObject gameStartUI;

    private EntityManager entityManager;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Instance = this;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnClickPlayGame()
    {
        gameStartUI.SetActive(false);

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity entity = entityManager.CreateEntity();
        entityManager.AddComponent<GameStateStart>(entity);
    }

    public void GameEnd()
    {
        gameStartUI.SetActive(true);
    }
}
