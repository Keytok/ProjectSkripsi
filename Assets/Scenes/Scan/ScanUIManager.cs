using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanUIManager : MonoBehaviour
{
    public GameObject buttonGO;
    public TMPro.TextMeshProUGUI textInfo;
    public QuantumTek.QuantumUI.QUI_SceneTransition sceneTransition;

    public void loadScene(string sceneName)
    {
        GameObject adminManager = GameObject.Find("Admin Manager");
        if(adminManager != null)
        {
            AdminManager amComponent = adminManager.GetComponent<AdminManager>();
            amComponent.setDefaultChar(Mathf.RoundToInt(Random.Range(0, amComponent.getCharName().Count - 1)));
        }
        buttonGO.SetActive(false);
        textInfo.text = "Scan the image";
        sceneTransition.LoadScene(sceneName);
    }
}
