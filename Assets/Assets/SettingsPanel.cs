using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("AudioSource фоновой музыки")]
    public AudioSource musicAudioSource;
    
    [Tooltip("Ползунок громкости")]
    public Slider musicVolumeSlider;
    
    [Tooltip("Текст для отображения процентов громкости")]
    public TMP_Text musicVolumeValueText;
    
    [Header("Day/Night Settings")]
    [Tooltip("Галочка для смены дня и ночи")]
    public Toggle dayNightToggle;
    
    [Tooltip("Текст для галочки (опционально)")]
    public TMP_Text dayNightToggleText;
    
    [Header("Lighting")]
    [Tooltip("Главный источник света (Directional Light)")]
    public Light mainLight;
    
    [Tooltip("Цвет освещения для дня")]
    public Color dayColor = Color.white;
    
    [Tooltip("Цвет освещения для ночи")]
    public Color nightColor = new Color(0.2f, 0.2f, 0.5f);
    
    [Tooltip("Интенсивность освещения для дня")]
    public float dayIntensity = 1f;
    
    [Tooltip("Интенсивность освещения для ночи")]
    public float nightIntensity = 0.3f;
    
    [Tooltip("Цвет ambient (окружающего) освещения для дня")]
    public Color dayAmbientColor = new Color(0.5f, 0.5f, 0.6f);
    
    [Tooltip("Цвет ambient освещения для ночи")]
    public Color nightAmbientColor = new Color(0.1f, 0.1f, 0.2f);
    
    [Header("Optional Effects")]
    [Tooltip("Небо (Skybox) для смены (опционально)")]
    public Material daySkybox;
    public Material nightSkybox;
    
    private bool isNight = false;
    
    void Start()
    {
        LoadSettings();
        SetupUI();
    }
    
    void SetupUI()
    {
        // Настройка ползунка громкости
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.value = GetSavedVolume();
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            UpdateMusicVolumeText(musicVolumeSlider.value);
        }
        
        // Настройка галочки дня/ночи
        if (dayNightToggle != null)
        {
            dayNightToggle.isOn = GetSavedDayNightState();
            dayNightToggle.onValueChanged.AddListener(OnDayNightToggled);
            OnDayNightToggled(dayNightToggle.isOn);
        }
    }
    
    void LoadSettings()
    {
        // Загружаем сохранённые настройки
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        bool savedDayNight = PlayerPrefs.GetInt("IsNight", 0) == 1;
        
        isNight = savedDayNight;
        
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = savedVolume;
        }
    }
    
    float GetSavedVolume()
    {
        return PlayerPrefs.GetFloat("MusicVolume", 0.7f);
    }
    
    bool GetSavedDayNightState()
    {
        return PlayerPrefs.GetInt("IsNight", 0) == 1;
    }
    
    void OnMusicVolumeChanged(float value)
    {
        // Меняем громкость AudioSource
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = value;
        }
        
        // Обновляем текст
        UpdateMusicVolumeText(value);
        
        // Сохраняем настройку
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }
    
    void UpdateMusicVolumeText(float value)
    {
        if (musicVolumeValueText != null)
        {
            int percent = Mathf.RoundToInt(value * 100f);
            musicVolumeValueText.text = $"{percent}%";
        }
    }
    
    void OnDayNightToggled(bool isOn)
    {
        isNight = isOn;
        
        // Меняем освещение
        ApplyDayNightSettings(isOn);
        
        // Сохраняем настройку
        PlayerPrefs.SetInt("IsNight", isOn ? 1 : 0);
        PlayerPrefs.Save();
        
        Debug.Log(isOn ? "🌙 Включена ночь" : "☀️ Включён день");
    }
    
    void ApplyDayNightSettings(bool night)
    {
        // Меняем основной источник света
        if (mainLight != null)
        {
            if (night)
            {
                mainLight.color = nightColor;
                mainLight.intensity = nightIntensity;
            }
            else
            {
                mainLight.color = dayColor;
                mainLight.intensity = dayIntensity;
            }
        }
        
        // Меняем окружающее освещение
        if (night)
        {
            RenderSettings.ambientLight = nightAmbientColor;
        }
        else
        {
            RenderSettings.ambientLight = dayAmbientColor;
        }
        
        // Меняем Skybox (опционально)
        if (night && nightSkybox != null)
        {
            RenderSettings.skybox = nightSkybox;
        }
        else if (!night && daySkybox != null)
        {
            RenderSettings.skybox = daySkybox;
        }
        
        // Принудительно обновляем отображение
        DynamicGI.UpdateEnvironment();
    }
    
    // Публичные методы для вызова из других скриптов
    
    public void SetMusicVolume(float volume)
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = volume;
        }
    }
    
    public float GetMusicVolume()
    {
        return musicAudioSource != null ? musicAudioSource.volume : 0.7f;
    }
    
    public bool IsNightTime()
    {
        return isNight;
    }
    
    public void ToggleDayNight()
    {
        if (dayNightToggle != null)
        {
            dayNightToggle.isOn = !dayNightToggle.isOn;
        }
    }
    
    // Методы для кнопок (опционально)
    public void SetDay()
    {
        if (dayNightToggle != null) dayNightToggle.isOn = false;
    }
    
    public void SetNight()
    {
        if (dayNightToggle != null) dayNightToggle.isOn = true;
    }
}