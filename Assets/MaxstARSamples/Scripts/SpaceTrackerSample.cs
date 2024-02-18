/*==============================================================================
Copyright 2017 Maxst, Inc. All Rights Reserved.roomTrackablesMap
==============================================================================*/

using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using maxstAR;
using System.IO;

public class SpaceTrackerSample : ARBehaviour
{
    public GameObject guideView;
    public GameObject alertView;
    public GameObject arCamera;

    public Material occlusionMaterial;
    public List<GameObject> occlusionObjects = new List<GameObject>();
    public bool isOcclusion = true;

    private Dictionary<string, SpaceTrackableBehaviour> spaceTrackablesMap = new Dictionary<string, SpaceTrackableBehaviour>();
    private CameraBackgroundBehaviour cameraBackgroundBehaviour = null;

    void Awake()
    {
        Init();

        AndroidRuntimePermissions.Permission[] result = AndroidRuntimePermissions.RequestPermissions("android.permission.WRITE_EXTERNAL_STORAGE", "android.permission.CAMERA");
        if (result[0] == AndroidRuntimePermissions.Permission.Granted && result[1] == AndroidRuntimePermissions.Permission.Granted)
            Debug.Log("We have all the permissions!");
        else
            Debug.Log("Some permission(s) are not granted...");

        cameraBackgroundBehaviour = FindObjectOfType<CameraBackgroundBehaviour>();
        if (cameraBackgroundBehaviour == null)
        {
            Debug.LogError("Can't find CameraBackgroundBehaviour.");
            return;
        }

        if (alertView != null)
        {
            alertView.SetActive(false);
        }
    }

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        spaceTrackablesMap.Clear();
        SpaceTrackableBehaviour[] RoomTrackables = FindObjectsOfType<SpaceTrackableBehaviour>();

        foreach (var trackable in RoomTrackables)
        {
            if (!string.IsNullOrEmpty(trackable.TrackableName))
            {
                spaceTrackablesMap.Add(trackable.TrackableName, trackable);
                Debug.Log("Trackable add: " + trackable.TrackableName);
            }
        }

        if (spaceTrackablesMap.Count == 0)
        {
            if (alertView != null)
            {
                alertView.SetActive(true);
            }
        }

        if ((Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
            && SimulationController.Instance.SimulationMode)
        {
            TrackerManager.GetInstance().StartTracker(TrackerManager.TRACKER_TYPE_SPACE);
            StartCoroutine(AddTrackerData());
        }
        else
        {
            if (TrackerManager.GetInstance().IsFusionSupported())
            {
                CameraDevice.GetInstance().SetARCoreTexture();
                CameraDevice.GetInstance().SetFusionEnable();
                CameraDevice.GetInstance().Start();
                TrackerManager.GetInstance().StartTracker(TrackerManager.TRACKER_TYPE_SPACE);
                StartCoroutine(AddTrackerData());
            }
            else
            {
                TrackerManager.GetInstance().RequestARCoreApk();
            }
        }

        // For see through smart glass setting
        if (ConfigurationScriptableObject.GetInstance().WearableType == WearableCalibration.WearableType.OpticalSeeThrough)
        {
            WearableManager.GetInstance().GetDeviceController().SetStereoMode(true);

            CameraBackgroundBehaviour cameraBackground = FindObjectOfType<CameraBackgroundBehaviour>();
            cameraBackground.gameObject.SetActive(false);

            WearableManager.GetInstance().GetCalibration().CreateWearableEye(Camera.main.transform);

            // BT-300 screen is splited in half size, but R-7 screen is doubled.
            if (WearableManager.GetInstance().GetDeviceController().IsSideBySideType() == true)
            {
                // Do something here. For example resize gui to fit ratio
            }
        }

        if (isOcclusion)
        {
            foreach (GameObject eachGameObject in occlusionObjects)
            {
                Renderer[] cullingRenderer = eachGameObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer eachRenderer in cullingRenderer)
                {
                    Material[] materials = eachRenderer.materials;
                    for (int i = 0; i < eachRenderer.materials.Length; i++)
                    {
                        materials[i] = occlusionMaterial;
                        materials[i].renderQueue = 1900;
                    }

                    eachRenderer.materials = materials;
                }
            }
        }
    }

    private IEnumerator AddTrackerData()
    {
        yield return new WaitForEndOfFrame();
        foreach (var trackable in spaceTrackablesMap)
        {
            if (trackable.Value.TrackerDataFileName.Length == 0)
            {
                continue;
            }

            if (trackable.Value.StorageType == StorageType.AbsolutePath)
            {
                TrackerManager.GetInstance().AddTrackerData(trackable.Value.TrackerDataFileName);
            }
            else if (trackable.Value.StorageType == StorageType.StreamingAssets)
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    List<string> fileList = new List<string>();
                    yield return StartCoroutine(MaxstARUtil.ExtractAssets(trackable.Value.TrackerDataFileName, fileList));
                    TrackerManager.GetInstance().AddTrackerData(fileList[0], false);
                }
                else
                {
                    TrackerManager.GetInstance().AddTrackerData(Application.streamingAssetsPath + "/" + trackable.Value.TrackerDataFileName);
                }
            }
        }

        TrackerManager.GetInstance().LoadTrackerData();
    }

    private void DisableAllTrackables()
    {
        foreach (var trackable in spaceTrackablesMap)
        {
            trackable.Value.OnTrackFail();
        }
    }

    void Update()
    {
        DisableAllTrackables();

        TrackingState state = TrackerManager.GetInstance().UpdateTrackingState();

        if (state == null)
        {
            return;
        }

        cameraBackgroundBehaviour.UpdateCameraBackgroundImage(state);

        if (guideView != null)
        {
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
            {
                guideView.SetActive(false);
            }
            else
            {
                int fusionState = TrackerManager.GetInstance().GetFusionTrackingState();
                if (fusionState == -1)
                {
                    guideView.SetActive(true);
                    return;
                }
                else
                {
                    guideView.SetActive(false);
                }
            }
        }

        TrackingResult trackingResult = state.GetTrackingResult();

        for (int i = 0; i < trackingResult.GetCount(); i++)
        {
            Trackable trackable = trackingResult.GetTrackable(i);

            if (!spaceTrackablesMap.ContainsKey(trackable.GetName()))
            {
                return;
            }

            spaceTrackablesMap[trackable.GetName()].OnTrackSuccess(trackable.GetId(), trackable.GetName(), trackable.GetPose(arCamera));
        }
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            CameraDevice.GetInstance().Stop();
            TrackerManager.GetInstance().StopTracker();
        }
        else
        {
            if ((Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
                && SimulationController.Instance.SimulationMode)
            {
                string simulatePath = SimulationController.Instance.simulatePath;
                if (Directory.Exists(simulatePath))
                {
                    CameraDevice.GetInstance().Start(simulatePath);
                    MaxstAR.SetScreenOrientation((int)ScreenOrientation.Portrait);
                }
            }
            else
            {
                if (TrackerManager.GetInstance().IsFusionSupported())
                {
                    CameraDevice.GetInstance().SetARCoreTexture();
                    CameraDevice.GetInstance().SetFusionEnable();
                    CameraDevice.GetInstance().Start();
                    TrackerManager.GetInstance().StartTracker(TrackerManager.TRACKER_TYPE_SPACE);
                }
                else
                {
                    TrackerManager.GetInstance().RequestARCoreApk();
                }
            }
        }
    }

    void OnDestroy()
    {
        spaceTrackablesMap.Clear();
        CameraDevice.GetInstance().Stop();
        TrackerManager.GetInstance().StopTracker();
        TrackerManager.GetInstance().DestroyTracker();
    }
}