using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdminManager : MonoBehaviour
{
    public bool isAdmin = true;
    private bool currentMode;

    // Start is called before the first frame update
    void Awake()
    {
        if (GameObject.Find("Admin Manager") == null)
        {
            currentMode = isAdmin;
            transform.name = "Admin Manager";
            DontDestroyOnLoad(transform.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool getMode()
    {
        return currentMode;
    }

    public void setMode(bool x)
    {
        currentMode = x;
    }

    public List<string> listCharName;

    public List<string> getCharName()
    {
        return listCharName;
    }

    public int defaultChar = 0;
    public void setDefaultChar(int x)
    {
        defaultChar = x;
    }
    public int getDefaultChar()
    {
        return defaultChar;
    }
}
