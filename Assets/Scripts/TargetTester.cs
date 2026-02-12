using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class TargetTester : MonoBehaviour
    {
        [Header("Test Settings")]
        public KeyCode testHitKey = KeyCode.T;
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

            if (Input.GetKeyDown(resetScoreKey) && ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetScore();
                Debug.Log("Счёт сброшен!");
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
                if (target == null)
                    target = hit.collider.GetComponentInParent<MyTargetScript>();

                if (target != null)
                {
                    // Вызываем правильный метод из MyTargetScript
                    target.ApplyDamage(hit.point);
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"Попадание в мишень! Точка: {hit.point}");
                    }
                }
            }
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(300, 10, 400, 200));

            GUILayout.Label("=== ТЕСТИРОВАНИЕ МИШЕНИ ===");
            GUILayout.Label("T - Попадание лучом (мышь)");
            GUILayout.Label("R - Сбросить счёт");

            if (ScoreManager.Instance != null)
            {
                GUILayout.Space(10);
                GUILayout.Label($"Счёт: {ScoreManager.Instance.GetCurrentScore()}");
            }

            // Информация о мишени под курсором
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, raycastDistance))
                {
                    MyTargetScript target = hit.collider.GetComponent<MyTargetScript>();
                    if (target == null)
                        target = hit.collider.GetComponentInParent<MyTargetScript>();

                    if (target != null && target.targetCenter != null)
                    {
                        GUILayout.Space(10);
                        float distance = Vector3.Distance(hit.point, target.targetCenter.position);
                        GUILayout.Label($"Расстояние до центра: {distance:F3}м");
                    }
                }
            }

            GUILayout.EndArea();
        }
    }
}