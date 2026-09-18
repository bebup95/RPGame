using UnityEngine;

public class Enemy_Respawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject[] additionalEnemyPrefabs;
    [SerializeField] private Transform[] respawnPoints;

    [Header("Spawn Settings")]
    [SerializeField] private float coolDown = 3f; // Gán giá trị mặc định an toàn (3 giây)
    [Space]
    [SerializeField] private float coolDownDecreaseRate = 0.05f; // Tốc độ giảm thời gian chờ
    [SerializeField] private float coolDownCap = 0.7f; // Không cho phép spawn nhanh hơn 0.7 giây/con

    private float timer;
    private Transform player;

    private void Awake()
    {
        if (!IsValidEnemyPrefab(enemyPrefab))
        {
            Debug.LogError("Enemy Respawner requires a prefab with an Enemy component.", this);
            enabled = false;
            return;
        }

        if (additionalEnemyPrefabs != null)
        {
            foreach (GameObject additionalPrefab in additionalEnemyPrefabs)
            {
                if (!IsValidEnemyPrefab(additionalPrefab))
                {
                    Debug.LogError("Enemy Respawner additional prefabs must contain an Enemy component.", this);
                    enabled = false;
                    return;
                }
            }
        }

        if (respawnPoints == null || respawnPoints.Length == 0)
        {
            Debug.LogError("Enemy Respawner requires at least one respawn point.", this);
            enabled = false;
            return;
        }

        // Kiểm tra xem player có tồn tại không để tránh lỗi NullReference
        Player playerObj = FindFirstObjectByType<Player>();
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // FIX 1: Gán timer bằng coolDown ngay từ đầu để tránh quái spawn lập tức khi vừa vào game
        timer = coolDown;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            CreateNewEnemy();

            // FIX 2: Reset lại timer
            timer = coolDown;

            // Tăng độ khó bằng cách giảm thời gian spawn, nhưng chặn lại ở mức coolDownCap
            coolDown = Mathf.Max(coolDownCap, coolDown - coolDownDecreaseRate);
        }
    }

    private void CreateNewEnemy()
    {
        // Đề phòng trường hợp bạn quên kéo Respawn Points vào Inspector
        if (enemyPrefab == null || respawnPoints == null || respawnPoints.Length == 0)
            return;

        int respawnPointIndex = Random.Range(0, respawnPoints.Length);
        Transform respawnPoint = respawnPoints[respawnPointIndex];
        if (respawnPoint == null)
        {
            Debug.LogError("Enemy Respawner contains an unassigned respawn point.", this);
            return;
        }

        Vector3 spawnPoint = respawnPoint.position;

        GameObject selectedPrefab = SelectEnemyPrefab();
        GameObject newEnemy = Instantiate(selectedPrefab, spawnPoint, Quaternion.identity);

        // Đảm bảo player không bị null trước khi so sánh vị trí
        if (player != null && newEnemy.transform.position.x > player.position.x)
        {
            if (newEnemy.TryGetComponent(out Enemy enemy))
            {
                enemy.Flip();
            }
        }
    }

    private GameObject SelectEnemyPrefab()
    {
        int additionalCount = additionalEnemyPrefabs != null ? additionalEnemyPrefabs.Length : 0;
        int selection = Random.Range(0, additionalCount + 1);
        return selection == 0 ? enemyPrefab : additionalEnemyPrefabs[selection - 1];
    }

    private static bool IsValidEnemyPrefab(GameObject candidate)
    {
        return candidate != null && candidate.TryGetComponent(out Enemy _);
    }
}
