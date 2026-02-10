using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    public class MyTargetScript : MonoBehaviour
    {
        [System.Serializable]
        public class ScoreZone
        {
            public string zoneName = "Zone";
            public float radius = 0.5f; // Радиус зоны от центра в метрах
            public int scoreValue = 10;
            public Color gizmoColor = Color.white;
        }

        [Header("Target Settings")]
        public UnityEvent onTakeDamage;
        public Transform targetCenter; // Центр мишени
        public float targetRadius = 0.31f; // Исправлено: должен быть равен максимальному радиусу зон

        [Header("Score Zones")]
        public ScoreZone[] scoreZones;

        [Header("Sound")]
        public AudioClip hitSound;

        private AudioSource audioSource;

        void Awake()
        {
            // Инициализируем зоны если массив пустой или null
            InitializeScoreZones();

            // Если центр не назначен, используем центр объекта
            if (targetCenter == null)
            {
                CreateTargetCenter();
            }
        }

        void Start()
        {
            // Сортируем зоны по радиусу (от меньшего к большему) чтобы логика проверки работала корректно
            if (scoreZones != null)
            {
                System.Array.Sort(scoreZones, (a, b) => a.radius.CompareTo(b.radius));
            }

            // Аудио
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
            audioSource.playOnAwake = false;

            // Автоматически определяем радиус если не задан или меньше максимального
            UpdateTargetRadius();
        }
        void InitializeScoreZones()
        {
            // Если зоны еще не инициализированы, создаем стандартные
            if (scoreZones == null || scoreZones.Length == 0)
            {
                scoreZones = new ScoreZone[]
                {
                    new ScoreZone { zoneName = "100", radius = 0.04f, scoreValue = 100 },
                    new ScoreZone { zoneName = "80", radius = 0.08f, scoreValue = 80},
                    new ScoreZone { zoneName = "60", radius = 0.12f, scoreValue = 60},
                    new ScoreZone { zoneName = "50", radius = 0.16f, scoreValue = 50 },
                    new ScoreZone { zoneName = "40", radius = 0.20f, scoreValue = 40},
                    new ScoreZone { zoneName = "30", radius = 0.24f, scoreValue = 30},
                    new ScoreZone { zoneName = "20", radius = 0.28f, scoreValue = 20},
                    new ScoreZone { zoneName = "10", radius = 0.30f, scoreValue = 10},
                    new ScoreZone { zoneName = "Outer Ring", radius = 0.31f, scoreValue = 0}
                };
            }
        }

        void UpdateTargetRadius()
        {
            // Если targetRadius не задан или меньше максимального радиуса зон, обновляем
            float maxZoneRadius = 0f;
            foreach (var zone in scoreZones)
            {
                if (zone.radius > maxZoneRadius)
                {
                    maxZoneRadius = zone.radius;
                }
            }

            if (targetRadius < maxZoneRadius || targetRadius <= 0)
            {
                targetRadius = maxZoneRadius;
            }
        }

        void CreateTargetCenter()
        {
            GameObject centerObj = new GameObject("TargetCenter");
            centerObj.transform.SetParent(transform);
            centerObj.transform.localPosition = Vector3.zero;
            targetCenter = centerObj.transform;
        }

        // Этот метод вызывается стрелой при попадании.
        // Теперь он требует передачи точки попадания.
        public void ApplyDamage(Vector3 hitPoint)
        {
            OnHit(hitPoint);
        }

        void OnHit(Vector3 hitPoint)
        {
            // Рассчитываем расстояние до центра
            float distance = CalculateDistanceToCenter(hitPoint);

            // Определяем зону попадания
            int score = 0;
            string zoneName = "Miss";

            // Проверяем зоны от самой маленькой к большой
            for (int i = 0; i < scoreZones.Length; i++)
            {
                if (distance <= scoreZones[i].radius)
                {
                    score = scoreZones[i].scoreValue;
                    zoneName = scoreZones[i].zoneName;
                    break;
                }
            }
            PlayHitEffects();

            // Вызываем событие
            onTakeDamage.Invoke();

            // Добавляем очки
            ScoreManager scoreManager = ScoreManager.Instance;
            if (scoreManager != null)
            {
                float accuracy = CalculateAccuracy(distance);
                scoreManager.AddScore(score, zoneName, accuracy);
            }
            else
            {
                Debug.LogError("ScoreManager.Instance равен null!");
            }
        }

        void PlayHitEffects()
        {
            if (hitSound != null)
            {
                audioSource.clip = hitSound;
                audioSource.Play();
            }
        }

        float CalculateDistanceToCenter(Vector3 hitPoint)
        {
            float distance3D = Vector3.Distance(hitPoint, targetCenter.position);
            return distance3D;
        }

        float CalculateAccuracy(float distance)
        {
            float accuracy = (1f - (distance / targetRadius)) * 100f;
            return Mathf.Clamp(accuracy, 0f, 100f);
        }

        // Визуализация в редакторе
        void OnDrawGizmosSelected()
        {
            // Если зоны не инициализированы в редакторе, инициализируем
            if (scoreZones == null || scoreZones.Length == 0)
            {
                InitializeScoreZones();
            }

            if (targetCenter == null) return;

            Vector3 center = targetCenter.position;

            // Рисуем общий радиус
            Gizmos.color = Color.white;

            Gizmos.DrawWireSphere(center, targetRadius);

            // Рисуем зоны
            foreach (var zone in scoreZones)
            {
                Gizmos.color = zone.gizmoColor;
                Gizmos.DrawWireSphere(center, zone.radius);

            }

            // Центр
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(center, 0.01f);
        }

    }
}
