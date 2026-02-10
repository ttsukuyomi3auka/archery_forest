using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Display Settings")]
        public Vector3 offsetFromPlayer = new Vector3(0, 0.3f, 1.5f);

        private TextMesh scoreText;
        private int score = 0;
        private Transform playerHead;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            SetupText();
            FindPlayerHead();
        }

        public int GetCurrentScore()
        {
            return score;
        }

        public void ResetScore()
        {
            scoreText.text = "Score: 0";
            score = 0;
        }

        void SetupText()
        {
            GameObject textObj = new GameObject("ScoreDisplay");
            scoreText = textObj.AddComponent<TextMesh>();

            scoreText.text = "Score: 0";
            scoreText.fontSize = 80;
            scoreText.characterSize = 0.01f;
            scoreText.anchor = TextAnchor.MiddleCenter;
            scoreText.alignment = TextAlignment.Center;
            scoreText.color = Color.green;

            // Обводка через несколько текстов (простой способ)
            CreateTextOutline(textObj);

            textObj.AddComponent<FacePlayer>();
        }

        void CreateTextOutline(GameObject parent)
        {
            // Создаем 4 копии текста для обводки
            for (int i = 0; i < 4; i++)
            {
                GameObject outlineObj = new GameObject($"Outline_{i}");
                outlineObj.transform.SetParent(parent.transform);
                outlineObj.transform.localPosition = GetOffset(i);

                TextMesh outline = outlineObj.AddComponent<TextMesh>();
                outline.text = "Score: 0";
                outline.fontSize = 80;
                outline.characterSize = 0.01f;
                outline.anchor = TextAnchor.MiddleCenter;
                outline.color = Color.black;
            }
        }

        Vector3 GetOffset(int index)
        {
            switch (index)
            {
                case 0: return new Vector3(0.002f, 0, 0);
                case 1: return new Vector3(-0.002f, 0, 0);
                case 2: return new Vector3(0, 0.002f, 0);
                case 3: return new Vector3(0, -0.002f, 0);
                default: return Vector3.zero;
            }
        }

        void FindPlayerHead()
        {
            if (Player.instance != null)
            {
                playerHead = Player.instance.hmdTransform;
            }
        }

        void Update()
        {
            // Обновляем позицию относительно игрока
            if (playerHead != null && scoreText != null)
            {
                scoreText.transform.position = playerHead.position +
                    playerHead.forward * offsetFromPlayer.z +
                    playerHead.up * offsetFromPlayer.y +
                    playerHead.right * offsetFromPlayer.x;
            }
        }

        public void AddScore(int points)
        {
            score += points;
            UpdateDisplay();
        }

        void UpdateDisplay()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";

                // Обновляем все обводки
                foreach (Transform child in scoreText.transform)
                {
                    TextMesh childText = child.GetComponent<TextMesh>();
                    if (childText != null)
                    {
                        childText.text = $"Score: {score}";
                    }
                }
            }
        }
    }

    public class FacePlayer : MonoBehaviour
    {
        private Transform playerHead;

        void Start()
        {
            if (Player.instance != null)
            {
                playerHead = Player.instance.hmdTransform;
            }
        }

        void LateUpdate()
        {
            if (playerHead != null)
            {
                transform.LookAt(2 * transform.position - playerHead.position);
            }
        }
    }
}