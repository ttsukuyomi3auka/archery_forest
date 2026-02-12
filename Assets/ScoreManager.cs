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
            StartCoroutine(ScoreEffect(points));
            
            Debug.Log($"Добавлено {points} очков. Всего: {currentScore}");
        }

        // Сбросить счёт
        public void ResetScore()
        {
            currentScore = 0;
            UpdateDisplay();
            Debug.Log("Счёт сброшен");
        }

        // Обновить UI
        void UpdateDisplay()
        {
            if (scoreText != null)
                scoreText.text = $"Счет: {currentScore}";
        }

        // Визуальный эффект при добавлении очков
        IEnumerator ScoreEffect(int points)
        {
            if (scoreText == null) yield break;

            Color originalColor = scoreText.color;
            Vector3 originalScale = scoreText.transform.localScale;

            scoreText.color = Color.green;
            scoreText.transform.localScale = originalScale * 1.2f;

            string originalText = scoreText.text;
            scoreText.text = $"+{points}!\n{originalText}";

            yield return new WaitForSeconds(0.2f);

            scoreText.color = originalColor;
            scoreText.transform.localScale = originalScale;
            scoreText.text = originalText;
        }

        // Геттер для получения текущего счёта
        public int GetCurrentScore() => currentScore;
    }
}