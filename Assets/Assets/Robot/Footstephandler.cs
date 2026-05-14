using UnityEngine;
using StarterAssets;

public class FootstepHandler : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] footstepSounds;
    
    [Header("Settings")]
    [Tooltip("Минимальный интервал между шагами (сек)")]
    public float minInterval = 0.3f;
    
    [Tooltip("Максимальный интервал между шагами (сек)")]
    public float maxInterval = 0.6f;
    
    [Tooltip("Минимальная горизонтальная скорость для воспроизведения шагов")]
    public float minSpeed = 0.1f;
    
    private ThirdPersonController _controller;
    private CharacterController _charController;
    private float _nextFootstepTime;
    private bool _wasMoving = false;
    
    void Start()
    {
        _controller = GetComponent<ThirdPersonController>();
        _charController = GetComponent<CharacterController>();
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        _nextFootstepTime = 0f;
        
        // Проверка при старте
        Debug.Log("=== FootstepHandler инициализирован ===");
        Debug.Log($"AudioSource назначен: {(audioSource != null)}");
        Debug.Log($"Звуков в массиве: {(footstepSounds != null ? footstepSounds.Length : 0)}");
        Debug.Log($"ThirdPersonController найден: {(_controller != null)}");
        Debug.Log($"CharacterController найден: {(_charController != null)}");
    }
    
    void Update()
    {
        // Проверяем, на земле ли персонаж
        bool isGrounded = _controller.Grounded;
        
        // Получаем горизонтальную скорость (игнорируем вертикальную составляющую)
        Vector3 horizontalVelocity = new Vector3(_charController.velocity.x, 0, _charController.velocity.z);
        float currentHorizontalSpeed = horizontalVelocity.magnitude;
        
        // Условие для шагов: на земле и горизонтальная скорость выше минимальной
        bool isMoving =  isGrounded & currentHorizontalSpeed > minSpeed;
        
        // Дебаг-логи (выводятся каждые 30 кадров, чтобы не заспамливать)
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[Отладка] На земле: {isGrounded} | Скорость: {currentHorizontalSpeed:F2} | Движется: {isMoving} | Таймер: {(_nextFootstepTime - Time.time):F2}");
        }
        
        // Если состояние движения изменилось
        if (isMoving != _wasMoving)
        {
            if (isMoving)
                Debug.Log("▶ ПЕРСОНАЖ НАЧАЛ ДВИГАТЬСЯ");
            else
                Debug.Log("⏹ ПЕРСОНАЖ ОСТАНОВИЛСЯ");
        }
        
        if (isMoving)
        {
            // Вычисляем интервал в зависимости от скорости
            float speedNormalized = Mathf.Clamp01(currentHorizontalSpeed / 8f);
            float currentInterval = Mathf.Lerp(maxInterval, minInterval, speedNormalized);
            
            // Воспроизводим звук с заданным интервалом
            if (Time.time >= _nextFootstepTime)
            {
                PlayRandomFootstep();
                _nextFootstepTime = Time.time + currentInterval;
                Debug.Log($"👣 ШАГ! Интервал: {currentInterval:F2}сек | Скорость: {currentHorizontalSpeed:F2}");
            }
        }
        
        _wasMoving = isMoving;
    }
    
    void PlayRandomFootstep()
    {
        if (audioSource == null)
        {
            Debug.LogError("❌ AudioSource не назначен!");
            return;
        }
        
        if (footstepSounds == null || footstepSounds.Length == 0)
        {
            Debug.LogError("❌ Массив звуков пуст или не назначен!");
            return;
        }
        
        AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
        audioSource.PlayOneShot(clip);
        Debug.Log($"🔊 Воспроизведён звук: {clip.name}");
    }
}