using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    [SerializeField] private GameObject creditBG;
    [SerializeField] private QuantumTek.QuantumUI.QUI_Window creditWindow;
    [SerializeField] private GameObject pauseBG;
    [SerializeField] private GameObject pauseResume;
    [SerializeField] private QuantumTek.QuantumUI.QUI_Window pauseWindow;
    [SerializeField] private QuantumTek.QuantumUI.QUI_Window confirmWindow;
    [SerializeField] private QuantumTek.QuantumUI.QUI_TabGroup settingsWindow;

    public void toggleMenu()
    {
        pauseWindow.SetActive(!pauseBG.activeSelf);
        pauseBG.SetActive(!pauseBG.activeSelf);
        // GameObject.Find("").GetComponent<Image>().color = new Color32(42, 209, 255, 255);
    }

    public void toggleCredit()
    {
        creditWindow.SetActive(!creditBG.activeSelf);
        creditBG.SetActive(!creditBG.activeSelf);
    }

    private bool settingState = true;
    public void toggleSettings()
    {
        settingsWindow.SetActive(settingState);
        pauseWindow.SetActive(!settingState);
        pauseResume.SetActive(!settingState);
        settingState = !settingState;
    }

    private bool confrimState = true;
    public void toggleConfirm()
    {
        confirmWindow.SetActive(confrimState);
        pauseWindow.SetActive(!confrimState);
        pauseResume.SetActive(!confrimState);
        confrimState = !confrimState;
    }

    public void toggleAdmin()
    {
        GameObject.Find("Admin Manager").GetComponent<AdminManager>().setMode(adminToggle.GetComponent<Toggle>().isOn);
    }

    public GameObject adminToggle;
    public GameObject adminText;

    void Start()
    {
        AdminManager adminManager = GameObject.Find("Admin Manager").GetComponent<AdminManager>();
        if (adminManager.isAdmin)
        {
            adminToggle.SetActive(true);
            adminText.SetActive(false);
            if (adminManager.getMode())
            {   
                adminToggle.GetComponent<Toggle>().isOn = true;
                adminManager.setMode(true);
            }
            else
            {
                adminToggle.GetComponent<Toggle>().isOn = false;
                adminManager.setMode(false);
            }
        }
        else
        {
            adminToggle.SetActive(false);
            adminText.SetActive(true);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!settingState)
                toggleSettings();
            else if (!confrimState)
                toggleConfirm();
            else if (creditBG.activeSelf)
                toggleCredit();
            else
                toggleMenu();
        }
    }
}
