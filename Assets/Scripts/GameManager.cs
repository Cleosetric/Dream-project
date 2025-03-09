using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Monster Spawn Settings")]
    public int maxMonsters = 10; // Max limit of monsters that can be spawned
    public Vector2 spawnAreaMin = new Vector2(-10, -10);
    public Vector2 spawnAreaMax = new Vector2(10, 10);
    public float initialSpawnInterval = 10f; // Starting interval
    public float minSpawnInterval = 5f; // Minimum interval
    public bool constantSpawn = true;
    public bool isBattle = false;

    public GameObject monsterPrefab;

    public enum BattleType
    {
        Normal,
        Advantage,
        Ambushed,
    }

    public BattleType currentBattleType;

    private float currentSpawnInterval;
    private int currentMonsterCount = 0;
    private GameObject encounter;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentSpawnInterval = initialSpawnInterval;
    }

    public void StartCombat()
    {
        StartCoroutine(SpawnMonsterRoutine());
    }

    private IEnumerator SpawnMonsterRoutine()
    {
        while (true)
        {
            if (currentMonsterCount < maxMonsters)
            {
                if (!isBattle)
                {
                    SpawnMonster();
                    if (!constantSpawn)
                        AdjustSpawnInterval();
                }
            }
            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    private void SpawnMonster()
    {
        if (monsterPrefab == null)
        {
            Debug.LogError("Monster prefab is not assigned!");
            return;
        }

        Vector2 spawnPosition = new Vector2(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y)
        );

        Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
        currentMonsterCount++;
    }

    private void AdjustSpawnInterval()
    {
        if (currentSpawnInterval > minSpawnInterval)
        {
            currentSpawnInterval -= 1f;
            if (currentSpawnInterval < minSpawnInterval)
            {
                currentSpawnInterval = minSpawnInterval;
            }
        }
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ProcessBattle(BattleType battleType, GameObject enemyEncounter)
    {
        isBattle = true;
        currentBattleType = battleType;
        encounter = enemyEncounter;
        SceneManager.LoadScene("Battle", LoadSceneMode.Additive);
        //SceneManager.SetActiveScene(SceneManager.GetSceneByName("Battle"));
        SetActiveOverworld(false);
    }

    public void ProcessBattleEnd(BattleSystem.BattleResult result)
    {
        SceneManager.UnloadSceneAsync("Battle");
        SetActiveOverworld(true);

        if (result == BattleSystem.BattleResult.Victory)
        {
            Destroy(encounter);
            encounter = null;
            currentMonsterCount--;
        }
        else if (
            result == BattleSystem.BattleResult.Defeat
            || result == BattleSystem.BattleResult.Run
        )
        {
            Vector2 spawnPosition = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );
            encounter.transform.position = spawnPosition;
            encounter = null;
        }
        isBattle = false;
    }

    private void SetActiveOverworld(bool state)
    {
        foreach (GameObject obj in SceneManager.GetSceneByName("Dungeon").GetRootGameObjects())
        {
            obj.SetActive(state);
        }
    }
}
