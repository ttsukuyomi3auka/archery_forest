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
            public float radius = 0.5f;
            public int scoreValue = 10;
            public Color gizmoColor = Color.white;
        }

        [Header("Target Settings")]
        public UnityEvent onTakeDamage;
        public Transform targetCenter;

        [Header("Score Zones")]
        public ScoreZone[] scoreZones;

        [Header("Sound")]
        public AudioClip hitSound;

        private AudioSource audioSource;

        void Awake()
        {
            InitializeScoreZones();
            
            if (targetCenter == null)
                CreateTargetCenter();
        }

        void Start()
        {
            if (scoreZones != null)
            {
                System.Array.Sort(scoreZones, (a, b) => a.radius.CompareTo(b.radius));
            }

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
            audioSource.playOnAwake = false;
        }

        void InitializeScoreZones()
        {
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

        void CreateTargetCenter()
        {
            GameObject centerObj = new GameObject("TargetCenter");
            centerObj.transform.SetParent(transform);
            centerObj.transform.localPosition = Vector3.zero;
            targetCenter = centerObj.transform;
        }

        public void ApplyDamage(Vector3 hitPoint)
        {
            OnHit(hitPoint);
        }

        void OnHit(Vector3 hitPoint)
        {
            float distance = Vector3.Distance(hitPoint, targetCenter.position);
            
            int score = 0;

            for (int i = 0; i < scoreZones.Length; i++)
            {
                if (distance <= scoreZones[i].radius)
                {
                    score = scoreZones[i].scoreValue;
                    break;
            }
        }

            if (hitSound != null)
            {
                audioSource.clip = hitSound;
                audioSource.Play();
            }

            onTakeDamage.Invoke();

            ScoreManager scoreManager = ScoreManager.Instance;
            if (scoreManager != null)
            {
                scoreManager.AddScore(score);
            }
        }

        void OnDrawGizmosSelected()
        {
            if (scoreZones == null || scoreZones.Length == 0)
                InitializeScoreZones();

            if (targetCenter == null) return;

            Vector3 center = targetCenter.position;

            foreach (var zone in scoreZones)
            {
                Gizmos.color = zone.gizmoColor;
                Gizmos.DrawWireSphere(center, zone.radius);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(center, 0.01f);
        }
    }
}