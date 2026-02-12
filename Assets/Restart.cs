using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    public class Restart : MonoBehaviour
    {
        [Header("Objects to Reset")]
        public GameObject[] objectsToReset;  // Объекты для возврата
        
        private bool isPressed = false;
        
        // Сохраняем начальные позиции
        private Vector3[] startPositions;
        private Quaternion[] startRotations;
        
        void Awake()
        {
            // Сохраняем позиции объектов
            if (objectsToReset != null)
            {
                startPositions = new Vector3[objectsToReset.Length];
                startRotations = new Quaternion[objectsToReset.Length];
                
                for (int i = 0; i < objectsToReset.Length; i++)
                {
                    if (objectsToReset[i] != null)
                    {
                        startPositions[i] = objectsToReset[i].transform.position;
                        startRotations[i] = objectsToReset[i].transform.rotation;
                    }
                }
            }
        }
        
        // Вызывается стрелой при попадании
        public void ApplyDamage(Vector3 hitPoint)
        {
            if (isPressed) return;
            
            PressButton();
        }
        
        void PressButton()
        {
            isPressed = true;
            
            // Сброс счета
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetScore();
                Debug.Log("Кнопка нажата! Счет сброшен.");
            }
            
            // Возврат объектов
            ResetAllObjects();
            
            // Событие
            onButtonPressed.Invoke();
            
            // Разблокируем кнопку
            isPressed = false;
        }
        
        void ResetAllObjects()
        {
            if (objectsToReset == null) return;
            
            for (int i = 0; i < objectsToReset.Length; i++)
            {
                if (objectsToReset[i] != null)
                {
                    objectsToReset[i].transform.position = startPositions[i];
                    objectsToReset[i].transform.rotation = startRotations[i];
                    
                    // Сбрасываем физику
                    Rigidbody rb = objectsToReset[i].GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.velocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                    
                    // Уничтожаем стрелы
                    if (objectsToReset[i].GetComponent<Arrow>() != null)
                    {
                        Destroy(objectsToReset[i]);
                    }
                }
            }
            
            Debug.Log("Объекты возвращены на исходные позиции");
        }
    }
}