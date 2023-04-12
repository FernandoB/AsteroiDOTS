using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class MainGame : MonoBehaviour
{
    public GameObject gameStartUI;

    private EntityManager entityManager;

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
}
