using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class TargetTester : MonoBehaviour
    {
        [Header("Test Settings")]
        public KeyCode testHitKey = KeyCode.T;
        public KeyCode resetScoreKey = KeyCode.R;
        public float raycastDistance = 100f;
        
        [Header("Visual")]
        public Color rayColor = Color.green;
        
        void Update()
        {
            // Тестовое попадание по T
            if (Input.GetKeyDown(testHitKey))
            {
                TestHit();
            }
            
            // Сброс счета по R
            if (Input.GetKeyDown(resetScoreKey) && ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetScore();
                Debug.Log("Счет сброшен!");
            }
        }
        
        void TestHit()
        {
            if (Camera.main == null)
            {
                Debug.LogError("Main Camera не найдена!");
                return;
            }
            
            // Создаем луч от камеры к точке под курсором мыши
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            // Рисуем луч в сцене для визуализации
            Debug.DrawRay(ray.origin, ray.direction * raycastDistance, rayColor, 1f);
            
            // Проверяем столкновение луча с объектами
            if (Physics.Raycast(ray, out hit, raycastDistance))
            {
                Debug.Log("Попал в: " + hit.collider.name + " в точке: " + hit.point);
                
                // Пытаемся найти мишень
                MyTargetScript target = hit.collider.GetComponent<MyTargetScript>();
                if (target != null)
                {
                    // Вызываем попадание
                    target.SendMessage("ApplyDamage", SendMessageOptions.DontRequireReceiver);
                    Debug.Log("Попадание зарегистрировано! +" + target.scoreValue + " очков");
                }
                else
                {
                    // Проверяем в родительских объектах
                    target = hit.collider.GetComponentInParent<MyTargetScript>();
                    if (target != null)
                    {
                        target.SendMessage("ApplyDamage", SendMessageOptions.DontRequireReceiver);
                        Debug.Log("Попадание зарегистрировано в родительском объекте! +" + target.scoreValue + " очков");
                    }
                    else
                    {
                        Debug.LogWarning("Объект " + hit.collider.name + " не является мишенью!");
                    }
                }
            }
            else
            {
                Debug.Log("Не попал ни в что");
            }
        }
        
        void OnGUI()
        {
            // Простой UI для тестирования
            GUI.Label(new Rect(300, 10, 300, 50), "ТЕСТИРОВАНИЕ МИШЕНИ:");
            GUI.Label(new Rect(300, 30, 300, 50), "T - Тестовое попадание (куда смотрит мышь)");
            GUI.Label(new Rect(300, 50, 300, 50), "R - Сбросить счет");
            
            if (ScoreManager.Instance != null)
            {
                GUI.Label(new Rect(300, 80, 300, 50), "Текущий счет: " + ScoreManager.Instance.GetCurrentScore());
            }
        }
    }
}