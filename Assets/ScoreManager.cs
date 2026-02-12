using UnityEngine;
using TMPro;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
    public class ScoreManager : MonoBehaviour
    {
        [Header("Display")]
        public TMP_Text scoreText;

        [Header("Statistics")]
        public int currentScore = 0;

        private static ScoreManager _instance;
        public static ScoreManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<ScoreManager>();
                return _instance;
            }
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            // Автопоиск текста если не назначен
            if (scoreText == null)
            {
                GameObject scoreObj = GameObject.Find("ScoreText");
                if (scoreObj != null) 
                    scoreText = scoreObj.GetComponent<TMP_Text>();
            }

            UpdateDisplay();
        }

        // Добавить очки
        public void AddScore(int points)
        {
            currentScore += points;
            UpdateDisplay();
        }

        // Сбросить счёт
        public void ResetScore()
        {
            currentScore = 0;
            UpdateDisplay();
        }

        // Обновить UI
        void UpdateDisplay()
        {
            if (scoreText != null)
                scoreText.text = $"Счет: {currentScore}";
        }

        // Геттер для получения текущего счёта
        public int GetCurrentScore() => currentScore;
    }
}