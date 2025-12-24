using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Настройки движения")]
    public float baseSpeed = 10f;

    public float acceleration = 30f;
    public float maxSpeed = 300f;
    public float sensitivity = 2f;

    [Header("Настройки высоты")]
    public float altitudeSpeed = 5f;

    Vector3 currentVelocity;
    float currentForwardSpeed;
    float currentStrafeSpeed;
    float currentVerticalSpeed;
    float rotationX;
    float rotationY;

    void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(1)) // Правый клик для вращения
        {
            rotationX += Input.GetAxis("Mouse X") * sensitivity;
            rotationY -= Input.GetAxis("Mouse Y") * sensitivity;
            rotationY = Mathf.Clamp(rotationY, -90f, 90f);

            transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);
        }
    }

    void HandleMovement()
    {
        // Базовое управление (WASD)
        float targetForward = 0f;
        float targetStrafe = 0f;
        float targetVertical = 0f;

        if (Input.GetKey(KeyCode.W))
            targetForward = 1f;

        if (Input.GetKey(KeyCode.S))
            targetForward = -1f;

        if (Input.GetKey(KeyCode.D))
            targetStrafe = 1f;

        if (Input.GetKey(KeyCode.A))
            targetStrafe = -1f;

        // Управление высотой (Space/Ctrl или Q/E)
        if (Input.GetKey(KeyCode.Space)
         || Input.GetKey(KeyCode.E))
            targetVertical = 1f;

        if (Input.GetKey(KeyCode.LeftControl)
         || Input.GetKey(KeyCode.Q))
            targetVertical = -1f;

        // Применение ускорения
        float accelerationMultiplier = Input.GetKey(KeyCode.LeftShift)
            ? 2f
            : 1f;

        float currentAcceleration = acceleration * accelerationMultiplier;

        currentForwardSpeed = Mathf.MoveTowards(
            currentForwardSpeed,
            targetForward * maxSpeed,
            currentAcceleration * Time.deltaTime
        );

        currentStrafeSpeed = Mathf.MoveTowards(
            currentStrafeSpeed,
            targetStrafe * maxSpeed,
            currentAcceleration * Time.deltaTime
        );

        currentVerticalSpeed = Mathf.MoveTowards(
            currentVerticalSpeed,
            targetVertical * maxSpeed,
            currentAcceleration * Time.deltaTime
        );

        // Вычисление вектора движения
        Vector3 moveDirection = new Vector3(currentStrafeSpeed, currentVerticalSpeed, currentForwardSpeed);
        moveDirection = transform.TransformDirection(moveDirection);

        // Применение движения
        transform.position += moveDirection * Time.deltaTime;

        // Быстрый сброс скорости при отпускании клавиш
        if (Mathf.Approximately(targetForward, 0f))
            currentForwardSpeed = 0f;

        if (Mathf.Approximately(targetStrafe, 0f))
            currentStrafeSpeed = 0f;

        if (Mathf.Approximately(targetVertical, 0f))
            currentVerticalSpeed = 0f;
    }
}
