/*==============================================================================
Copyright 2017 Maxst, Inc. All Rights Reserved.
==============================================================================*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HomeSceneManager : MonoBehaviour
{
    public RectTransform backgroundTransform;
    public RectTransform topTransform;
    public RectTransform titleTransform;
    public RectTransform scrollTransform;

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        Rect safeArea = Screen.safeArea;
        float screenHeight = backgroundTransform.rect.height;
        float realSafeAreaHeight = safeArea.y / (safeArea.height / screenHeight);

        topTransform.sizeDelta = new Vector2(0, realSafeAreaHeight + topTransform.sizeDelta.y);
        titleTransform.offsetMax = new Vector2(0, -realSafeAreaHeight);
        scrollTransform.offsetMax = new Vector2(0, scrollTransform.offsetMax.y - realSafeAreaHeight);

    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void OnImageTargetClick()
    {
        SceneStackManager.Instance.LoadScene("Home", "ImageTracker");
    }

    public void OnMarkerImageClick()
    {
        SceneStackManager.Instance.LoadScene("Home", "MarkerTracker");
    }

    public void OnInstantImageClick()
    {
        SceneStackManager.Instance.LoadScene("Home", "InstantTracker");
    }

    public void OnObjectTargetClick()
    {
        SceneStackManager.Instance.LoadScene("Home", "ObjectTracker");
    }

    public void OnQR_BarcodeClick()
    {
        SceneStackManager.Instance.LoadScene("Home", "CodeReader");
    }

    public void OnCameraConfigClick()
    {
        SceneStackManager.Instance.LoadScene("Home", "CameraConfiguration");
    }

    public void OnCloudRecognizerClick()
    {
        SceneStackManager.Instance.LoadScene("Home", "CloudRecognizer");
    }

    public void OnQRTrackerClick()
    {
        SceneStackManager.Instance.LoadScene("Home", "QrCodeTracker");
    }

    public void OnVideoTrackerClick()
    {
        SceneStackManager.Instance.LoadScene("Home", "VideoTracker");
    }

    public void OnImageFusionTrackerClick()
    {
        SceneStackManager.Instance.LoadScene("Home", "ImageFusionTracker");
    }

    public void OnInstantFusionTrackerClick()
    {
        SceneStackManager.Instance.LoadScene("Home", "InstantFusionTracker");
    }

    public void OnMarkerFusionTrackerClick()
    {
        SceneStackManager.Instance.LoadScene("Home", "MarkerFusionTracker");
    }

    public void OnQRCodeFusionTrackerClick()
    {
        SceneStackManager.Instance.LoadScene("Home", "QRCodeFusionTracker");
    }

    public void OnObjectFusionTrackerClick()
    {
        SceneStackManager.Instance.LoadScene("Home", "ObjectFusionTracker");
    }

    public void OnSpaceTrackerClick()
    {
        SceneStackManager.Instance.LoadScene("Home", "SpaceTracker");
    }
}