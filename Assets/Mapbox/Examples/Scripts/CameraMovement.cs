namespace Mapbox.Examples
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using Mapbox.Unity.Map;
    using System.Collections.Generic;

    public class CameraMovement : MonoBehaviour
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
        AbstractMap _map;

        [SerializeField]
        float _panSpeed = 20f;

        [SerializeField]
        float _zoomSpeed = 50f;

        [SerializeField]
        Camera _referenceCamera;

        Quaternion _originalRotation;
        Vector3 _origin;
        Vector3 _delta;
        // bool _shouldDrag;
        public bool _shouldDrag;

        void HandleTouch()
        {
            float zoomFactor = 0.0f;
            //pinch to zoom. 
            switch (Input.touchCount)
            {
                case 1:
                    {
                        HandleMouseAndKeyBoard();
                    }
                    break;
                case 2:
                    {
                        // Store both touches.
                        Touch touchZero = Input.GetTouch(0);
                        Touch touchOne = Input.GetTouch(1);

                        // Find the position in the previous frame of each touch.
                        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                        // Find the magnitude of the vector (the distance) between the touches in each frame.
                        float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                        float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                        // Find the difference in the distances between each frame.
                        zoomFactor = 0.05f * (touchDeltaMag - prevTouchDeltaMag);
                    }
                    ZoomMapUsingTouchOrMouse(zoomFactor);
                    break;
                default:
                    break;
            }
        }

        // START EDIT 1/2 ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public GameObject raycastPlane;
		public float minHeight = 25f;
		public float maxHeight = 75f;

        void ZoomMapUsingTouchOrMouse(float zoomFactor) // EXC
        {
            if (transform.localPosition.y > minHeight && zoomFactor > 0 || transform.localPosition.y < maxHeight && zoomFactor < 0)
            {
                Vector3 tempLP = raycastPlane.transform.position;
                var y = zoomFactor * _zoomSpeed; // EXC
                transform.localPosition += (transform.forward * y); // EXC
                raycastPlane.transform.position = tempLP;
            }
            // END EDIT 1/2 /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }

        void HandleMouseAndKeyBoard()
        {
            if (Input.GetMouseButton(0) && !IsPointerOverUIObject())
            {
                var mousePosition = Input.mousePosition;
                mousePosition.z = _referenceCamera.transform.localPosition.y;
                _delta = _referenceCamera.ScreenToWorldPoint(mousePosition) - _referenceCamera.transform.localPosition;
                _delta.y = 0f;
                if (_shouldDrag == false)
                {
                    _shouldDrag = true;
                    _origin = _referenceCamera.ScreenToWorldPoint(mousePosition);
                }
            }
            else
            {
                _shouldDrag = false;
            }

            if (_shouldDrag == true)
            {
                var offset = _origin - _delta;
                offset.y = transform.localPosition.y;
                transform.localPosition = offset;
            }
            else
            {
                if (IsPointerOverUIObject())
                {
                    return;
                }

                var x = Input.GetAxis("Horizontal");
                var z = Input.GetAxis("Vertical");
                var y = Input.GetAxis("Mouse ScrollWheel") * _zoomSpeed;
                if (!(Mathf.Approximately(x, 0) && Mathf.Approximately(y, 0) && Mathf.Approximately(z, 0)))
                {
                    // START EDIT 2/2 ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    if (transform.localPosition.y > minHeight && y > 0 || transform.localPosition.y < maxHeight && y < 0)
                    {
                        Vector3 tempLP = raycastPlane.transform.position;
                        transform.localPosition += transform.forward * y + (_originalRotation * new Vector3(x * _panSpeed, 0, z * _panSpeed)); // EXC
                        raycastPlane.transform.position = tempLP;
                        _map.UpdateMap(); // EXC
                    }
                    // END EDIT 2/2 /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
            }


        }

        void Awake()
        {
            _originalRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

            if (_referenceCamera == null)
            {
                _referenceCamera = GetComponent<Camera>();
                if (_referenceCamera == null)
                {
                    throw new System.Exception("You must have a reference camera assigned!");
                }
            }

            if (_map == null)
            {
                _map = FindObjectOfType<AbstractMap>();
                if (_map == null)
                {
                    throw new System.Exception("You must have a reference map assigned!");
                }
            }
        }

        void LateUpdate()
        {

            if (Input.touchSupported && Input.touchCount > 0)
            {
                HandleTouch();
            }
            else
            {
                HandleMouseAndKeyBoard();
            }
        }
    }
}