using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Cinemachine;
using TMPro;

public class MenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject menuPanel;
    public GameObject aboutPanel;
    public GameObject background;
    public GameObject winPanel;
    public GameObject settingsPanel;

    [Header("In-Game UI")]
    public GameObject inGameUI;
    public TMP_Text timerText;
    public TMP_Text enemiesText;
    
    [Header("Win Screen")]
    public TMP_Text winTimeText;      // Время на экране победы
    public TMP_Text winEnemiesText;   // Счёт на экране победы
    
    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip winSound;         // Звук победы (опционально)

    [Header("Player References")]
    public GameObject playerArmature;
    public StarterAssets.StarterAssetsInputs starterAssetsInputs;

    [Header("Camera References")]
    public CinemachineBrain cinemachineBrain;
    public GameObject playerFollowCamera;

    [Header("Game Settings")]
    public int totalEnemies = 10;
    
    private bool isGameStarted = false;
    private bool isMenuOpen = true;
    private float gameTime = 0f;
    private int caughtEnemies = 0;
    private bool isGameActive = false;
    private bool isGameWon = false;   // Флаг победы
    
    void Start()
    {
        Debug.Log("=== START: Начало инициализации ===");
        
        // Настройка меню
        if (menuPanel != null) menuPanel.SetActive(true);
        if (aboutPanel != null) aboutPanel.SetActive(false);
        if (background != null) background.SetActive(true);
        if (winPanel != null) winPanel.SetActive(false);  // Панель победы скрыта
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (inGameUI != null) inGameUI.SetActive(false);
        
        isMenuOpen = true;
        Time.timeScale = 0f;
        
        if (musicSource != null) musicSource.Pause();
        
        SetPlayerInputEnabled(false);
        SetCursorLock(false);
        
        if (cinemachineBrain != null) cinemachineBrain.enabled = false;
        if (playerFollowCamera != null) playerFollowCamera.SetActive(false);
        
        // ДЕБАГ: проверка ссылок
        if (timerText == null) Debug.LogError("❌ timerText НЕ НАЗНАЧЕН!");
        if (enemiesText == null) Debug.LogError("❌ enemiesText НЕ НАЗНАЧЕН!");
        if (winPanel == null) Debug.LogWarning("⚠️ winPanel не назначен! Экран победы не будет работать.");
        
        UpdateEnemiesText();
        UpdateTimerText();
        
        Debug.Log("=== START: Инициализация завершена ===");
    }
    
    void Update()
    {
        // Обработка Escape (НЕ РАБОТАЕТ если игра закончена)
        if (!isGameWon && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!isGameStarted) return;
            
            if (aboutPanel != null && aboutPanel.activeSelf)
            {
                BackToMenu();
                return;
            }
            
            if (isMenuOpen)
                ResumeGame();
            else
                OpenPauseMenu();
        }
        
        // Обновление таймера
        if (isGameActive && !isGameWon)
        {
            gameTime += Time.deltaTime;
            UpdateTimerText();
        }
    }
    
    public void PlayGame()
    {
        Debug.Log("=== PLAY GAME ===");
        
        if (!isGameStarted)
        {
            EnableCinemachineForFirstTime();
        }
        
        isGameStarted = true;
        isGameActive = true;
        isGameWon = false;  // Сбрасываем флаг победы
        ResumeGame();
        
        if (musicSource != null)
        {
            if (!musicSource.isPlaying) musicSource.Play();
            else musicSource.UnPause();
        }
        
        if (inGameUI != null)
        {
            inGameUI.SetActive(true);
            Debug.Log("✅ inGameUI включён");
        }
        
        // Сбрасываем счётчики при новом запуске
        gameTime = 0f;
        caughtEnemies = 0;
        UpdateTimerText();
        UpdateEnemiesText();
        
        // Скрываем панель победы если она была открыта
        if (winPanel != null) winPanel.SetActive(false);
    }
    
    public void OpenAbout()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (aboutPanel != null) aboutPanel.SetActive(true);
        if (background != null) background.SetActive(true);
        
        isMenuOpen = true;
        Time.timeScale = 0f;
        if (musicSource != null) musicSource.Pause();
        
        SetPlayerInputEnabled(false);
        SetCursorLock(false);
        if (playerFollowCamera != null) playerFollowCamera.SetActive(false);
        if (inGameUI != null) inGameUI.SetActive(false);
    }
    
    public void BackToMenu()
    {
        if (aboutPanel != null) aboutPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(true);
        if (background != null) background.SetActive(true);
        if (winPanel != null) winPanel.SetActive(false);
        
        isMenuOpen = true;
        Time.timeScale = 0f;
        if (musicSource != null) musicSource.Pause();
        
        SetPlayerInputEnabled(false);
        SetCursorLock(false);
        if (playerFollowCamera != null) playerFollowCamera.SetActive(false);
        if (inGameUI != null) inGameUI.SetActive(false);
    }
    
    public void OpenPauseMenu()
    {
        if (isGameWon) return;  // Нельзя открыть паузу при победе
        
        if (menuPanel != null) menuPanel.SetActive(true);
        if (aboutPanel != null) aboutPanel.SetActive(false);
        if (background != null) background.SetActive(true);
        
        isMenuOpen = true;
        isGameActive = false;
        Time.timeScale = 0f;
        if (musicSource != null) musicSource.Pause();
        
        SetPlayerInputEnabled(false);
        SetCursorLock(false);
        if (playerFollowCamera != null) playerFollowCamera.SetActive(false);
        if (inGameUI != null) inGameUI.SetActive(false);
    }
    
    public void ResumeGame()
    {
        if (isGameWon) return;  // Нельзя продолжить игру при победе
        
        if (menuPanel != null) menuPanel.SetActive(false);
        if (aboutPanel != null) aboutPanel.SetActive(false);
        if (background != null) background.SetActive(false);
        
        isMenuOpen = false;
        isGameActive = true;
        Time.timeScale = 1f;
        
        if (isGameStarted && musicSource != null) musicSource.UnPause();
        
        SetPlayerInputEnabled(true);
        SetCursorLock(true);
        if (playerFollowCamera != null) playerFollowCamera.SetActive(true);
        if (inGameUI != null) inGameUI.SetActive(true);
        
        UpdateTimerText();
        UpdateEnemiesText();
    }
    
    public void ExitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    public void AddCaughtEnemy()
    {
        if (isGameWon) return;  // Если игра уже закончена — не считаем
        
        caughtEnemies++;
        Debug.Log($"🐛 Враг пойман! caughtEnemies = {caughtEnemies}, totalEnemies = {totalEnemies}");
        UpdateEnemiesText();
        
        if (caughtEnemies >= totalEnemies)
        {
            WinGame();
        }
    }
    
    public void SetTotalEnemies(int newTotal)
    {
        totalEnemies = newTotal;
        Debug.Log($"📊 Общее количество врагов обновлено: {totalEnemies}");
        UpdateEnemiesText();
    }
    
    private void UpdateEnemiesText()
    {
        if (enemiesText != null)
        {
            string newText = $"{caughtEnemies} / {totalEnemies}";
            enemiesText.text = newText;
        }
    }
    
    private void UpdateTimerText()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
    
    // ========== СИСТЕМА ПОБЕДЫ ==========
    
    private void WinGame()
    {
        isGameWon = true;
        isGameActive = false;
        Time.timeScale = 0f;
        
        Debug.Log($"🏆 ПОБЕДА! Время: {gameTime:F2} секунд, Поймано: {caughtEnemies}");
        
        // Обновляем тексты на экране победы
        if (winTimeText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            winTimeText.text = $"Время: {minutes:00}:{seconds:00}";
        }
        
        if (winEnemiesText != null)
        {
            winEnemiesText.text = $"Поймано: {caughtEnemies} из {totalEnemies}";
        }
        
        // Показываем панель победы
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Debug.Log("🎉 Панель победы показана!");
        }
        else
        {
            Debug.LogWarning("winPanel не назначен! Создайте панель победы.");
        }
        
        // Проигрываем звук победы
        if (winSound != null && musicSource != null)
        {
            musicSource.PlayOneShot(winSound);
        }
        
        // Прячем внутриигровой UI
        if (inGameUI != null) inGameUI.SetActive(false);
        
        // Отключаем управление персонажем
        SetPlayerInputEnabled(false);
        SetCursorLock(false);
        
        // Отключаем камеру
        if (playerFollowCamera != null) playerFollowCamera.SetActive(false);
    }
    
    // Метод для кнопки "Играть снова" на экране победы
    public void RestartGame()
    {
        Debug.Log("🔄 Перезапуск игры...");
        
        // Сбрасываем всё
        isGameWon = false;
        isGameStarted = false;
        isGameActive = false;
        caughtEnemies = 0;
        gameTime = 0f;
        
        // Прячем панель победы
        if (winPanel != null) winPanel.SetActive(false);
        
        // Запускаем игру заново
        PlayGame();
    }
    
    public float GetCurrentTime() => gameTime;
    public int GetCaughtEnemies() => caughtEnemies;
    
    private void SetPlayerInputEnabled(bool enabled)
    {
        if (playerArmature != null)
        {
            PlayerInput playerInput = playerArmature.GetComponent<PlayerInput>();
            if (playerInput != null) playerInput.enabled = enabled;
            
            var thirdPersonController = playerArmature.GetComponent<StarterAssets.ThirdPersonController>();
            if (thirdPersonController != null) thirdPersonController.enabled = enabled;
        }
        
        if (starterAssetsInputs != null) starterAssetsInputs.enabled = enabled;
    }
    
    private void SetCursorLock(bool locked)
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    private void EnableCinemachineForFirstTime()
    {
        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = true;
            Debug.Log("CinemachineBrain активирован при первом запуске игры");
        }
    }

    // ========== МЕТОДЫ ДЛЯ НАСТРОЕК ==========

public void OpenSettings()
{
    // Скрываем главное меню и панель "Об игре"
    if (menuPanel != null) menuPanel.SetActive(false);
    if (aboutPanel != null) aboutPanel.SetActive(false);
    if (settingsPanel != null) settingsPanel.SetActive(true);
    if (background != null) background.SetActive(true);
    
    isMenuOpen = true;
    Time.timeScale = 0f;
    
    SetPlayerInputEnabled(false);
    SetCursorLock(false);
    if (playerFollowCamera != null) playerFollowCamera.SetActive(false);
    if (inGameUI != null) inGameUI.SetActive(false);
}

public void CloseSettings()
{
    // Возвращаемся в главное меню
    if (menuPanel != null) menuPanel.SetActive(true);
    if (aboutPanel != null) aboutPanel.SetActive(false);
    if (settingsPanel != null) settingsPanel.SetActive(false);
    if (background != null) background.SetActive(true);
    
    isMenuOpen = true;
    Time.timeScale = 0f;
    
    SetPlayerInputEnabled(false);
    SetCursorLock(false);
    if (playerFollowCamera != null) playerFollowCamera.SetActive(false);
    if (inGameUI != null) inGameUI.SetActive(false);
}


}