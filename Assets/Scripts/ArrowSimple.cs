using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    [RequireComponent(typeof(Rigidbody))]
    public class ArrowSimplified : MonoBehaviour
    {
        public Rigidbody arrowHeadRB;      // Физика наконечника
        public Rigidbody shaftRB;          // Физика древка

        public float stickMinSpeed = 0.2f;    // Минимальная скорость для застревания
        public float stickLifetime = 30f;   // Время жизни воткнутой стрелы

        private bool inFlight = false;      // В полете?
        private bool isStuck = false;       // Застряла?
        private float releaseVelocity = 0f; // Скорость выпуска

        //-------------------------------------------------
        private void Start()
        {
            // Игнорируем коллизию с игроком (чтоб не застревать в камере)
            if (Player.instance != null)
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), Player.instance.headCollider);
            }

            // Настраиваем физику
            shaftRB.interpolation = RigidbodyInterpolation.Interpolate;
            shaftRB.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            arrowHeadRB.interpolation = RigidbodyInterpolation.Interpolate;
            arrowHeadRB.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        //-------------------------------------------------
        public void ArrowReleased(float inputVelocity)
        {
            inFlight = true;
            releaseVelocity = inputVelocity;

            // Применяем скорость к стреле
            shaftRB.velocity = transform.forward * inputVelocity;
            arrowHeadRB.velocity = transform.forward * inputVelocity;

            // Уничтожаем через 30 секунд, если никуда не воткнулась
            Destroy(gameObject, 30f);
        }

        //-------------------------------------------------
        private void FixedUpdate()
        {
            if (inFlight && !isStuck)
            {
                // Древко следует за наконечником через Joint
                // (Joint настроен в редакторе: FixedJoint соединяет arrowHeadRB и shaftRB)
            }
        }

        //-------------------------------------------------
        private void OnCollisionEnter(Collision collision)
        {
            if (!inFlight || isStuck) return;

            float hitSpeed = shaftRB.velocity.magnitude;

            // Если ударили достаточно сильно - застреваем
            if (hitSpeed >= stickMinSpeed)
            {
                StickInTarget(collision);
            }
            else
            {
                // Слишком слабо - просто уничтожаем
                Destroy(gameObject);
            }
        }

        //-------------------------------------------------
        private void StickInTarget(Collision collision)
        {
            inFlight = false;
            isStuck = true;

            // Останавливаем физику
            shaftRB.velocity = Vector3.zero;
            shaftRB.angularVelocity = Vector3.zero;
            shaftRB.isKinematic = true;
            shaftRB.useGravity = false;

            arrowHeadRB.velocity = Vector3.zero;
            arrowHeadRB.angularVelocity = Vector3.zero;
            arrowHeadRB.isKinematic = true;
            arrowHeadRB.useGravity = false;

            // Отключаем коллайдеры (чтобы не мешали)
            shaftRB.GetComponent<Collider>().enabled = false;
            arrowHeadRB.GetComponent<Collider>().enabled = false;

            // Создаем родительский объект для масштабирования
            GameObject stickParent = new GameObject("ArrowStickParent");

            // Прикрепляем к тому, во что воткнулись
            stickParent.transform.parent = collision.collider.transform;
            stickParent.transform.position = collision.contacts[0].point;

            // Стрела становится дочерней
            transform.parent = stickParent.transform;

            // Позиционируем стрелу правильно (немного утапливаем в поверхность)
            transform.position = collision.contacts[0].point - transform.forward * 0.3f;
            transform.rotation = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(transform.forward, collision.contacts[0].normal).normalized
            );

            // Уничтожаем через время (стрела исчезнет)
            Destroy(gameObject, stickLifetime);
            Destroy(stickParent, stickLifetime);
        }

        //-------------------------------------------------
        private void OnDestroy()
        {
        }
    }
}