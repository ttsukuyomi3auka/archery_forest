using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class TargetTester : MonoBehaviour
    {
        [Header("Test Settings")]
        public KeyCode testHitKey = KeyCode.T;
        public KeyCode debugHitKey = KeyCode.D; // Отладочные попадания
        public KeyCode resetScoreKey = KeyCode.R;
        public float raycastDistance = 100f;
        
        [Header("Debug Mode")]
        public bool showDebugInfo = true;
        
        void Update()
        {
            if (Input.GetKeyDown(testHitKey))
            {
                TestRaycastHit();
            }
            
            if (Input.GetKeyDown(debugHitKey))
            {
                TestDebugHits();
            }
            
            if (Input.GetKeyDown(resetScoreKey) && ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetScore();
                Debug.Log("Счет сброшен!");
            }
        }
        
        void TestRaycastHit()
        {
            if (Camera.main == null)
            {
                Debug.LogError("Main Camera не найдена!");
                return;
            }
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.green, 1f);
            
            if (Physics.Raycast(ray, out hit, raycastDistance))
            {
                MyTargetScript target = hit.collider.GetComponent<MyTargetScript>();
                if (target != null)
                {
                    // Показываем отладочную информацию
                    if (showDebugInfo)
                    {
                        string debugInfo = target.GetDebugInfo(hit.point);
                        Debug.Log(debugInfo);
                    }
                    
                    target.SendMessage("ApplyDamageAtPoint", hit.point, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    target = hit.collider.GetComponentInParent<MyTargetScript>();
                    if (target != null)
                    {
                        if (showDebugInfo)
                        {
                            string debugInfo = target.GetDebugInfo(hit.point);
                            Debug.Log(debugInfo);
                        }
                        
                        target.SendMessage("ApplyDamageAtPoint", hit.point, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }
        
        void TestDebugHits()
        {
            MyTargetScript[] targets = FindObjectsOfType<MyTargetScript>();
            if (targets.Length == 0)
            {
                Debug.Log("Мишени не найдены!");
                return;
            }
            
            foreach (var target in targets)
            {
                if (target.targetCenter == null) continue;
                
                Vector3 center = target.targetCenter.position;
                
                // Тест 1: Центр
                TestHit(target, center, "Центр");
                
                // Тест 2: Вправо (по X)
                TestHit(target, center + Vector3.right * 0.15f, "Вправо (X+)");
                
                // Тест 3: Влево (по X)
                TestHit(target, center + Vector3.left * 0.15f, "Влево (X-)");
                
                // Тест 4: Вверх (по Y)
                TestHit(target, center + Vector3.up * 0.15f, "Вверх (Y+)");
                
                // Тест 5: Вниз (по Y)
                TestHit(target, center + Vector3.down * 0.15f, "Вниз (Y-)");
                
                // Тест 6: По диагонали
                TestHit(target, center + new Vector3(0.1f, 0.1f, 0), "Диагональ (X+Y+)");
                
                // Тест 7: Край мишени
                TestHit(target, center + new Vector3(0.3f, 0, 0), "Край (X+)");
                
                // Тест 8: За пределами
                TestHit(target, center + new Vector3(0.4f, 0, 0), "За пределами");
            }
        }
        
        void TestHit(MyTargetScript target, Vector3 point, string testName)
        {
            Debug.Log($"\n=== Тест: {testName} ===");
            string debugInfo = target.GetDebugInfo(point);
            Debug.Log(debugInfo);
            
            target.SendMessage("ApplyDamageAtPoint", point, SendMessageOptions.DontRequireReceiver);
        }
        
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(300, 10, 400, 400));
            
            GUILayout.Label("=== ТЕСТИРОВАНИЕ МИШЕНИ ===");
            GUILayout.Label("T - Попадание лучом (мышь)");
            GUILayout.Label("D - Отладочные тесты");
            GUILayout.Label("R - Сбросить счет");
            
            if (ScoreManager.Instance != null)
            {
                GUILayout.Space(10);
                GUILayout.Label($"Счет: {ScoreManager.Instance.GetCurrentScore()}");
                GUILayout.Label($"Точность: {ScoreManager.Instance.GetAverageAccuracy():F1}%");
            }
            
            // Информация о мишени под курсором
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit, raycastDistance))
                {
                    MyTargetScript target = hit.collider.GetComponent<MyTargetScript>();
                    if (target != null && target.targetCenter != null)
                    {
                        GUILayout.Space(10);
                        GUILayout.Label($"Мишень: {target.name}");
                        
                        Vector3 diff = hit.point - target.targetCenter.position;
                        GUILayout.Label($"От центра:");
                        GUILayout.Label($"  X: {diff.x:F3}m");
                        GUILayout.Label($"  Y: {diff.y:F3}m");
                        GUILayout.Label($"  Z: {diff.z:F3}m");
                        
                        float distance2D = Vector2.Distance(
                            new Vector2(hit.point.x, hit.point.y),
                            new Vector2(target.targetCenter.position.x, target.targetCenter.position.y)
                        );
                        GUILayout.Label($"2D расстояние: {distance2D:F3}m");
                        
                        float distance3D = Vector3.Distance(hit.point, target.targetCenter.position);
                        GUILayout.Label($"3D расстояние: {distance3D:F3}m");
                        
                        string zoneInfo = target.GetZoneInfo(hit.point);
                        GUILayout.Label($"Зона: {zoneInfo}");
                    }
                }
            }
            
            GUILayout.EndArea();
        }
    }
}