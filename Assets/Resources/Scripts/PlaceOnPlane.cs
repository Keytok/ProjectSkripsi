using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// <summary>
    /// Listens for touch events and performs an AR raycast from the screen touch point.
    /// AR raycasts will only hit detected trackables like feature points and planes.
    ///
    /// If a raycast hits a trackable, the <see cref="placedPrefab"/> is instantiated
    /// and moved to the hit position.
    /// </summary>
    [RequireComponent(typeof(ARRaycastManager))]
    public class PlaceOnPlane : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Instantiates this prefab on a plane at the touch location.")]
        GameObject m_PlacedPrefab;

        /// <summary>
        /// The prefab to instantiate on touch.
        /// </summary>
        public GameObject placedPrefab
        {
            get { return m_PlacedPrefab; }
            set { m_PlacedPrefab = value; }
        }

        /// <summary>
        /// The object instantiated as a result of a successful raycast intersection with a plane.
        /// </summary>
        public GameObject spawnedObject { get; private set; }

        void Awake()
        {
            m_RaycastManager = GetComponent<ARRaycastManager>();
        }

        void Start()
        {
            // SET DEFAULT CHAR
            GameObject adminManager = GameObject.Find("Admin Manager");
            if (adminManager != null)
            {
                int tCurrent = adminManager.GetComponent<AdminManager>().getDefaultChar();
                if (tCurrent != currentModel)
                {
                    currentModel = tCurrent;
                    updateChar();
                }
            }
        }

        bool TryGetTouchPosition(out Vector2 touchPosition)
        {
            if (Input.touchCount > 0)
            {
                touchPosition = Input.GetTouch(0).position;
                return true;
            }

            touchPosition = default;
            return false;
        }

        [SerializeField] private GameObject[] characterModels = new GameObject[3];

        private int currentModel = 0;
        private void setAnim()
        {
            anim = null;
            anim = spawnedObject.GetComponent<Animator>();
            if (anim != null)
            {
                currentState = anim.GetCurrentAnimatorStateInfo(0);
                previousState = currentState;
            }
        }
        public Animator getAnim()
        {
            return anim;
        }

        public void updateObj()
        {
            currentModel = characterModels.Length;
            updateChar();
        }

        public float defaultObjectScale = 1.5f;

        public void updateChar()
        {
            GameObject loadedObj = uiManager.getLoadedObj();
            if (currentModel >= characterModels.Length)
                m_PlacedPrefab = loadedObj;
            else
                m_PlacedPrefab = characterModels[currentModel];

            if (spawnedObject != null)
            {
                Transform temp = spawnedObject.transform;
                spawnedObject.Destroy();
                if (currentModel >= characterModels.Length)
                    spawnedObject = Instantiate(loadedObj, temp.position, temp.rotation);
                else
                    spawnedObject = Instantiate(characterModels[currentModel], temp.position, temp.rotation);
                spawnedObject.SetActive(true);
                spawnedObject.transform.localScale = new Vector3(defaultObjectScale, defaultObjectScale, defaultObjectScale);
                setAnim();
            }
        }
        public void charNext()
        {
            GameObject loadedObj = uiManager.getLoadedObj();
            if (currentModel >= characterModels.Length - 1 && loadedObj == null || currentModel >= characterModels.Length && loadedObj != null)
                currentModel = -1;
            currentModel++;
            updateChar();
        }
        public void charBack()
        {
            GameObject loadedObj = uiManager.getLoadedObj();
            if (currentModel <= 0 && loadedObj == null)
                currentModel = characterModels.Length;
            else if (currentModel <= 0 && loadedObj != null)
                currentModel = characterModels.Length + 1;
            currentModel--;
            updateChar();
        }

        private Animator anim;
        private AnimatorStateInfo currentState;
        private AnimatorStateInfo previousState;

        public void animNext()
        {
            if (spawnedObject != null && anim != null)
                anim.SetBool("Next", true);
        }

        public void animBack()
        {
            if (spawnedObject != null && anim != null)
                anim.SetBool("Back", true);
        }

        public void rotateClockwise()
        {
            if (spawnedObject != null)
                spawnedObject.transform.Rotate(0.0f, 15, 0.0f, Space.World);
        }

        public void rotateCounterClockwise()
        {
            if (spawnedObject != null)
                spawnedObject.transform.Rotate(0.0f, -15, 0.0f, Space.World);
        }

        public void scaleUp()
        {
            if (spawnedObject != null)
            {
                Vector3 temp = spawnedObject.transform.localScale;
                // if (temp.x >= 0.095f)
                if (temp.x >= 0.1f)
                {
                    temp.x += 0.1f;
                    temp.y += 0.1f;
                    temp.z += 0.1f;
                    spawnedObject.transform.localScale = temp;
                }
                // else if (temp.x >= 0.0045f)
                else
                {
                    temp.x += 0.005f;
                    temp.y += 0.005f;
                    temp.z += 0.005f;
                    spawnedObject.transform.localScale = temp;
                }
            }
        }

        public void scaleDown()
        {
            if (spawnedObject != null)
            {
                Vector3 temp = spawnedObject.transform.localScale;
                if (temp.x > 0.15f)
                {
                    temp.x -= 0.1f;
                    temp.y -= 0.1f;
                    temp.z -= 0.1f;
                    spawnedObject.transform.localScale = temp;
                }
                // else if (temp.x >= 0.075f)
                else if (temp.x >= 0.0055f)
                {
                    temp.x -= 0.005f;
                    temp.y -= 0.005f;
                    temp.z -= 0.005f;
                    spawnedObject.transform.localScale = temp;
                }
                else
                {
                    temp.x = 0.005f;
                    temp.y = 0.005f;
                    temp.z = 0.005f;
                    spawnedObject.transform.localScale = temp;
                }
            }
        }

        public void destroySpawnedObject()
        {
            if (spawnedObject != null)
            {
                spawnedObject.Destroy();
                spawnedObject = null;

                // SET PLANE
                uiManager.turnHintOn(true);
            }
        }

        bool IsPointOverUIObject(Vector2 pos)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return false;

            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(pos.x, pos.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;

        }

        private bool canPlace = true;
        public void setCanPlace(bool x)
        {
            canPlace = x;
        }

        public float yAxisRotateValue = 180;
        public ARUIManager uiManager;

        void Update()
        {

            if (spawnedObject != null && anim != null)
            {
                if (anim.GetBool("Next"))
                {
                    currentState = anim.GetCurrentAnimatorStateInfo(0);
                    if (previousState.fullPathHash != currentState.fullPathHash)
                    {
                        anim.SetBool("Next", false);
                        previousState = currentState;
                    }
                }

                if (anim.GetBool("Back"))
                {
                    currentState = anim.GetCurrentAnimatorStateInfo(0);
                    if (previousState.fullPathHash != currentState.fullPathHash)
                    {
                        anim.SetBool("Back", false);
                        previousState = currentState;
                    }
                }
            }

            if (!TryGetTouchPosition(out Vector2 touchPosition))
                return;

            if (canPlace && !IsPointOverUIObject(touchPosition) && m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                // Raycast hits are sorted by distance, so the first one
                // will be the closest hit.
                var hitPose = s_Hits[0].pose;

                if (spawnedObject == null)
                {
                    spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
                    spawnedObject.SetActive(true);
                    spawnedObject.transform.Rotate(0.0f, yAxisRotateValue, 0.0f, Space.World);
                    spawnedObject.transform.localScale = new Vector3(defaultObjectScale, defaultObjectScale, defaultObjectScale);

                    // SET ANIM & PLANE
                    setAnim();
                    uiManager.turnHintOn(false);
                }
                else
                {
                    spawnedObject.transform.position = hitPose.position;
                }
            }
        }

        static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

        ARRaycastManager m_RaycastManager;
    }
}
