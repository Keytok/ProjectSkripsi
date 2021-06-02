using Dummiesman;
using System.IO;
using UnityEngine;

public class ObjFromFile : MonoBehaviour
{
    string objPath = string.Empty;
    string error = string.Empty;
    GameObject loadedObject;

    void OnGUI() {
        objPath = GUI.TextField(new Rect(0, 0, 256, 32), objPath);

        GUI.Label(new Rect(0, 0, 256, 32), "Obj Path:");
        if(GUI.Button(new Rect(256, 32, 64, 32), "Load File"))
        {
            //file path
            if (!File.Exists(objPath))
            {
                error = "File doesn't exist.";
            }else{
                if(loadedObject != null)            
                    Destroy(loadedObject);
                loadedObject = new OBJLoader().Load(objPath);
                error = string.Empty;
            }
        }

        if(!string.IsNullOrWhiteSpace(error))
        {
            GUI.color = Color.red;
            GUI.Box(new Rect(0, 64, 256 + 64, 32), error);
            GUI.color = Color.white;
        }
    }

    void Start()
    {
        //file path
        /** Sample OBJ from 
        * https://www.turbosquid.com/3d-models/free-obj-model-ivysaur-pokemon-sample/1136333 
        * Model .mtl file needs editing to add the texture by default
        */
        string filePath = @"Models/wolf/Wolf_One_obj.obj";
        // MTL should be linked in the OBJ file by default, 
        // but if not, you can specify it manually
        // string mtlPath = @"Models/ship/Intergalactic_Spaceship-(Wavefront).mtl";
        
        // var loadedObj = new OBJLoader().Load(filePath, mtlPath);
        var loadedObj = new OBJLoader().Load(filePath);

    }
}
