using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.EventSystems;
using Mapbox.Unity.Location;

public class MapUIManager : MonoBehaviour
{

    [SerializeField] private GameObject helpBG;
    [SerializeField] private QuantumTek.QuantumUI.QUI_Window helpWindow;

    public void toggleHelp()
    {
        helpWindow.SetActive(!helpBG.activeSelf);
        helpBG.SetActive(!helpBG.activeSelf);
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    public ListManager listManager;

    List<GameObject> _spawnedObjects;

    FirebaseFirestore db;
    // LOCATION PROVIDER/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private AbstractLocationProvider _locationProvider = null;
    private List<string> m_DropOptions;
    void Start()
    {
        if (null == _locationProvider)
        {
            _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider as AbstractLocationProvider;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // == NULL FOR EDITOR ONLY
        GameObject adminManager = GameObject.Find("Admin Manager");
        if (adminManager == null)
        {
            // SET DROPDOWN
            m_DropOptions = new List<string> { "c1", "c2", "c3", "c4", "c5", "c6", "c7" };
            propChar.ClearOptions();
            propChar.AddOptions(m_DropOptions);
            editChar.ClearOptions();
            editChar.AddOptions(m_DropOptions);

            GameObject.Find("btnAddPlayer").SetActive(true);
            GameObject.Find("btnAddCustom").SetActive(true);
            GameObject.Find("btnEdit").SetActive(true);
            GameObject.Find("btnDelete").SetActive(true);
        }
        else
        {
            // SET DROPDOWN
            m_DropOptions = adminManager.GetComponent<AdminManager>().getCharName();
            propChar.ClearOptions();
            propChar.AddOptions(m_DropOptions);
            editChar.ClearOptions();
            editChar.AddOptions(m_DropOptions);

            if (adminManager.GetComponent<AdminManager>().getMode())
            {
                GameObject.Find("btnAddPlayer").SetActive(true);
                GameObject.Find("btnAddCustom").SetActive(true);
                GameObject.Find("btnEdit").SetActive(true);
                GameObject.Find("btnDelete").SetActive(true);
            }
            else
            {
                GameObject.Find("btnAddPlayer").SetActive(false);
                GameObject.Find("btnAddCustom").SetActive(false);
                GameObject.Find("btnEdit").SetActive(false);
                GameObject.Find("btnDelete").SetActive(false);
            }
        }

        // INITIALIZE
        _spawnedObjects = new List<GameObject>();

        // GET FROM DATABASE
        db = FirebaseFirestore.DefaultInstance;
        CollectionReference markersRef = db.Collection("markers");
        markersRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            QuerySnapshot snapshot = task.Result;
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                Dictionary<string, object> documentDictionary = document.ToDictionary();
                Vector2d tPos;
                tPos.x = double.Parse(documentDictionary["lat"].ToString());
                tPos.y = double.Parse(documentDictionary["long"].ToString());
                Vector3 tGamePos = _map.GeoToWorldPosition(tPos);
                GameObject tObj = Instantiate(markerObject, tGamePos, Quaternion.identity);

                tObj.GetComponent<MarkerProperties>().mName = document.Id;
                tObj.GetComponent<MarkerProperties>().mDesc = documentDictionary["desc"].ToString();
                tObj.GetComponent<MarkerProperties>().mChar = documentDictionary["char"].ToString();
                tObj.GetComponent<MarkerProperties>().mLat = tPos.x;
                tObj.GetComponent<MarkerProperties>().mLong = tPos.y;
                tObj.SetActive(false);
                _spawnedObjects.Add(tObj);

                // Debug.Log(String.Format("User: {0}", document.Id));
                // Dictionary<string, object> documentDictionary = document.ToDictionary();
                // Debug.Log(String.Format("First: {0}", documentDictionary["First"]));
                // if (documentDictionary.ContainsKey("Middle"))
                // {
                //     Debug.Log(String.Format("Middle: {0}", documentDictionary["Middle"]));
                // }

                // Debug.Log(String.Format("Last: {0}", documentDictionary["Last"]));
                // Debug.Log(String.Format("Born: {0}", documentDictionary["Born"]));

                // ADD TO LISTMANAGER //////////////////////////////////////////////////////////////////////////////////////
                listManager.addToList(document.Id, documentDictionary["desc"].ToString(), m_DropOptions[int.Parse(documentDictionary["char"].ToString())], tPos);
            }
            listManager.generateList();
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // Debug.Log(snapshot.Count);

        });

        // TOAST ORIGINAL COLOR ////////////////////////////////////////////////////////////////////////////////////////////
        currentColor = txt.color;
        txt.transform.parent.gameObject.SetActive(true);
        parentColor = txt.GetComponentInParent<Image>().color;
        txt.transform.parent.gameObject.SetActive(false);
    }
    Color currentColor;
    Color parentColor;
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    [SerializeField]
    private GameObject menu;
    public GameObject player;
    public GameObject markerObject;
    public GameObject bgProperties;
    public GameObject bgEdit;
    public GameObject listWindow;

    private void Awake()
    {
        Assert.IsNotNull(menu);
    }

    public void toggleMenu()
    {
        menu.SetActive(!menu.activeSelf);
    }

    public TMPro.TMP_InputField propName;
    public TMPro.TMP_InputField propDesc;
    public TMPro.TMP_Dropdown propChar;
    public void showProperties()
    {
        bgProperties.SetActive(true);
        GameObject.Find("Properties Window").GetComponent<QuantumTek.QuantumUI.QUI_Window>().SetActive(true);

        // GET PROPERTIES
        propName.text = "";
        propDesc.text = "";
        propChar.value = 0;
    }

    public void closeProperties()
    {
        bgProperties.SetActive(false);
        GameObject.Find("Properties Window").GetComponent<QuantumTek.QuantumUI.QUI_Window>().SetActive(false);
    }

    private string oldName = "";
    public TMPro.TMP_InputField editName;
    public TMPro.TMP_InputField editDesc;
    public TMPro.TMP_Dropdown editChar;
    public void showEdit()
    {
        bgEdit.SetActive(true);
        GameObject.Find("Edit Window").GetComponent<QuantumTek.QuantumUI.QUI_Window>().SetActive(true);

        // GET PROPERTIES
        MarkerProperties temp = editObject.GetComponent<MarkerProperties>();
        editName.text = temp.mName;
        editDesc.text = temp.mDesc;
        editChar.value = int.Parse(temp.mChar);
        oldName = temp.mName;
    }

    public void closeEdit()
    {
        bgEdit.SetActive(false);
        GameObject.Find("Edit Window").GetComponent<QuantumTek.QuantumUI.QUI_Window>().SetActive(false);
    }

    public void showList()
    {
        listWindow.SetActive(true);
        menu.SetActive(false);
    }

    public void closeList()
    {
        listWindow.SetActive(false);
    }

    private Vector3 tTarget = Vector3.zero;

    public void spawnMarkerAtPlayer()
    {
        tTarget = player.transform.position;
        showProperties();
    }

    public void saveMarker()
    {
        // CHECK NAME ALREADY EXIST
        if (listManager.listContains(propName.text))
        {
            showToast("Name already used", 1);
        }
        else
        {
            GameObject tObj = Instantiate(markerObject, tTarget, Quaternion.identity);
            Vector2d tPos = _map.WorldToGeoPosition(tTarget);
            tObj.GetComponent<MarkerProperties>().mName = propName.text;
            tObj.GetComponent<MarkerProperties>().mDesc = propDesc.text;
            tObj.GetComponent<MarkerProperties>().mChar = propChar.value.ToString();
            tObj.GetComponent<MarkerProperties>().mLat = tPos.x;
            tObj.GetComponent<MarkerProperties>().mLong = tPos.y;
            _spawnedObjects.Add(tObj);

            // SAVE TO LIST
            listManager.addToList(propName.text, propDesc.text, m_DropOptions[int.Parse(propChar.value.ToString())], tPos);
            listManager.generateList();

            // SAVE TO DATABASE
            DocumentReference docRef = db.Collection("markers").Document(propName.text);
            Dictionary<string, object> markerDict = new Dictionary<string, object>
            {
                { "lat", tPos.x },
                { "long", tPos.y },
                { "desc", propDesc.text },
                { "char", propChar.value.ToString() },
            };
            docRef.SetAsync(markerDict).ContinueWithOnMainThread(task =>
            {
                Debug.Log("Data Added");
            });
            closeProperties();
            propName.text = "";
            propDesc.text = "";
            propChar.value = 0;
        }
    }

    public void saveEdit()
    {
        // CHECK NAME ALREADY EXIST
        if (editName.text != oldName && listManager.listContains(editName.text))
        {
            showToast("Name already used", 1);
        }
        else
        {
            // SET NEW PROPERTIES
            MarkerProperties temp = editObject.GetComponent<MarkerProperties>();
            temp.mName = editName.text;
            temp.mDesc = editDesc.text;
            temp.mChar = editChar.value.ToString();

            // MAKE TEMP POS
            Vector2d tPos = new Vector2d(editObject.GetComponent<MarkerProperties>().mLat, editObject.GetComponent<MarkerProperties>().mLong);

            // EDIT FROM LIST
            listManager.deleteFromList(oldName);
            listManager.addToList(temp.mName, temp.mDesc, m_DropOptions[int.Parse(editChar.value.ToString())], tPos);
            listManager.generateList();

            // EDIT FROM DATABASE
            DocumentReference markerRef = db.Collection("markers").Document(oldName);
            markerRef.DeleteAsync().ContinueWithOnMainThread(task =>
            {
                DocumentReference docRef = db.Collection("markers").Document(temp.mName);
                Dictionary<string, object> markerDict = new Dictionary<string, object>
                {
                { "lat", temp.mLat },
                { "long", temp.mLong },
                { "desc", temp.mDesc },
                { "char", temp.mChar },
                };
                docRef.SetAsync(markerDict);
            });

            closeEdit();
            editName.text = "";
            editDesc.text = "";
            editChar.value = 0;
        }
    }

    [SerializeField]
    AbstractMap _map;
    private bool addState = false;

    public void spawnMarkerAtCustom()
    {
        if (addState)
        {
            addState = false;
            GameObject.Find("btnAddCustom").GetComponent<Image>().color = new Color32(255, 255, 255, 192);
        }
        else
        {
            addState = true;
            GameObject.Find("btnAddCustom").GetComponent<Image>().color = new Color32(101, 181, 247, 255);
            editState = false;
            GameObject.Find("btnEdit").GetComponent<Image>().color = new Color32(255, 255, 255, 192);
            delState = false;
            GameObject.Find("btnDelete").GetComponent<Image>().color = new Color32(255, 255, 255, 192);
        }
    }
    private bool delState = false;
    public void despawnMarker()
    {
        if (delState)
        {
            delState = false;
            GameObject.Find("btnDelete").GetComponent<Image>().color = new Color32(255, 255, 255, 192);
        }
        else
        {
            delState = true;
            GameObject.Find("btnDelete").GetComponent<Image>().color = new Color32(101, 181, 247, 255);
            addState = false;
            GameObject.Find("btnAddCustom").GetComponent<Image>().color = new Color32(255, 255, 255, 192);
            editState = false;
            GameObject.Find("btnEdit").GetComponent<Image>().color = new Color32(255, 255, 255, 192);

        }
    }

    private bool editState = false;
    public void editMarker()
    {
        if (editState)
        {
            editState = false;
            GameObject.Find("btnEdit").GetComponent<Image>().color = new Color32(255, 255, 255, 192);
        }
        else
        {
            editState = true;
            GameObject.Find("btnEdit").GetComponent<Image>().color = new Color32(101, 181, 247, 255);
            addState = false;
            GameObject.Find("btnAddCustom").GetComponent<Image>().color = new Color32(255, 255, 255, 192);
            delState = false;
            GameObject.Find("btnDelete").GetComponent<Image>().color = new Color32(255, 255, 255, 192);
        }
    }

    public bool getState()
    {
        if (addState || editState || delState)
            return true;
        else
            return false;
    }

    private GameObject editObject;
    private string delName = "";

    void Update()
    {
        if (addState && !IsPointerOverUIObject() && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                tTarget = hit.point;
                showProperties();

                addState = false;
                GameObject.Find("btnAddCustom").GetComponent<Image>().color = new Color32(255, 255, 255, 192);
            }
        }
        else if (editState && !IsPointerOverUIObject() && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject temp = hit.transform.root.gameObject;
                if (temp.tag == "Marker")
                {
                    editObject = temp;
                    showEdit();

                    editState = false;
                    GameObject.Find("btnEdit").GetComponent<Image>().color = new Color32(255, 255, 255, 192);
                }
            }
        }
        else if (delState && !IsPointerOverUIObject() && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject temp = hit.transform.root.gameObject;
                if (temp.tag == "Marker")
                {
                    if (delName == temp.GetComponent<MarkerProperties>().mName)
                    {
                        _spawnedObjects.Remove(temp);
                        temp.Destroy();

                        delState = false;
                        GameObject.Find("btnDelete").GetComponent<Image>().color = new Color32(255, 255, 255, 192);

                        // DELETE FROM LIST
                        listManager.deleteFromList(delName);
                        listManager.generateList();

                        // DELETE FROM DATABASE
                        DocumentReference markerRef = db.Collection("markers").Document(delName);
                        markerRef.DeleteAsync();
                    }
                    else
                    {
                        showToast("Select Again to Delete", 1);
                        delName = temp.GetComponent<MarkerProperties>().mName;
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (bgProperties.activeSelf)
                closeProperties();
            else if (bgEdit.activeSelf)
                closeEdit();
            else if (listWindow.activeSelf)
                closeList();
            else if (helpBG.activeSelf)
                toggleHelp();
            else
                toggleMenu();
        }

        Location currLoc = _locationProvider.CurrentLocation;
        if (_spawnedObjects != null && !currLoc.LatitudeLongitude.Equals(Vector2d.zero))
        {
            for (int i = 0; i < _spawnedObjects.Count; i++)
            {
                _spawnedObjects[i].SetActive(true);
                Vector2d location = new Vector2d(_spawnedObjects[i].GetComponent<MarkerProperties>().mLat, _spawnedObjects[i].GetComponent<MarkerProperties>().mLong);
                _spawnedObjects[i].transform.localPosition = _map.GeoToWorldPosition(location, true);
            }
        }
    }

    // TOAST FROM https://stackoverflow.com/questions/52590525/how-to-show-a-toast-message-in-unity-similar-to-one-in-android
    [SerializeField] private Text txt;

    public void showToast(string text, int duration)
    {
        StopCoroutine(showToastCOR(text, duration));
        StartCoroutine(showToastCOR(text, duration));
    }

    private IEnumerator showToastCOR(string text, int duration)
    {
        StopCoroutine(fadeInAndOut(txt, false, 0.5f));
        StopCoroutine(fadeInAndOut(txt, true, 0.5f));
        txt.transform.parent.gameObject.SetActive(true);

        txt.text = text;
        txt.enabled = true;

        //Fade in
        yield return fadeInAndOut(txt, true, 0.5f);

        //Wait for the duration
        float counter = 0;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            yield return null;
        }

        //Fade out
        yield return fadeInAndOut(txt, false, 0.5f);

        txt.enabled = false;
        txt.transform.parent.gameObject.SetActive(false);
        delName = "";
    }

    IEnumerator fadeInAndOut(Text targetText, bool fadeIn, float duration)
    {
        //Set Values depending on if fadeIn or fadeOut
        float a, b, c, d;
        if (fadeIn)
        {
            a = 0f;
            b = currentColor.a;
            c = 0f;
            d = parentColor.a;
        }
        else
        {
            a = currentColor.a;
            b = 0f;
            c = parentColor.a;
            d = 0f;
        }
        float counter = 0f;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(a, b, counter / duration);
            float alphaParent = Mathf.Lerp(c, d, counter / duration);

            targetText.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            targetText.GetComponentInParent<Image>().color = new Color(parentColor.r, parentColor.g, parentColor.b, alphaParent);
            yield return null;
        }
    }

}
