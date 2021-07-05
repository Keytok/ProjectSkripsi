using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.IO;
using Dummiesman;

public class ARUIManager : MonoBehaviour
{
    [SerializeField] private GameObject pnlPose;
    [SerializeField] private GameObject pnlTransform;
    [SerializeField] private GameObject pnlChange;
    [SerializeField] private TextMeshProUGUI txtFPS;
    [SerializeField] private GameObject dLight;
    [SerializeField] private GameObject arSessionOrigin;
    [SerializeField] private GameObject pnlBackground;
    [SerializeField] private Material plnTrans;
    [SerializeField] private Material plnSemi;
    [SerializeField] private MeshRenderer plnVisualizer;
    [SerializeField] private GameObject helpBG;
    [SerializeField] private QuantumTek.QuantumUI.QUI_Window helpWindow;

    public void toggleHelp()
    {
        helpWindow.SetActive(!helpBG.activeSelf);
        helpBG.SetActive(!helpBG.activeSelf);
    }

    private void Awake()
    {
        Assert.IsNotNull(pnlPose);
    }

    public void showLoadDialog()
    {
        pnlBackground.SetActive(true);
        // Set filters (optional)
        // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
        // if all the dialogs will be using the same filters
        // FileBrowser.SetFilters( true, new FileBrowser.Filter( "Images", ".jpg", ".png" ), new FileBrowser.Filter( "Text Files", ".txt", ".pdf" ) );
        FileBrowser.SetFilters(true, new FileBrowser.Filter("OBJ Files", ".obj"));

        // Set default filter that is selected when the dialog is shown (optional)
        // Returns true if the default filter is set successfully
        // In this case, set Images filter as the default filter
        FileBrowser.SetDefaultFilter(".obj");

        // Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
        // Note that when you use this function, .lnk and .tmp extensions will no longer be
        // excluded unless you explicitly add them as parameters to the function
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

        // Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
        // It is sufficient to add a quick link just once
        // Name: Users
        // Path: C:\Users
        // Icon: default (folder icon)
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);
        FileBrowser.AddQuickLink("Models", "D:\\Kevin\\Documents\\TA\\Unity\\Models", null);

        // Show a save file dialog 
        // onSuccess event: not registered (which means this dialog is pretty useless)
        // onCancel event: not registered
        // Save file/folder: file, Allow multiple selection: false
        // Initial path: "C:\", Initial filename: "Screenshot.png"
        // Title: "Save As", Submit button text: "Save"
        // FileBrowser.ShowSaveDialog( null, null, FileBrowser.PickMode.Files, false, "C:\\", "Screenshot.png", "Save As", "Save" );

        // Show a select folder dialog 
        // onSuccess event: print the selected folder's path
        // onCancel event: print "Canceled"
        // Load file/folder: folder, Allow multiple selection: false
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Select Folder", Submit button text: "Select"
        // FileBrowser.ShowLoadDialog( ( paths ) => { Debug.Log( "Selected: " + paths[0] ); },
        //						   () => { Debug.Log( "Canceled" ); },
        //						   FileBrowser.PickMode.Folders, false, null, null, "Select Folder", "Select" );

        // Coroutine example
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    public GameObject textModel;
    private void updateTextChar()
    {
        textModel.GetComponent<TextMeshProUGUI>().text = listCharName[currentModel];
        setAnim();
    }
    private GameObject loadedObj;
    public GameObject getLoadedObj()
    {
        return loadedObj;
    }
    public UnityEngine.XR.ARFoundation.Samples.PlaceOnPlane scriptPOP;
    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: files, Allow multiple selection: false
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Load Files and Folders", "Load");

        // Dialog is closed
        // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
            for (int i = 0; i < FileBrowser.Result.Length; i++)
                Debug.Log(FileBrowser.Result[i]);

            // // Read the bytes of the first file via FileBrowserHelpers
            // // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            // byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);

            // // Or, copy the first file to persistentDataPath
            // string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
            // FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationPath);

            // IMPORT OBJ
            string filePath = FileBrowser.Result[0];
            if (Application.platform != RuntimePlatform.Android || filePath.Contains("/storage/emulated/0"))
            {
                if (loadedObj != null)
                {
                    listCharName.RemoveAt(characterModels.Length);
                    loadedObj.Destroy();
                }
                loadedObj = new OBJLoader().Load(filePath);
                loadedObj.transform.localScale = new Vector3(1, 1, 1);
                loadedObj.SetActive(false);

                // SET POP CHAR
                scriptPOP.updateObj();

                // SET EDITOR CHAR
                Transform temp = character.transform;
                if (character.activeSelf)
                {
                    character.Destroy();
                    character = Instantiate(loadedObj, temp.position, temp.rotation);
                    character.SetActive(true);
                }
                else
                {
                    character.Destroy();
                    character = Instantiate(loadedObj, temp.position, temp.rotation);
                    character.SetActive(false);
                }
                currentModel = characterModels.Length;
                listCharName.Add(loadedObj.name);
                updateTextChar();
            }
            else
            {
                showToast("Please choose files from internal storage", 1);
            }

        }
    }

    private bool transparentState = true;

    public void turnHintOn(bool planeHint)
    {
        if (planeHint)
        {
            MeshRenderer[] rends = arSessionOrigin.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < rends.Length; i++)
            {
                rends[i].material = plnSemi;
            }
            plnVisualizer.material = plnSemi;
        }
        else
        {
            if (transparentState)
            {
                MeshRenderer[] rends = arSessionOrigin.GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < rends.Length; i++)
                {
                    rends[i].material = plnTrans;
                }
                plnVisualizer.material = plnTrans;
            }
            else
            {
                MeshRenderer[] rends = arSessionOrigin.GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < rends.Length; i++)
                {
                    rends[i].material = plnSemi;
                }
                plnVisualizer.material = plnSemi;
            }
        }
    }

    public void toggleDebug()
    {
        txtFPS.gameObject.SetActive(!txtFPS.gameObject.activeSelf);
        dLight.GetComponent<UnityEngine.XR.ARFoundation.Samples.HDRLightEstimation>().toggleUseArrow();
        if (transparentState)
        {
            // arSessionOrigin.GetComponent<UnityEngine.XR.ARFoundation.ARPlaneManager>().planePrefab = plnSemi;
            transparentState = false;
        }
        else
        {
            // arSessionOrigin.GetComponent<UnityEngine.XR.ARFoundation.ARPlaneManager>().planePrefab = plnTrans;
            transparentState = true;
        }
    }

    public void togglePose()
    {
        if (pnlPose.activeSelf)
        {
            pnlPose.SetActive(false);
            GameObject.Find("btnPose").GetComponent<Image>().color = Color.white;
        }
        else
        {
            pnlPose.SetActive(true);
            GameObject.Find("btnPose").GetComponent<Image>().color = new Color32(101, 181, 247, 255);
            pnlTransform.SetActive(false);
            GameObject.Find("btnTransform").GetComponent<Image>().color = Color.white;
            pnlChange.SetActive(false);
            GameObject.Find("btnChange").GetComponent<Image>().color = Color.white;
        }
    }

    public void toggleTransform()
    {
        if (pnlTransform.activeSelf)
        {
            pnlTransform.SetActive(false);
            GameObject.Find("btnTransform").GetComponent<Image>().color = Color.white;
        }
        else
        {
            pnlTransform.SetActive(true);
            GameObject.Find("btnTransform").GetComponent<Image>().color = new Color32(101, 181, 247, 255);
            pnlPose.SetActive(false);
            GameObject.Find("btnPose").GetComponent<Image>().color = Color.white;
            pnlChange.SetActive(false);
            GameObject.Find("btnChange").GetComponent<Image>().color = Color.white;
        }
    }

    public void toggleChange()
    {
        if (pnlChange.activeSelf)
        {
            pnlChange.SetActive(false);
            GameObject.Find("btnChange").GetComponent<Image>().color = Color.white;
        }
        else
        {
            pnlChange.SetActive(true);
            GameObject.Find("btnChange").GetComponent<Image>().color = new Color32(101, 181, 247, 255);
            pnlPose.SetActive(false);
            GameObject.Find("btnPose").GetComponent<Image>().color = Color.white;
            pnlTransform.SetActive(false);
            GameObject.Find("btnTransform").GetComponent<Image>().color = Color.white;
        }
    }

    public void gamePause()
    {
        Time.timeScale = 0;
    }

    public void gameResume()
    {
        Time.timeScale = 1;
    }

    // EDITOR ONLY /////////////////////////////////////////////

    [SerializeField] private GameObject character;
    [SerializeField] private GameObject debugPlane;
    [SerializeField] private GameObject[] characterModels = new GameObject[3];

    public void toggleChar()
    {
        debugPlane.SetActive(!debugPlane.activeSelf);
        character.SetActive(!character.activeSelf);
    }

    private Animator anim;
    private AnimatorStateInfo currentState;
    private AnimatorStateInfo previousState;

    public void animNext()
    {
        if (anim != null)
            anim.SetBool("Next", true);
    }

    public void animBack()
    {
        if (anim != null)
            anim.SetBool("Back", true);
    }

    public void rotateClockwise()
    {
        character.transform.Rotate(0.0f, 15, 0.0f, Space.World);
    }

    public void rotateCounterClockwise()
    {
        character.transform.Rotate(0.0f, -15, 0.0f, Space.World);
    }

    public List<string> listCharName;

    private int currentModel = 0;
    private void setAnim()
    {
        anim = null;
        anim = character.GetComponent<Animator>();
        if (anim != null)
        {
            currentState = anim.GetCurrentAnimatorStateInfo(0);
            previousState = currentState;
        }
    }

    private void instantiateNextChar()
    {
        Transform temp = character.transform;
        character.Destroy();
        if (currentModel >= characterModels.Length - 1 && loadedObj == null || currentModel >= characterModels.Length && loadedObj != null)
            currentModel = -1;
        if (++currentModel >= characterModels.Length)
            character = Instantiate(loadedObj, temp.position, temp.rotation);
        else
            character = Instantiate(characterModels[currentModel], temp.position, temp.rotation);
        character.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }
    public void charNext()
    {
        if (character.activeSelf)
        {
            instantiateNextChar();
            character.SetActive(true);
        }
        else
        {
            instantiateNextChar();
            character.SetActive(false);
        }
        updateTextChar();
    }

    private void instantiatePrevChar()
    {
        Transform temp = character.transform;
        character.Destroy();
        if (currentModel <= 0 && loadedObj == null)
            currentModel = characterModels.Length;
        else if (currentModel <= 0 && loadedObj != null)
            currentModel = characterModels.Length + 1;
        if (--currentModel >= characterModels.Length)
            character = Instantiate(loadedObj, temp.position, temp.rotation);
        else
            character = Instantiate(characterModels[currentModel], temp.position, temp.rotation);
        character.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }
    public void charBack()
    {
        if (character.activeSelf)
        {
            instantiatePrevChar();
            character.SetActive(true);
        }
        else
        {
            instantiatePrevChar();
            character.SetActive(false);
        }
        updateTextChar();
    }

    public void scaleUp()
    {
        Vector3 temp = character.transform.localScale;
        // if (temp.x >= 0.095f)
        if (temp.x >= 0.1f)
        {
            temp.x += 0.1f;
            temp.y += 0.1f;
            temp.z += 0.1f;
            character.transform.localScale = temp;
        }
        // else if (temp.x >= 0.0045f)
        else
        {
            temp.x += 0.005f;
            temp.y += 0.005f;
            temp.z += 0.005f;
            character.transform.localScale = temp;
        }
    }

    public void scaleDown()
    {
        Vector3 temp = character.transform.localScale;
        if (temp.x >= 0.15f)
        {
            temp.x -= 0.1f;
            temp.y -= 0.1f;
            temp.z -= 0.1f;
            character.transform.localScale = temp;
        }
        // else if (temp.x >= 0.075f)
        else if (temp.x >= 0.0055f)
        {
            temp.x -= 0.005f;
            temp.y -= 0.005f;
            temp.z -= 0.005f;
            character.transform.localScale = temp;
        }
        else
        {
            temp.x = 0.005f;
            temp.y = 0.005f;
            temp.z = 0.005f;
            character.transform.localScale = temp;
        }
    }
    ////////////////////////////////////////////////////////////

    private UnityEngine.XR.ARFoundation.LightEstimation leState;

    void Start()
    {
        updateTextChar();

        leState = arSessionOrigin.transform.GetChild(0).GetComponent<UnityEngine.XR.ARFoundation.ARCameraManager>().requestedLightEstimation;
        turnHintOn(true);

        // SET DEFAULT CHAR
        GameObject adminManager = GameObject.Find("Admin Manager");
        if (adminManager != null)
        {
            int tCurrent = adminManager.GetComponent<AdminManager>().getDefaultChar();
            if (tCurrent != currentModel)
            {
                currentModel = tCurrent;
                Transform temp = character.transform;
                character.Destroy();
                character = Instantiate(characterModels[currentModel], temp.position, temp.rotation);
                character.SetActive(false);
                updateTextChar();
            }
        }

        // START TOAST //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        currentColor = txt.color;
        txt.transform.parent.gameObject.SetActive(true);
        parentColor = txt.GetComponentInParent<Image>().color;
        txt.transform.parent.gameObject.SetActive(false);
    }
    Color currentColor;
    Color parentColor;
    [SerializeField] private Text txt;

    void showToast(string text, int duration)
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
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void toggleLE()
    {
        UnityEngine.XR.ARFoundation.ARCameraManager temp = arSessionOrigin.transform.GetChild(0).GetComponent<UnityEngine.XR.ARFoundation.ARCameraManager>();
        if (temp.requestedLightEstimation == leState)
        {
            temp.requestedLightEstimation = UnityEngine.XR.ARFoundation.LightEstimation.None;
            dLight.transform.rotation = Quaternion.Euler(90, 0, 0);
            dLight.GetComponent<Light>().intensity = 1;
        }
        else
            temp.requestedLightEstimation = leState;
        Debug.Log(temp.requestedLightEstimation);
    }

    private bool escNotPressed = true;

    public void toggleEsc()
    {
        if (escNotPressed)
        {
            escNotPressed = false;
            gamePause();
            GameObject.Find("AR Session").GetComponent<UnityEngine.XR.ARFoundation.ARSession>().enabled = false;
            GameObject.Find("Pause Window").GetComponent<QuantumTek.QuantumUI.QUI_Window>().SetActive(true);
            pnlBackground.SetActive(true);
        }
        else
        {
            escNotPressed = true;
            gameResume();
            GameObject.Find("AR Session").GetComponent<UnityEngine.XR.ARFoundation.ARSession>().enabled = true;
            GameObject.Find("Pause Window").GetComponent<QuantumTek.QuantumUI.QUI_Window>().SetActive(false);
            pnlBackground.SetActive(false);
            frameSum = 0;
            frameCounted = 0;
            maxFrameRate = 0;
            minFrameRate = 500;
        }
    }

    private void handleEditorAnim()
    {
        if (anim != null)
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
    }

    private float frameSum = 0;
    private int frameCounted = 0;
    private int maxFrameRate = 0;
    private int minFrameRate = 500;
    void Update()
    {
        if (pnlBackground.activeSelf && escNotPressed && FileBrowser.IsOpen == false)
            pnlBackground.SetActive(false);

        if (character.activeSelf || scriptPOP.spawnedObject != null)
        {
            GameObject.Find("btnTransform").GetComponent<Button>().interactable = true;

            if (scriptPOP.getAnim() != null || anim != null)
            {
                GameObject.Find("btnPose").GetComponent<Button>().interactable = true;
                handleEditorAnim();
            }
            else
            {
                GameObject.Find("btnPose").GetComponent<Button>().interactable = false;
            }

        }
        else
        {
            if (pnlPose.activeSelf)
                togglePose();
            GameObject.Find("btnPose").GetComponent<Button>().interactable = false;

            if (pnlTransform.activeSelf)
                toggleTransform();
            GameObject.Find("btnTransform").GetComponent<Button>().interactable = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (tabGroupContent.activeSelf)
            {
                settingsWindow.SetActive(false);
                pauseWindow.SetActive(true);
            }
            else if (windowContent.activeSelf)
            {
                confirmWindow.SetActive(false);
                pauseWindow.SetActive(true);
            }
            else if (FileBrowser.IsOpen)
            {
                pnlBackground.SetActive(false);
                FileBrowser.HideDialog(true);
            }
            else if (helpBG.activeSelf)
                toggleHelp();
            else
                toggleEsc();
        }

        if (txtFPS.gameObject.activeSelf && Time.timeScale == 1)
        {
            float fps = 1 / Time.unscaledDeltaTime;
            txtFPS.text = $"FPS: {Mathf.Ceil(fps)}";
            if (fps > maxFrameRate)
                maxFrameRate = (int)fps;
            if (fps < minFrameRate)
                minFrameRate = (int)fps;
            frameSum += fps;
            frameCounted++;
            txtFPS.text += "\nAVG: " + (frameSum / frameCounted).ToString("f2");
            txtFPS.text += "\nMAX: " + maxFrameRate;
            txtFPS.text += "\nMIN: " + minFrameRate;

            // System.Diagnostics.PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            // Debug.Log(ramCounter.NextValue() + "MB");
        }

    }
    [SerializeField] private GameObject tabGroupContent;
    [SerializeField] private GameObject windowContent;
    [SerializeField] private QuantumTek.QuantumUI.QUI_Window pauseWindow;
    [SerializeField] private QuantumTek.QuantumUI.QUI_Window confirmWindow;
    [SerializeField] private QuantumTek.QuantumUI.QUI_TabGroup settingsWindow;

}
