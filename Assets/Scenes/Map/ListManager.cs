using System;
using System.Collections;
using System.Collections.Generic;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using UnityEngine;
using UnityEngine.UI;

public static class ButtonExtension
{
    public static void AddEventListener<T>(this Button button, T param, Action<T> OnClick)
    {
        button.onClick.AddListener(delegate ()
        {
            OnClick(param);
        });
    }
}

public class ListManager : MonoBehaviour
{
    [Serializable]
    public struct sMarker : IEquatable<sMarker>
    {
        public string Name;
        public string Description;
        public string Character;
        public Vector2d Position;

        public bool Equals(sMarker other)
        {
            return (this.Name.Equals(other.Name));
        }
    }
    List<sMarker> listMarker = new List<sMarker>();

    public GameObject listWindow;
    public GameObject buttonTemplate;
    public CameraController camController;
    public AbstractMap _map;

    void ItemClicked(int itemIndex)
    {
        // Debug.Log("------------item " + itemIndex + " clicked---------------");
        // Debug.Log("name " + listMarker[itemIndex].Name);
        // Debug.Log("desc " + listMarker[itemIndex].Description);
        listWindow.SetActive(false);
        camController.focusTo(_map.GeoToWorldPosition(listMarker[itemIndex].Position, true));
    }

    public void generateList()
    {
        // DELETE ALL
        foreach (Transform child in transform){
            if (child.name != "btnTemplate")
                GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < listMarker.Count; i++)
        {
            GameObject g = Instantiate(buttonTemplate, transform);
            g.SetActive(true);
            g.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = listMarker[i].Name;
            g.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = listMarker[i].Description;
            g.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = "Character : " + listMarker[i].Character;
            g.GetComponent<Button>().AddEventListener(i, ItemClicked);
        }
    }

    public void addToList(string xName, string xDesc, string xChar, Vector2d xPos)
    {
        sMarker temp;
        temp.Name = xName;
        temp.Description = xDesc;
        temp.Character = xChar;
        temp.Position = xPos;
        listMarker.Add(temp);
    }

    public void deleteFromList(string xName)
    {
        listMarker.Remove(new sMarker { Name = xName });
    }

    public bool listContains(string xName)
    {
        if (listMarker.Contains(new sMarker { Name = xName }))
        {
            return true;
        }
        return false;
    }
}
