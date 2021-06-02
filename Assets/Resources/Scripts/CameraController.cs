using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
    
    [SerializeField]
    Camera cam;
    Vector3 previousPos = Vector3.zero;
    Vector3 deltaPos = Vector3.zero;
    // Vector3 offsetPos;

    // void Awake()
    // {
    //     offsetPos = cam.transform.position;
    // }

    void CamControl()
    {
        deltaPos = transform.position - previousPos;
        deltaPos.y = 0;
        cam.transform.position = Vector3.Lerp(cam.transform.position, cam.transform.position + deltaPos, Time.time);
        previousPos = transform.position;
    }

    public void focusTo(Vector3 x)
    {
        turnOffPlayerFocus();
        StopCoroutine(focusTransition(x));
        StartCoroutine(focusTransition(x));
    }

    IEnumerator focusTransition(Vector3 endPos, bool toPlayer = false)
    {
        Vector3 startPos = cam.transform.position;
        endPos.y = startPos.y;
        endPos.z -= startPos.y;
        float counter = 0f;
        float duration = 0.5f;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            if (toPlayer)
            {
                endPos = transform.position;
                endPos.y = startPos.y;
                endPos.z -= startPos.y;
            }
            float xCurrent = Mathf.Lerp(startPos.x, endPos.x, counter / duration);
            float zCurrent = Mathf.Lerp(startPos.z, endPos.z, counter / duration);
            cam.transform.position = new Vector3(xCurrent, startPos.y, zCurrent);
            yield return null;
        }
    }

    public void refocusPlayer()
    {
        turnOnPlayerFocus();
        StopCoroutine(focusTransition(transform.position, true));
        StartCoroutine(focusTransition(transform.position, true));
    }

    public GameObject refocusButton;

    public void turnOffPlayerFocus()
    {
        isPlayerFocused = false;
        refocusButton.SetActive(true);
    }
    public void turnOnPlayerFocus()
    {
        isPlayerFocused = true;
        refocusButton.SetActive(false);
    }

    public bool isPlayerFocused = true;

    void Update()
    {
        if (isPlayerFocused)
        {
            if (cam.GetComponent<Mapbox.Examples.CameraMovement>()._shouldDrag && !IsPointerOverUIObject() && Input.GetMouseButton(0))
                turnOffPlayerFocus();
            else
                CamControl();
        }

    }
}
