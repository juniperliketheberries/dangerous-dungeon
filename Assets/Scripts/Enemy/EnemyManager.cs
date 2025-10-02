using UnityEngine;
using UnityEngine.Pool;

/// <summary>
///     An Object Pool for managing enemies (entities) that are spawned into the map
/// </summary>
public class EnemyManager : MonoBehaviour
{
    private float m_Elapsed = 0f;
    private GameObject[] m_SpawnPoints = null;
    private IObjectPool<IEnemy> m_EnemyPool = null;


    public string RespawnTag = "Respawn";
    public int MaxEnemies = 100;
    public float SpawnTime = 2f;

    [Tooltip("Location in world space to store pooled entities")]
    public Vector3 m_CachePosition = Vector3.down * 50;

    public ParticleSystem Particles;

    public GameObject[] EnemyResources;
    public bool DebugMessages = false;

    public int ActiveEnemies { get; private set; } = 0;

    public bool CanCreateEnemy => ActiveEnemies < MaxEnemies;

    public IObjectPool<IEnemy> EnemyPool
    {
        get
        {
            m_EnemyPool ??= new ObjectPool<IEnemy>(CreateEnemy, SpawnEnemy, ReleaseEnemy, DestroyEnemy,
                collectionCheck: true, defaultCapacity: 10, maxSize: MaxEnemies);

            return m_EnemyPool;
        }
    }

    public GameObject[] SpawnPoints
    {
        get
        {
            m_SpawnPoints ??= GameObject.FindGameObjectsWithTag(RespawnTag);

            return m_SpawnPoints;
        }
    }



    private IEnemy CreateEnemy()
    {
        if (!CanCreateEnemy) { return null; }

        IEnemy enemy = Instantiate(
                EnemyResources[Random.Range(0, EnemyResources.Length)],
                m_CachePosition,
                Quaternion.identity,
                transform)
            .GetComponent<IEnemy>();

        enemy.Controller.enabled = false;

        enemy.OnDeath += () =>
        {
            if (!Particles) { return; }

            Particles.transform.position = enemy.Controller.transform.TransformPoint(Vector3.forward * -0.45f);
            Particles.Play();
        };

        enemy.OnDeath += () => m_EnemyPool.Release(enemy);

        return enemy;
    }

    private void SpawnEnemy(IEnemy enemy)
    {
        if (!CanCreateEnemy) { return; }

        if (SpawnPoints == null) { return; }

        int index = Random.Range(0, SpawnPoints.Length);

        // Resets the enemy back to its default settings
        enemy.Reset();

        enemy.Controller.enabled = false;
        enemy.Controller.transform.position = SpawnPoints[index].transform.position;
        enemy.Controller.enabled = true;

        ++ActiveEnemies;

        if (DebugMessages) { Debug.Log($"Spawning an Orc!"); }
    }

    private void ReleaseEnemy(IEnemy enemy)
    {
        enemy.Controller.enabled = false;
        enemy.Controller.transform.position = m_CachePosition;

        --ActiveEnemies;
    }

    private void DestroyEnemy(IEnemy enemy)
    {
        Destroy(enemy.Controller.gameObject);
    }

    private void Update()
    {
        m_Elapsed += Time.deltaTime;

        if (m_Elapsed >= SpawnTime)
        {
            EnemyPool.Get();
            m_Elapsed = 0f;
        }
    }

    private void OnGUI()
    {
        // DEBUG
        GUILayout.Label($"Active Enemies: {ActiveEnemies}");
    }
}
