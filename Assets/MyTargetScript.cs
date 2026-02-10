using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    public class MyTargetScript : MonoBehaviour
    {
        [Header("Target Settings")]
        public UnityEvent onTakeDamage; // Событие при попадании
        public int scoreValue = 10; // Сколько очков дает мишень

        [Header("Sound")]
        public AudioClip hitSound; // Звук попадания (опционально)
        
        private AudioSource audioSource;
        
        void Start()
        {
            // Создаем AudioSource если нужен звук
            if (hitSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f;
                audioSource.playOnAwake = false;
                audioSource.clip = hitSound;
            }
            
            // Убедимся что есть коллайдер
            if (GetComponent<Collider>() == null)
            {
                gameObject.AddComponent<BoxCollider>();
            }
        }
        
        // Этот метод вызывается стрелой при попадании
        private void ApplyDamage()
        {
            OnHit();
        }
        
        void OnHit()
        {
            // 1. Воспроизводим звук если есть
            audioSource?.Play();
            
            // 2. Вызываем событие (для других скриптов)
            onTakeDamage.Invoke();
            
            // 3. Добавляем очки через ScoreManager
            ScoreManager scoreManager = ScoreManager.Instance;
            scoreManager?.AddScore(scoreValue);
        }
    }
}