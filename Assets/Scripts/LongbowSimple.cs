using UnityEngine;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
public class LongbowSimplified : MonoBehaviour
{
    public Transform pivotTransform;      // Центр вращения лука
    public Transform nockTransform;      // Куда вставляется стрела
    public Transform nockRestTransform;  // Исходная позиция тетивы

    public float minPull = 0.1f;         // Мин натяжение (когда стрела коснулась)
    public float maxPull = 0.6f;         // Макс натяжение (полный выстрел)
    public float arrowMinVelocity = 3f;  // Мин скорость стрелы
    public float arrowMaxVelocity = 30f; // Макс скорость стрелы

    public bool nocked = false;          // Стрела вставлена?
    public bool pulled = false;          // Тетива натянута?

    private Hand hand;
    private ArrowHand arrowHand;
    private float nockDistanceTravelled = 0f;
    private float arrowVelocity = 30f;

    //-------------------------------------------------
    private void OnAttachedToHand(Hand attachedHand)
    {
        hand = attachedHand;
    }

    //-------------------------------------------------
    private void HandAttachedUpdate(Hand hand)
    {
        if (nocked && arrowHand != null)
        {
            // Расстояние от базы лука до руки со стрелой
            Vector3 nockToArrowHand = arrowHand.arrowNockTransform.parent.position - nockRestTransform.position;
            float pullDistance = Mathf.Clamp(nockToArrowHand.magnitude, minPull, maxPull);

            // Двигать тетиву назад
            nockTransform.localPosition = new Vector3(0, 0, -pullDistance);
            nockDistanceTravelled = pullDistance;

            // Скорость выстрела зависит от натяжения
            arrowVelocity = Mathf.Lerp(arrowMinVelocity, arrowMaxVelocity, 
                (pullDistance - minPull) / (maxPull - minPull));

            // Поворот лука в сторону натяжения
            Vector3 pullDirection = (arrowHand.arrowNockTransform.parent.position - pivotTransform.position).normalized;
            pivotTransform.rotation = Quaternion.LookRotation(pullDirection, Vector3.up);

            // Флаг натяжения
            pulled = (pullDistance > minPull);
        }
    }

    //-------------------------------------------------
    public void StartNock(ArrowHand currentArrowHand)
    {
        arrowHand = currentArrowHand;
        nocked = true;
    }

    //-------------------------------------------------
    public void ArrowReleased()
    {
        nocked = false;
        pulled = false;
        hand.HoverUnlock(GetComponent<Interactable>());

        // Сброс позиции тетивы
        nockTransform.localPosition = Vector3.zero;
    }

    //-------------------------------------------------
    public float GetArrowVelocity()
    {
        return arrowVelocity;
    }

    //-------------------------------------------------
    public void ReleaseNock()
    {
        nocked = false;
        hand.HoverUnlock(GetComponent<Interactable>());
        nockTransform.localPosition = Vector3.zero;
    }

    //-------------------------------------------------
    public void ArrowInPosition()
    {
        // Стрела коснулась тетивы
    }
}