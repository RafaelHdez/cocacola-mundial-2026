using UnityEngine;
using System.Collections;

public class BallSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameManager gameManager; // Arrastra aquí tu GameManager
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(10f, 6f);

    [Header("Configuración de Waves")]
    [Tooltip("Número mínimo de balones por oleada")]
    [SerializeField] private int minBallsPerWave = 1;
    [Tooltip("Número máximo de balones por oleada")]
    [SerializeField] private int maxBallsPerWave = 3;
    [SerializeField] private float delayBetweenBalls = 0.15f;
    
    [Header("Dificultad Dinámica")]
    [Tooltip("Tiempo de spawn de balones inicial")]
    [SerializeField] private float startWaveDelay = 2.0f;
    
    [Tooltip("Tiempo de spawn de balones final")]
    [SerializeField] private float endWaveDelay = 0.4f;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            if (gameManager && gameManager.IsGameActive)
            {
                // Calculamos cuántos balones salen en esta oleada
                int ballsThisWave = Random.Range(minBallsPerWave, maxBallsPerWave + 1);
                
                // Lanzamos esa cantidad de balones
                for (int i = 0; i < ballsThisWave; i++)
                {
                    SpawnBall();
                    yield return new WaitForSeconds(delayBetweenBalls);
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    private void SpawnBall()
    {
        if (!ballPrefab) return;

        Vector3 randomPos = new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            0
        );

        Vector3 spawnPosition = transform.TransformPoint(randomPos);
        Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 1));
    }
}