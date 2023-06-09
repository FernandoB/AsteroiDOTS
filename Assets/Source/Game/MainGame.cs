using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.UI;
using TMPro;

public class MainGame : MonoBehaviour
{
    public static MainGame Instance = null;

    public GameObject livesPrefab;

    public GameObject explosionAnim;

    public GameObject gameStartText;

    public TextMeshProUGUI scoreText;

    public GameObject livesContainer;

    public AudioManager audioManager;

    public CameraFollow cameraFollow;

    public GameObject background;

    private EntityManager entityManager;

    private bool gameRunning = false;

    private int currentLives = 0;

    private List<GameObject> livesElements;

    private bool playerDead = false;


    private void Awake()
    {
        Application.targetFrameRate = 60;
        Instance = this;
    }

    void Start()
    {
        livesElements = new List<GameObject>();
        gameStartText.SetActive(true);
        background.SetActive(true);
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

        if(playerDead)
        {
            SetPlayerPos(0f, 0f);
        }
    }

    public void OnStartGame()
    {
        gameStartText.SetActive(false);

        scoreText.text = "" + 0;

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity entity = entityManager.CreateEntity();
        entityManager.AddComponent<GameStateStart>(entity);

        gameRunning = true;
    }

    public void GameEnd()
    {
        gameStartText.SetActive(true);

        gameRunning = false;

        audioManager.StopAll();
    }

    public void PlayerReadyToRes()
    {
        gameStartText.SetActive(true);
    }

    public void PlayerRes()
    {
        gameStartText.SetActive(false);

        playerDead = false;
    }

    public void SetPlayerLives(int lives)
    {
        int deltaLives = lives - currentLives;
        currentLives = lives;

        if (deltaLives > 0)
        {
            for (int i = 0; i < deltaLives; i++)
            {
                GameObject go = GameObject.Instantiate(livesPrefab);
                go.transform.SetParent(livesContainer.transform);
                livesElements.Add(go);
            }
        }
        else
        {
            deltaLives = Mathf.Abs(deltaLives);
            for (int i = deltaLives - 1; i >= 0; i--)
            {
                GameObject go = livesElements[deltaLives - 1];
                livesElements.Remove(go);
                go.transform.SetParent(null);
                GameObject.Destroy(go);
            }
            playerDead = true;            
        }
    }

    public void SetScore(int score)
    {
        scoreText.text = "" + score;
    }

    public void SetFX(FXEnum fxId, float posX, float posY)
    {
        if(fxId == FXEnum.EXPLOSION)
        {
            ExplosionFX(posX, posY);
        }
        else
        {
            audioManager.PlaySound(fxId);
        }
    }

    private Vector3 explosionPos = Vector3.zero;
    private Quaternion explosionRot = Quaternion.identity;
    private void ExplosionFX(float x, float y)
    {
        explosionPos.x = x;
        explosionPos.y = y;
        GameObject.Instantiate(explosionAnim, explosionPos, explosionRot);
    }

    public void SetPlayerPos(float posX, float posY)
    {
        cameraFollow.SetTargetPos(posX, posY);
    }
}
