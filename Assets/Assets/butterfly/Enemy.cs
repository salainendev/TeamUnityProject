using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Перетащите сюда родительский объект, который нужно уничтожить")]
    public GameObject parentToDestroy;
    
    [Tooltip("AudioSource для воспроизведения звука (можно перетащить компонент)")]
    public AudioSource audioSource;  // <-- НОВОЕ ПОЛЕ! Ссылка на AudioSource
    
    [Header("Settings (опционально)")]
    [Tooltip("Очки за этого врага")]
    public int pointsValue = 1;
    
    [Tooltip("Задержка перед удалением объекта (сек)")]
    public float destroyDelay = 0.2f;
    
    [Header("Auto Effects (можно переопределить)")]
    [Tooltip("Если оставить пустым — создаст стандартные частицы")]
    public ParticleSystem customParticleEffect;
    
    [Tooltip("Если оставить пустым — проиграет стандартный писк")]
    public AudioClip customSound;
    
    [Tooltip("Громкость звука")]
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;
    
    private MenuController menuController;
    private bool isCaught = false;
    
    void Start()
    {
        // Находим MenuController
        menuController = FindObjectOfType<MenuController>();
        
        // Если parentToDestroy не назначен — используем текущий объект
        if (parentToDestroy == null)
        {
            parentToDestroy = gameObject;
            Debug.LogWarning($"parentToDestroy не назначен для {gameObject.name}, буду уничтожать сам себя");
        }
        
        // Если AudioSource не назначен — пытаемся найти на объекте
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                // Если нет AudioSource — создаём временный для звуков
                Debug.Log($"AudioSource не назначен для {gameObject.name}, буду использовать временный");
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Проверяем, что коснулся игрок и враг ещё не пойман
        if (other.CompareTag("Player") && !isCaught)
        {
            CatchEnemy();
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isCaught)
        {
            CatchEnemy();
        }
    }
    
    void CatchEnemy()
    {
        if (isCaught) return;
        isCaught = true;
        
        Debug.Log($"Враг {gameObject.name} пойман!");
        
        // 1. Увеличиваем счётчик
        if (menuController != null)
        {
            menuController.AddCaughtEnemy();
            Debug.Log($"Очки: +{pointsValue}");
        }
        
        // 2. Создаём партиклы
        SpawnParticleEffect();
        
        // 3. Проигрываем звук
        PlayCatchSound();
        
        // 4. Уничтожаем родительский объект
        if (parentToDestroy != null)
        {
            // Отключаем рендеры, чтобы объект сразу исчез
            DisableRenderer(parentToDestroy);
            // Удаляем через задержку
            Destroy(parentToDestroy, destroyDelay);
            Debug.Log($"Уничтожаем объект: {parentToDestroy.name}");
        }
        else
        {
            // Если объект не назначен — уничтожаем текущий
            DisableRenderer(gameObject);
            Destroy(gameObject, destroyDelay);
        }
        
        // Отключаем коллайдер на этом объекте, чтобы не было повторных срабатываний
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
    
    // Отключает все рендеры в объекте и его детях
    void DisableRenderer(GameObject obj)
    {
        if (obj == null) return;
        
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            if (rend != null)
                rend.enabled = false;
        }
    }
    
    // Создание партиклов (автоматически)
    void SpawnParticleEffect()
    {
        ParticleSystem particles = null;
        
        // Если назначены свои партиклы — используем их
        if (customParticleEffect != null)
        {
            particles = Instantiate(customParticleEffect, transform.position, Quaternion.identity);
        }
        else
        {
            // Создаём стандартные милые партиклы
            particles = CreateDefaultParticles();
        }
        
        if (particles != null)
        {
            particles.transform.position = transform.position;
            particles.Play();
            
            // Уничтожаем через время жизни частиц
            float duration = particles.main.duration;
            Destroy(particles.gameObject, duration + 0.5f);
        }
    }
    
    // Создание стандартных частиц (розовые)
    ParticleSystem CreateDefaultParticles()
    {
        GameObject particleObject = new GameObject("DefaultCatchParticles");
        particleObject.transform.position = transform.position;
        
        ParticleSystem ps = particleObject.AddComponent<ParticleSystem>();
        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        
        // Настройка визуала
        var main = ps.main;
        main.startLifetime = 0.6f;
        main.startSpeed = 2f;
        main.startSize = 0.2f;
        main.maxParticles = 15;
        main.startColor = new Color(1f, 0.6f, 0.8f, 1f); // Розовый
        
        // Форма частиц (сфера)
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        // Плавное затухание
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(1f, 0.5f, 0.7f), 0.5f),
                new GradientColorKey(new Color(1f, 0.8f, 0.9f), 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;
        
        // Материал для частиц
        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material = mat;
        
        return ps;
    }
    
    // Воспроизведение звука
    void PlayCatchSound()
    {
        // Определяем, какой звук играть
        AudioClip clipToPlay = customSound != null ? customSound : CreateDefaultBeep();
        
        // Если есть AudioSource — используем его
        if (audioSource != null)
        {
            audioSource.PlayOneShot(clipToPlay, soundVolume);
            Debug.Log("Звук через AudioSource");
        }
        else
        {
            // Если нет AudioSource — создаём временный
            AudioSource.PlayClipAtPoint(clipToPlay, transform.position, soundVolume);
            Debug.Log("Звук через PlayClipAtPoint");
        }
    }
    
    // Создание стандартного звука
    AudioClip CreateDefaultBeep()
    {
        int sampleRate = 44100;
        float duration = 0.15f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        
        // Две частоты для "милого" звука (восходящий тон)
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float freq1 = 880f; // Ля
            float freq2 = 1046.5f; // До
            float value = 0f;
            
            if (t < duration / 2)
            {
                value = Mathf.Sin(2 * Mathf.PI * freq1 * t);
            }
            else
            {
                value = Mathf.Sin(2 * Mathf.PI * freq2 * t);
            }
            
            // Затухание
            value *= (1f - t / duration);
            samples[i] = value * 0.5f;
        }
        
        AudioClip clip = AudioClip.Create("DefaultBeep", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}