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

                    target.SendMessage("ApplyDamageAtPoint", hit.point, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    target = hit.collider.GetComponentInParent<MyTargetScript>();
                    target?.SendMessage("ApplyDamageAtPoint", hit.point, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        void TestHit(MyTargetScript target, Vector3 point, string testName)
        {
            Debug.Log($"\n=== Тест: {testName} ===");

            target.SendMessage("ApplyDamageAtPoint", point, SendMessageOptions.DontRequireReceiver);
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(300, 10, 400, 400));

            GUILayout.Label("=== ТЕСТИРОВАНИЕ МИШЕНИ ===");
            GUILayout.Label("T - Попадание лучом (мышь)");
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

                        float distance3D = Vector3.Distance(hit.point, target.targetCenter.position);
                        GUILayout.Label($"3D расстояние: {distance3D:F3}m");

                    }
                }
            }

            GUILayout.EndArea();
        }
    }
}