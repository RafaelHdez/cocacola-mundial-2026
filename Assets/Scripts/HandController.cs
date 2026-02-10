using UnityEngine;
using System.Collections;

public class HandController : MonoBehaviour
{
    [Header("Referencias a las Manos")]
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;

    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 15f; // Qué tan rápido va la mano
    [SerializeField] private float returnSpeed = 10f; // Qué tan rápido vuelve
    [SerializeField] private float punchDepth = 2f; // Qué tanto se aleja de la cámara (hacia el balón)

    // Guardamos las posiciones originales para saber a dónde volver
    private Vector3 leftHandIdlePos;
    private Vector3 rightHandIdlePos;

    // Para controlar que no se solapen corrutinas
    private Coroutine leftHandRoutine;
    private Coroutine rightHandRoutine;

    void Start()
    {
        // Guardar posiciones iniciales
        if (leftHand) leftHandIdlePos = leftHand.position;
        if (rightHand) rightHandIdlePos = rightHand.position;
    }

    // Esta función la llamará el GameManager
    public void MoveHandToPosition(Vector3 targetWorldPosition)
    {
        // 1. Convertir la posición del mundo a posición de pantalla para saber si es Izq o Der
        Vector3 screenPos = Camera.main.WorldToScreenPoint(targetWorldPosition);

        // 2. Decidir qué mano usar
        if (screenPos.x < Screen.width / 2) // Mitad Izquierda
        {
            if (leftHandRoutine != null) StopCoroutine(leftHandRoutine);
            leftHandRoutine = StartCoroutine(PunchRoutine(leftHand, leftHandIdlePos, targetWorldPosition));
        }
        else // Mitad Derecha
        {
            if (rightHandRoutine != null) StopCoroutine(rightHandRoutine);
            rightHandRoutine = StartCoroutine(PunchRoutine(rightHand, rightHandIdlePos, targetWorldPosition));
        }
    }

    private IEnumerator PunchRoutine(Transform hand, Vector3 idlePos, Vector3 targetPos)
    {
        // Ajustamos un poco la posición objetivo para que la mano no atraviese el balón, sino que llegue "a él"
        // Mantenemos la profundidad (Z) controlada o usamos la del balón.
        // Aquí forzamos que la mano vaya un poco hacia el fondo.
        Vector3 actualTarget = targetPos;
        
        // Opcional: Ajustar rotación para mirar al balón (LookAt)
        // hand.LookAt(actualTarget); 

        // --- FASE 1: IR HACIA EL BALÓN ---
        while (Vector3.Distance(hand.position, actualTarget) > 0.1f)
        {
            hand.position = Vector3.Lerp(hand.position, actualTarget, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // --- FASE 2: VOLVER A POSICIÓN DE REPOSO (IDLE) ---
        while (Vector3.Distance(hand.position, idlePos) > 0.1f)
        {
            hand.position = Vector3.Lerp(hand.position, idlePos, returnSpeed * Time.deltaTime);
            
            // Opcional: Suavizar rotación de vuelta si usaste LookAt
            // hand.rotation = Quaternion.Lerp(hand.rotation, Quaternion.identity, returnSpeed * Time.deltaTime);
            
            yield return null;
        }

        // Asegurar posición exacta al final
        hand.position = idlePos;
    }
}