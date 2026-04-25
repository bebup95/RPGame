using UnityEngine;

public class Enemy_Respawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] respawnPoints;

    [Header("Spawn Settings")]
    [SerializeField] private float coolDown = 3f; // Gán giá trị mặc định an toàn (3 giây)
    [Space]
    [SerializeField] private float coolDownDecreaseRate = 0.05f; // Tốc độ giảm thời gian chờ
    [SerializeField] private float coolDownCap = .5f; // Không cho phép spawn nhanh hơn 1.5 giây/con

    private float timer;
    private Transform player;

    private void Awake()
    {
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
        if (respawnPoints.Length == 0) return;

        int respawnPointIndex = Random.Range(0, respawnPoints.Length);
        Vector3 spawnPoint = respawnPoints[respawnPointIndex].position;

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity);

        // Đảm bảo player không bị null trước khi so sánh vị trí
        if (player != null && newEnemy.transform.position.x > player.position.x)
        {
            newEnemy.GetComponent<Enemy>().Flip();
        }
    }
}