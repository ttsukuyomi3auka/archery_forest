using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Valve.VR.InteractionSystem
{
    public class ScoreManager : MonoBehaviour
    {
        [System.Serializable]
        public class HitData
        {
            public int points;
            public string zone;
            public float accuracy;
            public Vector3 position;
        }

        [Header("Display Settings")]
        public TMP_Text scoreText;
        public TMP_Text accuracyText;
        public TMP_Text zoneText;

        [Header("Statistics")]
        public int currentScore = 0;
        public int totalHits = 0;
        public float averageAccuracy = 0f;

        private List<HitData> hitHistory = new List<HitData>();

        private static ScoreManager _instance;
        public static ScoreManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ScoreManager>();
                }
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

            // Инициализация UI
            InitializeUI();
        }

        void InitializeUI()
        {
            // Если тексты не назначены, пытаемся найти
            if (scoreText == null)
            {
                GameObject scoreObj = GameObject.Find("ScoreText");
                if (scoreObj != null) scoreText = scoreObj.GetComponent<TMP_Text>();
            }

            UpdateAllDisplays();
        }

        // Основной метод добавления очков
        public void AddScore(int points, string zone = "Target", float accuracy = 100f, Vector3 hitPosition = default)
        {
            currentScore += points;
            totalHits++;

            // Сохраняем данные
            HitData hit = new HitData
            {
                points = points,
                zone = zone,
                accuracy = accuracy,
                position = hitPosition
            };
            hitHistory.Add(hit);

            // Обновляем статистику
            UpdateStatistics();

            // Обновляем UI
            UpdateAllDisplays();

            // Эффект
            if (points > 0)
            {
                StartCoroutine(ScoreEffect(points, zone));
            }

            Debug.Log($"Добавлено {points} очков. Зона: {zone}, Точность: {accuracy:F1}%");
        }

        void UpdateStatistics()
        {
            // Средняя точность
            float totalAccuracy = 0f;

            foreach (var hit in hitHistory)
            {
                totalAccuracy += hit.accuracy;
            }

            averageAccuracy = hitHistory.Count > 0 ? totalAccuracy / hitHistory.Count : 0f;
        }

        void UpdateAllDisplays()
        {
            if (scoreText != null)
                scoreText.text = $"Счет: {currentScore}";

            if (accuracyText != null)
                accuracyText.text = $"Accuracy: {averageAccuracy:F1}%";

            if (zoneText != null && hitHistory.Count > 0)
            {
                HitData lastHit = hitHistory[hitHistory.Count - 1];
                zoneText.text = $"Zone: {lastHit.zone}";
            }
        }

        IEnumerator ScoreEffect(int points, string zone)
        {
            if (scoreText == null) yield break;

            Color originalColor = scoreText.color;
            Vector3 originalScale = scoreText.transform.localScale;

            // Цвет в зависимости от зоны
            Color zoneColor = GetZoneColor(zone);
            scoreText.color = zoneColor;
            scoreText.transform.localScale = originalScale * 1.3f;

            // Временно показываем полученные очки
            string originalText = scoreText.text;
            scoreText.text = $"+{points}!\n{originalText}";

            yield return new WaitForSeconds(0.3f);

            // Возвращаем
            scoreText.color = originalColor;
            scoreText.transform.localScale = originalScale;
            scoreText.text = originalText;
        }

        Color GetZoneColor(string zone)
        {
            if (zone.Contains("Bullseye")) return Color.red;
            if (zone.Contains("Inner")) return Color.yellow;
            if (zone.Contains("Middle")) return Color.green;
            if (zone.Contains("Outer")) return Color.blue;
            return Color.white;
        }

        public void ResetScore()
        {
            currentScore = 0;
            totalHits = 0;
            averageAccuracy = 0f;
            hitHistory.Clear();

            UpdateAllDisplays();
        }

        // Геттеры для статистики
        public int GetCurrentScore() => currentScore;
        public float GetAverageAccuracy() => averageAccuracy;
        public int GetTotalHits() => totalHits;

        // Полная статистика
        public string GetFullStats()
        {
            return $"Cчет: {currentScore}\n" +
                   $"Hits: {totalHits}\n" +
                   $"Accuracy: {averageAccuracy:F1}%";
        }
    }
}