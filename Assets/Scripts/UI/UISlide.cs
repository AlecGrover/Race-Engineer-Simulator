using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISlide : MonoBehaviour
{

    [Range(0, float.MaxValue)]
    public float SlideDuration = 1f;

    private bool _moved = false;
    private bool _inMotion = false;

    public Vector2 DistanceMoved = Vector2.one;

    public bool Reversible;

    private bool _halfwayDone = false;
    // WARNING: THIS WILL DESTORY THE PARENT TOO
    public bool DestroyWithParentAfterMoving = false;

    public void StartSlide()
    {
        if (_moved && !Reversible)
        {
            return;
        }
        if (!_inMotion)
        {
            StartCoroutine(Slide());
        }
    }

    private IEnumerator Slide()
    {
        _halfwayDone = false;
        var timeStarted = Time.time;
        var startingPosition = transform.localPosition;
        _inMotion = true;
        var thisMovement = _moved ? DistanceMoved * -1: DistanceMoved;
        if (SlideDuration <= float.Epsilon)
        {
            transform.Translate(thisMovement.x, thisMovement.y, 0);
            _halfwayDone = true;
            yield return new WaitForEndOfFrame();
        }
        else
        {
            while (Time.time - timeStarted <= SlideDuration)
            {
                _halfwayDone = (Time.time - timeStarted >= SlideDuration / 2f);
                transform.localPosition += new Vector3(thisMovement.x / SlideDuration * Time.deltaTime,
                    thisMovement.y / SlideDuration * Time.deltaTime, 0f);
                yield return new WaitForEndOfFrame();
            }
            _inMotion = false;
            transform.localPosition = startingPosition + new Vector3(thisMovement.x, thisMovement.y, 0);
        }
        _moved = !_moved;

        if (DestroyWithParentAfterMoving)
        {
            Destroy(transform.parent.gameObject);
        }


    }

    public bool IsHalfway()
    {
        return _halfwayDone;
    }


}
