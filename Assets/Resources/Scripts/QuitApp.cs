using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitApp : MonoBehaviour
{
    public void QuitAppNow()
    {
        Debug.Log("Quitting Application");
        Application.Quit();
    }
}