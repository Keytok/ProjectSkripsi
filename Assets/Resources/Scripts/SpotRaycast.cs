using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpotRaycast : MonoBehaviour
{
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    public string sceneName = "ARScene";
    public MapUIManager UImanager;

    public QuantumTek.QuantumUI.QUI_SceneTransition sceneTransition;
    public QuantumTek.QuantumUI.QUI_Window confirmWindow;

    void OnCollisionStay(Collision collision)
    {
        if (!UImanager.getState() && !IsPointerOverUIObject() && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (collision.transform.root.gameObject == hit.transform.root.gameObject)
                {
                    GameObject adminManager = GameObject.Find("Admin Manager");
                    if (adminManager != null)
                    {
                        adminManager.GetComponent<AdminManager>().setDefaultChar(int.Parse(hit.transform.root.GetComponent<MarkerProperties>().mChar));
                    }
                    sceneTransition.LoadScene(sceneName);
                }
            }
        }

        if(loadState)
        {
            loadState = false;
            GameObject adminManager = GameObject.Find("Admin Manager");
            if (adminManager != null)
            {
                adminManager.GetComponent<AdminManager>().setDefaultChar(int.Parse(collision.transform.root.GetComponent<MarkerProperties>().mChar));
            }
            confirmWindow.SetActive(true);
        }

    }

    private bool loadState = true;

    // void OnCollisionEnter(Collision collision)
    // {
    //     if (loadState)
    //     {
    //         GameObject adminManager = GameObject.Find("Admin Manager");
    //         if (adminManager != null)
    //         {
    //             adminManager.GetComponent<AdminManager>().setDefaultChar(int.Parse(collision.transform.root.GetComponent<MarkerProperties>().mChar));
    //         }
    //         sceneTransition.LoadScene(sceneName);
    //     }
    // }

    void OnCollisionExit(Collision collision)
    {
        loadState = true;
    }

    public void loadScene()
    {
        sceneTransition.LoadScene(sceneName);
    }

}
