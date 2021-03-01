using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using LCPrinter;

public class UIController : MonoBehaviour
{
    public GameObject[] countdowns;
    public GameObject countdown;
    public GameObject screenShotPreview;
    public GameObject popupSendMail;
    public GameObject popup;
    public InputField inputFieldEmail;
    public CanvasGroup canvasGroup;
    public GameObject videoPlayer;
    public GameObject[] frames;
    public GameObject[] jointOverlays;
    public Sprite[] spritesButton;
    public Animator animButtonFrame;
    public Image currentFrameImage;
    public VirtualKeyboard vk;
    public GameObject toast;
    public Text messageToast;
    public string printerName = "";
    public GameObject borderImagePrint;
    public CanvasGroup popupPrint, handCanvasGroup;

    //   public Camera camOV;

    //   private int w, h;
    private Texture2D screenshotTexture;
    private string screenShotName, screensPath;
    private bool isTracking = false;
    private int currentFrame;
    private string emailName = "";
    string path;
    private const string m_URL = "http://171.244.143.150:9999/api/send/email";
    byte[] btScreenShot;
    void Start()
    {
        // w = camOV.targetTexture.width;
        // h = camOV.targetTexture.height;
        currentFrame = 0;
        path = Application.dataPath + "/ScreenShot";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        if (PlayerData.Load())
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("Error. Check internet connection!");
            }
            else StartCoroutine(SendOldImage());
        }

    }


    private IEnumerator SendOldImage()
    {
        for (int i = 0; i < PlayerData.players.Count; i++)
        {
            WWWForm form = new WWWForm();
            form.AddField("email", PlayerData.players[i].emailName);
            form.AddField("subject", "Boo");
            form.AddBinaryData("file", System.IO.File.ReadAllBytes(PlayerData.players[i].imagePath), "ScreenShot.jpg", "image/jpg");

            UnityWebRequest request = UnityWebRequest.Post(m_URL, form);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
            else
            {
                ResponseBody responsebody = JsonUtility.FromJson<ResponseBody>(request.downloadHandler.text);
                int index = PlayerData.players[i].sendCount + 1;
                if (responsebody.error_code == 0 || index >= 5)
                {
                    File.Delete(PlayerData.players[i].imagePath);
                    PlayerData.RemovePlayer(i);
                    PlayerData.Save();
                    i--;
                    Debug.Log("remove" + i);
                }
                else
                    PlayerData.players[i].sendCount = index;
            }
        }
    }

    public void ResetUI()
    {
        screenShotPreview.SetActive(false);
        canvasGroup.alpha = 1;
        frames[currentFrame].SetActive(false);
        jointOverlays[currentFrame].SetActive(false);
        currentFrame = Random.Range(0, 3);
        frames[currentFrame].SetActive(true);
        jointOverlays[currentFrame].SetActive(true);
        currentFrameImage.sprite = spritesButton[currentFrame];
        animButtonFrame.Play("ResetButton");
    }
    public void OnClickScreenShot()
    {
        StartCoroutine(CountdownAndMakePhoto());
    }
    IEnumerator CountdownAndMakePhoto()
    {
        countdown.SetActive(true);
        for (int i = 0; i < countdowns.Length; i++)
        {
            countdowns[i].SetActive(true);
            yield return new WaitForSeconds(1);
            countdowns[i].SetActive(false);
        }

        countdown.SetActive(false);
        canvasGroup.alpha = 0;
        Destroy(screenshotTexture);
        // RenderTexture currentRT = RenderTexture.active;
        // RenderTexture.active = camOV.targetTexture;
        // camOV.Render();
        yield return new WaitForEndOfFrame();
        screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture(2);

        // screenshotTexture.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        // screenshotTexture.Apply();
        // RenderTexture.active = currentRT;

        StartCoroutine(ShowScreenShot());
    }
    public void OnClickBackButton()
    {
        popupSendMail.SetActive(false);
        popup.SetActive(false);
        CloseKeyboard();
        screenShotPreview.SetActive(false);
        if (!isTracking)
            StartCoroutine(TurnOnVideo());
    }

    IEnumerator ShowScreenShot()
    {
        System.DateTime now = System.DateTime.Now;
        screenShotName = "Screenshot_" + now.Year + "_" + now.Month + "_" + now.Day + "_" + now.Hour + "_" + now.Minute + "_" + now.Second + ".jpg";
#if UNITY_EDITOR
        screensPath = Application.dataPath + "/" + screenShotName;
#else
        screensPath = path + "/" + screenShotName;
#endif

        btScreenShot = screenshotTexture.EncodeToJPG();
        screenShotPreview.GetComponentInChildren<RawImage>().texture = screenshotTexture;
        screenShotPreview.SetActive(true);
        popupSendMail.SetActive(false);
        popup.SetActive(false);
        canvasGroup.alpha = 1;
        toast.SetActive(false);

        yield return new WaitForSeconds(3f);
        popup.SetActive(true);
        // Turn on OSK


        inputFieldEmail.text = "";
    }

    public void TurnOnPopupEmailAndPrint()
    {
        popupSendMail.SetActive(true);
        popup.SetActive(false);
        CloseKeyboard();
        OpenKeyboard();
    }

    public void OpenKeyboard()
    {
        vk.ShowOnScreenKeyboard();
    }

    public void CloseKeyboard()
    {
        vk.HideOnScreenKeyboard();
    }

    public void InputEmail(string mail)
    {
        emailName = mail;
    }

    public void ShowEmail(bool isShow)
    {
        if (isShow)
            inputFieldEmail.inputType = InputField.InputType.Password;
        else inputFieldEmail.inputType = InputField.InputType.Standard;
        string temp = emailName;
        inputFieldEmail.text = "";

        inputFieldEmail.text = temp;
    }

    public void SendMail()
    {
        if (emailName == null || emailName.Trim().Length == 0)
            return;
        StartCoroutine(SendEmail());
    }

    public void PrintImage()
    {
        //  Print.PrintTexture(btScreenShot, 1, printerName);
        //   ShowToast("Hình ảnh đã gửi đến Email của bạn");
        StartCoroutine(PrintImageRatio());
    }

    IEnumerator PrintImageRatio()
    {
        borderImagePrint.SetActive(true);
        popupPrint.alpha = 0;
        handCanvasGroup.alpha = 0;
        yield return new WaitForEndOfFrame();
        Texture2D tempTex = ScreenCapture.CaptureScreenshotAsTexture(2);
        yield return new WaitForEndOfFrame();
        borderImagePrint.SetActive(false);
        popupPrint.alpha = 1;
        handCanvasGroup.alpha = 1;

        Print.PrintTexture(tempTex.EncodeToJPG(100), 1, printerName);
        //      Print.PrintTexture(btScreenShot, 1, printerName);
        ShowToast("Ảnh của bạn đang được in");
    }

    private IEnumerator SendEmail()
    {
        WWWForm form = new WWWForm();
        form.AddField("email", emailName);
        form.AddField("subject", "Boo");
        form.AddBinaryData("file", btScreenShot, "ScreenShot.jpg", "image/jpg");

        UnityWebRequest request = UnityWebRequest.Post(m_URL, form);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            //    Debug.Log(request.error);
            File.WriteAllBytes(screensPath, btScreenShot);
            ShowToast("Lỗi mạng, Hình ảnh sẽ gửi đến Email của bạn sau");
            Player player = new Player(screensPath, emailName, 0);
            PlayerData.AddPlayer(player);
            PlayerData.Save();
            Debug.Log("Save" + emailName);
        }
        else
        {
            ResponseBody responsebody = JsonUtility.FromJson<ResponseBody>(request.downloadHandler.text);
            // Debug.Log(responsebody.error_code);
            // Debug.Log(responsebody.notification);
            if (responsebody.error_code == 0)
            {
                ShowToast("Hình ảnh đã gửi đến Email của bạn");
            }
            else ShowToast("Lỗi");
        }
    }

    private void DeleteTempImage()
    {
        if (File.Exists(screensPath))
            File.Delete(screensPath);
    }

    public void FindUser()
    {
        if (!isTracking && !popupSendMail.activeInHierarchy)
        {
            videoPlayer.SetActive(false);
            ResetUI();
            isTracking = true;
            StopCoroutine(TurnOnVideo());
        }
    }

    public void LostUser()
    {
        isTracking = false;
        if (!popupSendMail.activeInHierarchy)
            StartCoroutine(TurnOnVideo());
    }
    IEnumerator TurnOnVideo()
    {
        yield return new WaitForSeconds(30);
        if (!isTracking && !popupSendMail.activeInHierarchy)
        {
            videoPlayer.SetActive(true);
            frames[currentFrame].SetActive(false);
            jointOverlays[currentFrame].SetActive(false);
            popupSendMail.SetActive(false);
            toast.SetActive(false);

            CloseKeyboard();
        }

    }

    public void OnClickButtonChoseFrame()
    {
        animButtonFrame.Play("TurnOn");
    }

    public void OnClickButtonFrame(int i)
    {
        // if (currentFrame != i)
        // {
        frames[currentFrame].SetActive(false);
        jointOverlays[currentFrame].SetActive(false);
        currentFrame = i;
        frames[currentFrame].SetActive(true);
        jointOverlays[currentFrame].SetActive(true);
        currentFrameImage.sprite = spritesButton[currentFrame];
        animButtonFrame.Play("TurnOff");
        //     }
    }

    private void ShowToast(string _text)
    {
        toast.SetActive(false);
        toast.SetActive(true);
        messageToast.text = _text;

    }

}
public class ResponseBody
{
    public int error_code = 3;
    public string notification = "Địa chỉ email không hợp lệ";
    public string field = "email";
}
