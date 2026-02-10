using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class MyTargetScript : MonoBehaviour
    {
        [Header("Target Settings")]
        public UnityEvent onTakeDamage;
        public bool onceOnly = false;
        public Transform targetCenter;
        public Transform baseTransform;
        public Transform fallenDownTransform;
        public float fallTime = 0.5f;
        
        [Header("Arrow Settings")]
        public PhysicMaterial targetPhysMaterial; // Назначьте тот же материал, что и у других мишеней
        public float minStickVelocity = 0.2f; // Минимальная скорость для прилипания стрелы
        
        [Header("Visual Effects")]
        public AudioClip hitSound;
        public ParticleSystem hitParticle;

        const float targetRadius = 0.25f;
        private bool targetEnabled = true;
        private AudioSource audioSource;
        
        // Список стрел, застрявших в этой мишени
        private List<Arrow> stuckArrows = new List<Arrow>();

        //-------------------------------------------------
        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f;
                audioSource.playOnAwake = false;
            }
            
            // Убедимся, что у мишени есть коллайдер
            Collider targetCollider = GetComponent<Collider>();
            if (targetCollider == null)
            {
                // Добавляем коллайдер по умолчанию
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(1f, 1f, 0.1f);
            }
            
            // Назначаем физический материал если он задан
            if (targetPhysMaterial != null)
            {
                Collider[] colliders = GetComponentsInChildren<Collider>();
                foreach (Collider collider in colliders)
                {
                    collider.material = targetPhysMaterial;
                }
            }
        }

        //-------------------------------------------------
        // Этот метод будет вызываться из стрелы через SendMessageUpwards
        private void ApplyDamage()
        {
            OnDamageTaken();
        }

        //-------------------------------------------------
        private void FireExposure()
        {
            OnDamageTaken();
        }

        //-------------------------------------------------
        private void OnDamageTaken()
        {
            if (targetEnabled)
            {
                // Воспроизводим звук попадания
                if (hitSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(hitSound);
                }
                
                // Воспроизводим партикл попадания
                if (hitParticle != null)
                {
                    hitParticle.Play();
                }
                
                // Вызываем событие
                onTakeDamage.Invoke();
                
                // Если мишень падает при попадании
                if (onceOnly && baseTransform != null && fallenDownTransform != null)
                {
                    StartCoroutine(FallDown());
                    targetEnabled = false;
                }
            }
        }

        //-------------------------------------------------
        private IEnumerator FallDown()
        {
            if (baseTransform && fallenDownTransform)
            {
                Quaternion startingRot = baseTransform.rotation;
                Vector3 startingPos = baseTransform.position;

                float startTime = Time.time;
                float rotLerp = 0f;

                while (rotLerp < 1)
                {
                    rotLerp = Util.RemapNumberClamped(Time.time, startTime, startTime + fallTime, 0f, 1f);
                    
                    // Интерполируем вращение
                    baseTransform.rotation = Quaternion.Lerp(startingRot, fallenDownTransform.rotation, rotLerp);
                    
                    // Также можно добавить интерполяцию позиции для плавности
                    baseTransform.position = Vector3.Lerp(startingPos, fallenDownTransform.position, rotLerp);
                    
                    yield return null;
                }
            }

            yield return null;
        }
        
        //-------------------------------------------------
        // Дополнительный метод для отслеживания стрел, застрявших в этой мишени
        public void RegisterStuckArrow(Arrow arrow)
        {
            if (!stuckArrows.Contains(arrow))
            {
                stuckArrows.Add(arrow);
                
                // Если нужно сделать что-то дополнительное при прилипании стрелы
                OnArrowStuck(arrow);
            }
        }
        
        //-------------------------------------------------
        private void OnArrowStuck(Arrow arrow)
        {
            // Дополнительные эффекты при прилипании стрелы
            // Например, можно запустить небольшую тряску мишени
            
            if (targetCenter != null)
            {
                StartCoroutine(ShakeTarget(0.1f, 0.05f));
            }
        }
        
        //-------------------------------------------------
        private IEnumerator ShakeTarget(float duration, float intensity)
        {
            Vector3 originalPosition = targetCenter.localPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                float shakeAmount = intensity * (1f - elapsed / duration);
                Vector3 shakeOffset = new Vector3(
                    Random.Range(-shakeAmount, shakeAmount),
                    Random.Range(-shakeAmount, shakeAmount),
                    Random.Range(-shakeAmount, shakeAmount)
                );
                
                targetCenter.localPosition = originalPosition + shakeOffset;
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            targetCenter.localPosition = originalPosition;
        }
        
        //-------------------------------------------------
        // Метод для очистки всех стрел (если нужно перезагрузить мишень)
        public void ClearAllArrows()
        {
            foreach (Arrow arrow in stuckArrows)
            {
                if (arrow != null && arrow.gameObject != null)
                {
                    Destroy(arrow.gameObject);
                }
            }
            stuckArrows.Clear();
        }
        
        //-------------------------------------------------
        // Получить все застрявшие стрелы
        public List<Arrow> GetStuckArrows()
        {
            return new List<Arrow>(stuckArrows);
        }
        
        //-------------------------------------------------
        // Метод для сброса мишени в исходное состояние
        public void ResetTarget()
        {
            if (baseTransform && fallenDownTransform)
            {
                StopAllCoroutines();
                baseTransform.rotation = Quaternion.identity;
                baseTransform.position = transform.position;
                targetEnabled = true;
                
                // Очищаем стрелы если нужно
                ClearAllArrows();
            }
        }
    }
}