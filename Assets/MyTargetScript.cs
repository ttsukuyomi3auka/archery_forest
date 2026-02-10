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
        public bool is3DTarget = false; // true - объемная мишень, false - плоская (игнорировать Z)

        [Header("Score Zones")]
        public ScoreZone[] scoreZones;

        [Header("Sound")]
        public AudioClip hitSound;
        public AudioClip missSound;

        [Header("Visual Feedback")]
        public ParticleSystem hitParticle;
        public ParticleSystem missParticle;

        private AudioSource audioSource;
        private Renderer targetRenderer;
        private Material originalMaterial;
        public Material hitMaterial;
        public float highlightDuration = 0.1f;


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
            // Аудио
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
            audioSource.playOnAwake = false;

            // Визуал
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                originalMaterial = targetRenderer.material;
            }
            // Проверка ориентации мишени
            CheckTargetOrientation();

            // Автоматически определяем радиус если не задан или меньше максимального
            UpdateTargetRadius();
        }

        void CheckTargetOrientation()
        {
            if (targetCenter != null)
            {
                Debug.Log($"=== ОРИЕНТАЦИЯ МИШЕНИ ===");
                Debug.Log($"Позиция мишени: {transform.position}");
                Debug.Log($"Поворот мишени: {transform.eulerAngles}");
                Debug.Log($"Центр мишени: {targetCenter.position}");
                Debug.Log($"Размер мишени: {targetRadius * 2}м в диаметре");

                // Проверяем в какую сторону смотрит мишень
                Debug.Log($"Право (right): {transform.right}");
                Debug.Log($"Вверх (up): {transform.up}");
                Debug.Log($"Вперед (forward): {transform.forward}");

                // Для плоской мишени forward должен указывать на игрока
                if (!is3DTarget)
                {
                    Debug.Log($"Мишень плоская (is3DTarget = false)");
                    Debug.Log($"Будет использоваться 2D расстояние (X, Y)");
                }
            }
        }

        void InitializeScoreZones()
        {
            // Если зоны еще не инициализированы, создаем стандартные
            if (scoreZones == null || scoreZones.Length == 0)
            {
                scoreZones = new ScoreZone[]
                {
                    new ScoreZone { zoneName = "100", radius = 0.04f, scoreValue = 100, gizmoColor = Color.red },
                    new ScoreZone { zoneName = "80", radius = 0.08f, scoreValue = 80, gizmoColor = Color.yellow },
                    new ScoreZone { zoneName = "60", radius = 0.12f, scoreValue = 60, gizmoColor = Color.green },
                    new ScoreZone { zoneName = "50", radius = 0.16f, scoreValue = 50, gizmoColor = Color.green },
                    new ScoreZone { zoneName = "40", radius = 0.20f, scoreValue = 40, gizmoColor = Color.green },
                    new ScoreZone { zoneName = "30", radius = 0.24f, scoreValue = 30, gizmoColor = Color.green },
                    new ScoreZone { zoneName = "20", radius = 0.28f, scoreValue = 20, gizmoColor = Color.green },
                    new ScoreZone { zoneName = "10", radius = 0.30f, scoreValue = 10, gizmoColor = Color.green },
                    new ScoreZone { zoneName = "Outer Ring", radius = 0.31f, scoreValue = 0, gizmoColor = Color.blue }
                };

                Debug.Log("Score zones инициализированы стандартными значениями");
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
                Debug.Log($"Target radius обновлен до {targetRadius} (максимальный радиус зон)");
            }
        }

        void CreateTargetCenter()
        {
            GameObject centerObj = new GameObject("TargetCenter");
            centerObj.transform.SetParent(transform);
            centerObj.transform.localPosition = Vector3.zero;
            targetCenter = centerObj.transform;
            Debug.Log("Target center создан автоматически");
        }

        // Этот метод вызывается стрелой при попадании
        private void ApplyDamage()
        {
            // Для теста
            Vector3 approximateHitPoint = GetRandomPointOnTarget();
            OnHit(approximateHitPoint);
        }

        // С передачей точки
        public void ApplyDamageAtPoint(Vector3 hitPoint)
        {
            OnHit(hitPoint);
        }

        void OnHit(Vector3 hitPoint)
        {
            // 1. Рассчитываем расстояние до центра
            float distance = CalculateDistanceToCenter(hitPoint);

            Debug.Log($"Расстояние до центра: {distance:F3}m, Target Radius: {targetRadius:F3}m");

            // 2. Если попадание за пределами круга - это промах
            if (distance > targetRadius)
            {
                OnMiss(hitPoint, distance);
                return;
            }

            // 3. Определяем зону попадания
            int score = 0;
            string zoneName = "Miss";

            // Проверяем зоны от самой маленькой к большой
            for (int i = 0; i < scoreZones.Length; i++)
            {
                if (distance <= scoreZones[i].radius)
                {
                    score = scoreZones[i].scoreValue;
                    zoneName = scoreZones[i].zoneName;
                    Debug.Log($"Попадание в зону {zoneName} (радиус: {scoreZones[i].radius:F3}m)");
                    break;
                }
            }

            // 4. Визуальные эффекты
            PlayHitEffects(hitPoint, zoneName);

            // 5. Вызываем событие
            onTakeDamage.Invoke();

            // 6. Добавляем очки
            ScoreManager scoreManager = ScoreManager.Instance;
            if (scoreManager != null)
            {
                float accuracy = CalculateAccuracy(distance);
                Debug.Log($"Добавляем очки: {score}, зона: {zoneName}, точность: {accuracy:F1}%");
                scoreManager.AddScore(score, zoneName, accuracy);
            }
            else
            {
                Debug.LogError("ScoreManager.Instance равен null!");
            }

            Debug.Log($"Попадание в зону '{zoneName}'! Дистанция: {distance:F3}m -> +{score} очков");
        }

        float CalculateDistanceToCenter(Vector3 hitPoint)
        {
            // if (is3DTarget)
            // {
            // Полное 3D расстояние (X, Y, Z)
            float distance3D = Vector3.Distance(hitPoint, targetCenter.position);
            Debug.Log($"3D расстояние: {distance3D:F5}м (X:{hitPoint.x - targetCenter.position.x:F3}, Y:{hitPoint.y - targetCenter.position.y:F3}, Z:{hitPoint.z - targetCenter.position.z:F3})");
            return distance3D;

            // else
            // {
            //     // Для плоской мишени - расстояние в 2D (X, Y)
            //     // Игнорируем Z координату, так как мишень плоская
            //     Vector2 center2D = new Vector2(targetCenter.position.x, targetCenter.position.y);
            //     Vector2 hit2D = new Vector2(hitPoint.x, hitPoint.y);
            //     float distance2D = Vector2.Distance(center2D, hit2D);

            //     // Детальная отладка
            //     Debug.Log($"2D расстояние: {distance2D:F5}м");
            //     Debug.Log($"  Центр: ({center2D.x:F3}, {center2D.y:F3})");
            //     Debug.Log($"  Точка: ({hit2D.x:F3}, {hit2D.y:F3})");
            //     Debug.Log($"  Разница X: {Mathf.Abs(hit2D.x - center2D.x):F5}м");
            //     Debug.Log($"  Разница Y: {Mathf.Abs(hit2D.y - center2D.y):F5}м");

            //     // Также покажем 3D расстояние для сравнения
            //     float distance3D = Vector3.Distance(hitPoint, targetCenter.position);
            //     Debug.Log($"Для сравнения 3D расстояние: {distance3D:F5}м");

            //     return distance2D;
            // }
        }

        Vector3 GetRandomPointOnTarget()
        {
            // Случайная точка в пределах мишени
            float randomAngle = Random.Range(0f, Mathf.PI * 2f);
            float randomRadius = Random.Range(0f, targetRadius);

            Vector3 localPoint = new Vector3(
                Mathf.Cos(randomAngle) * randomRadius,
                Mathf.Sin(randomAngle) * randomRadius,
                0
            );

            return transform.TransformPoint(localPoint);
        }

        void OnMiss(Vector3 hitPoint, float distance)
        {
            Debug.Log($"Промах! Дистанция {distance:F3}m превышает радиус мишени {targetRadius:F3}m");

            if (missSound != null)
            {
                audioSource.clip = missSound;
                audioSource.Play();
            }

            if (missParticle != null)
            {
                Instantiate(missParticle, hitPoint, Quaternion.identity);
            }
        }

        void PlayHitEffects(Vector3 hitPoint, string zoneName)
        {
            if (hitSound != null)
            {
                audioSource.clip = hitSound;
                audioSource.Play();
            }

            if (hitParticle != null)
            {
                ParticleSystem particle = Instantiate(hitParticle, hitPoint, Quaternion.identity);
                ParticleSystem.MainModule main = particle.main;

                // Цвет в зависимости от очков
                if (zoneName.Contains("100")) main.startColor = Color.red;
                else if (zoneName.Contains("80")) main.startColor = Color.yellow;
                else if (zoneName.Contains("60")) main.startColor = Color.green;
                else if (zoneName.Contains("40")) main.startColor = Color.blue;
                else main.startColor = Color.white;
            }

            if (targetRenderer != null && hitMaterial != null)
            {
                StartCoroutine(HighlightTarget());
            }
        }

        System.Collections.IEnumerator HighlightTarget()
        {
            if (targetRenderer != null && hitMaterial != null)
            {
                targetRenderer.material = hitMaterial;
                yield return new WaitForSeconds(highlightDuration);
                targetRenderer.material = originalMaterial;
            }
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
            if (is3DTarget)
            {
                Gizmos.DrawWireSphere(center, targetRadius);
            }
            else
            {
                DrawWireCircle(center, targetRadius, 32);
            }

            // Рисуем зоны
            foreach (var zone in scoreZones)
            {
                Gizmos.color = zone.gizmoColor;
                if (is3DTarget)
                {
                    Gizmos.DrawWireSphere(center, zone.radius);
                }
                else
                {
                    DrawWireCircle(center, zone.radius, 32);
                }
            }

            // Центр
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(center, 0.01f);
        }

        void DrawWireCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 nextPoint = center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0
                );

                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }

        // Метод для получения информации о зоне
        public string GetZoneInfo(Vector3 point)
        {
            float distance = CalculateDistanceToCenter(point);

            string result = $"Расстояние: {distance:F5}м\n";
            result += $"Радиус мишени: {targetRadius:F5}м\n";

            if (distance > targetRadius)
            {
                return result + "Status: ВНЕ МИШЕНИ";
            }

            result += "Зоны (радиус -> очки):\n";
            for (int i = 0; i < scoreZones.Length; i++)
            {
                bool isInZone = distance <= scoreZones[i].radius;
                string marker = isInZone ? "🎯" : "  ";
                result += $"{marker} {scoreZones[i].zoneName}: {scoreZones[i].radius:F5}м -> {scoreZones[i].scoreValue} очков\n";
            }

            // Определяем конкретную зону
            foreach (var zone in scoreZones)
            {
                if (distance <= zone.radius)
                {
                    return result + $"Попадание в: {zone.zoneName} (+{zone.scoreValue})";
                }
            }

            return result + "Status: В мишени, но не в зоне";
        }

        // Отладочная информация
        public string GetDebugInfo(Vector3 hitPoint)
        {
            float distance = CalculateDistanceToCenter(hitPoint);
            Vector3 diff = hitPoint - targetCenter.position;

            string info = "=== Отладка мишени ===\n";
            info += $"Точка попадания: {hitPoint}\n";
            info += $"Центр мишени: {targetCenter.position}\n";
            info += $"Разница: X={diff.x:F3}, Y={diff.y:F3}, Z={diff.z:F3}\n";
            info += $"Расстояние: {distance:F3}m\n";
            info += $"Радиус мишени: {targetRadius:F3}m\n";
            info += $"Количество зон: {scoreZones?.Length ?? 0}\n";

            if (scoreZones != null)
            {
                info += "Зоны (радиус -> очки):\n";
                foreach (var zone in scoreZones)
                {
                    info += $"  {zone.zoneName}: {zone.radius:F3}m -> {zone.scoreValue} очков\n";
                }
            }

            info += $"Попадание в: {GetZoneInfo(hitPoint)}";

            return info;
        }
    }
}