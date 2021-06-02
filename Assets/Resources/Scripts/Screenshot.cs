using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Screenshot : MonoBehaviour
{
    // PANELS
    public GameObject pnlFlash;
    public GameObject pnlPreview;

    // CANVAS
    public GameObject UI;
    public GameObject canvasPreview;

    public string fileNamePrefix = "Photo";
    public string fileTimeStampFormat = "yyyy-MM-dd_HH-mm-ss";
    public string fileFormat = ".png";
    public string fileFolder = "TA1";
    public float flashDuration = 0.1f;

    private bool isAndroid = true;
    private string myFileName;
    private string tempFileName;

    public void deleteSS()
    {
        if (isAndroid)
            File.Delete(tempFileName);
        else
            File.Delete(myFileName);
        canvasPreview.SetActive(!canvasPreview.activeSelf);
        UI.SetActive(!UI.activeSelf);
    }

    public void saveSS()
    {
        if (isAndroid)
        {
            NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(tempFileName, fileFolder, myFileName, (success, path) => Debug.Log("Media save result: " + success + " " + path));
            Debug.Log("Permission result: " + permission);
            deleteSS();
        }
        else
        {
            canvasPreview.SetActive(!canvasPreview.activeSelf);
            UI.SetActive(!UI.activeSelf);
        }
    }

    public void ShotNow()
    {
        StopCoroutine(SSCapture());
        StartCoroutine(SSCapture());
    }

    IEnumerator SSCapture()
    {
        this.transform.GetComponentInParent<UnityEngine.XR.ARFoundation.Samples.PlaceOnPlane>().setCanPlace(false);

        myFileName = fileNamePrefix + System.DateTime.Now.ToString(fileTimeStampFormat) + fileFormat;
        tempFileName = Application.persistentDataPath + "/" + myFileName;

        UI.SetActive(!UI.activeSelf);
        pnlFlash.SetActive(!pnlFlash.activeSelf);
        yield return new WaitForSeconds(flashDuration);
        pnlFlash.SetActive(!pnlFlash.activeSelf);

        ScreenCapture.CaptureScreenshot(myFileName);
        yield return new WaitForEndOfFrame();
        Debug.Log("A new screenshot was saved at " + tempFileName);
        canvasPreview.SetActive(!canvasPreview.activeSelf);

        this.transform.GetComponentInParent<UnityEngine.XR.ARFoundation.Samples.PlaceOnPlane>().setCanPlace(true);

        // PREVIEW
        Texture2D texture = null;
        byte[] fileBytes;

        while (true)
        {
            if (File.Exists(tempFileName))
            {
                fileBytes = File.ReadAllBytes(tempFileName);
                texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
                texture.LoadImage(fileBytes);

                Sprite sp = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
                pnlPreview.GetComponent<Image>().sprite = sp;

                isAndroid = true;
                break;
            }
            else if (File.Exists(myFileName))
            {
                fileBytes = File.ReadAllBytes(myFileName);
                texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
                texture.LoadImage(fileBytes);

                Sprite sp = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
                pnlPreview.GetComponent<Image>().sprite = sp;

                isAndroid = false;
                break;
            }
            else
            {
                Debug.Log("File doesnt exist yet");
            }
            yield return new WaitForSeconds(0.05f);
        }

    }
}
