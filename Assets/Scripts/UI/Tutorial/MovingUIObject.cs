using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI.Tutorial
{
    public class MovingUIObject : MonoBehaviour
    {
        public float MoveTime = 1f;
        public bool IsMoving { get; private set; } = false;


        public void SendToNewTransform(Vector3 newLocation, Vector3 newScale)
        {
            if (!IsMoving)
            {
                IsMoving = true;
                StartCoroutine(MoveToDestination(newLocation, newScale));
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(MoveToDestination(newLocation, newScale));
            }
        }

        private IEnumerator MoveToDestination(Vector3 newLocation, Vector3 newScale)
        {

            if (MoveTime >= Mathf.Epsilon)
            {
                float startTime = Time.time;
                Vector3 positionDisplacement = newLocation - transform.localPosition;
                Vector3 scaleDisplacement = newScale - transform.localScale;
                while (Time.time - startTime < MoveTime)
                {
                    transform.localPosition += positionDisplacement * Time.deltaTime / MoveTime;
                    transform.localScale += scaleDisplacement * Time.deltaTime / MoveTime;
                    yield return new WaitForEndOfFrame();
                }
            }
            transform.localPosition = newLocation;
            transform.localScale = newScale;
            IsMoving = false;
            yield return new WaitForEndOfFrame();
        }

    }
}
