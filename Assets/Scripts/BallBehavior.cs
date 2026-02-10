using UnityEngine;

public class BallBehavior : MonoBehaviour
{
    [Header("Configuración de movimiento")]
    [SerializeField] private float minSpeed = 5f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float rotationSpeed = 360f;

    private Vector3 _targetPosition;
    private float _currentSpeed;
    private bool _hasHitPlayer = false;
    
    void Start()
    {
        // El target del balón es la cámara
        _targetPosition = Camera.main.transform.position;
        
        // Asignamos la velocidad aleatoria para el balón
        _currentSpeed = Random.Range(minSpeed, maxSpeed);
        
        // Rotación aleatoria inicial
        transform.rotation = Random.rotation;
    }
    
    void Update()
    {
        MoveBall();
        RotateBall();
        CheckMiss();
    }

    private void MoveBall()
    {
        float step = _currentSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, step);
    }

    private void RotateBall()
    {
        transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
    }

    private void CheckMiss()
    {
        if (_hasHitPlayer)
        {
            return;
        }
        
        if (Vector3.Distance(transform.position, _targetPosition) < 0.5f)
        {
            _hasHitPlayer = true;

            if (GameManager.Instance)
            {
                GameManager.Instance.RegisterMiss();
            }
            
            Debug.Log("Te anotaron un gol!");
            Destroy(gameObject);
        }
    }
}
