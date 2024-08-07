﻿/*==============================================================================
Copyright 2017 Maxst, Inc. All Rights Reserved.
==============================================================================*/

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;

using maxstAR;

public class ObjectFusionTrackerSample : ARBehaviour
{
	private Dictionary<string, ObjectTrackableBehaviour> objectTrackablesMap =
	new Dictionary<string, ObjectTrackableBehaviour>();

    private CameraBackgroundBehaviour cameraBackgroundBehaviour = null;

    public GameObject guideView;

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
    }

	void Start()
	{
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        objectTrackablesMap.Clear();
		ObjectTrackableBehaviour[] objectTrackables = FindObjectsOfType<ObjectTrackableBehaviour>();
		foreach (var trackable in objectTrackables)
		{
			objectTrackablesMap.Add(trackable.TrackableName, trackable);
			Debug.Log("Trackable add: " + trackable.TrackableName);
		}

        if (TrackerManager.GetInstance().IsFusionSupported())
        {
            CameraDevice.GetInstance().SetARCoreTexture();
            CameraDevice.GetInstance().SetFusionEnable();
            CameraDevice.GetInstance().Start();
            TrackerManager.GetInstance().StartTracker(TrackerManager.TRACKER_TYPE_OBJECT_FUSION);
            StartCoroutine(AddTrackerData());
        }
        else
        {
            TrackerManager.GetInstance().RequestARCoreApk();
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
    }

    private IEnumerator AddTrackerData()
    {
        yield return new WaitForEndOfFrame();
        foreach (var trackable in objectTrackablesMap)
        {
            if (trackable.Value.TrackerDataFileName.Length == 0)
            {
                continue;
            }

            string realSizeData = null;

            if (trackable.Value.RealSize > 0)
            {
                realSizeData = "{\"object_fusion\":\"set_length\",\"object_name\":\"" + trackable.Value.TrackableName + "\", \"length\":" + trackable.Value.RealSize + "}";
            }

            if (trackable.Value.StorageType == StorageType.AbsolutePath)
            {
                TrackerManager.GetInstance().AddTrackerData(trackable.Value.TrackerDataFileName);
                if(realSizeData != null)
                {
                    TrackerManager.GetInstance().AddTrackerData(realSizeData);
                }
            }
            else if (trackable.Value.StorageType == StorageType.StreamingAssets)
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    List<string> fileList = new List<string>();
                    yield return StartCoroutine(MaxstARUtil.ExtractAssets(trackable.Value.TrackerDataFileName, fileList));
                    TrackerManager.GetInstance().AddTrackerData(fileList[0], false);
                    if (realSizeData != null)
                    {
                        TrackerManager.GetInstance().AddTrackerData(realSizeData);
                    }
                }
                else
                {
                    TrackerManager.GetInstance().AddTrackerData(Application.streamingAssetsPath + "/" + trackable.Value.TrackerDataFileName);
                    if (realSizeData != null)
                    {
                        TrackerManager.GetInstance().AddTrackerData(realSizeData);
                    }
                    
                }
            }
        }
        TrackerManager.GetInstance().LoadTrackerData();
    }


	private void DisableAllTrackables()
	{
		foreach (var trackable in objectTrackablesMap)
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

        TrackingResult trackingResult = state.GetTrackingResult();

        GuideInfo guideInfo = TrackerManager.GetInstance().GetGuideInfo();
        TagAnchor[] anchors = guideInfo.GetTagAnchors();

        for (int i = 0; i < trackingResult.GetCount(); i++)
		{
			Trackable trackable = trackingResult.GetTrackable(i);

			if (!objectTrackablesMap.ContainsKey(trackable.GetName()))
			{
				return;
			}

			objectTrackablesMap[trackable.GetName()].OnTrackSuccess(trackable.GetId(), trackable.GetName(),
																   trackable.GetPose());
		}
	}

	void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			TrackerManager.GetInstance().StopTracker();
		}
		else
		{
            if (TrackerManager.GetInstance().IsFusionSupported())
            {
                CameraDevice.GetInstance().SetARCoreTexture();
				CameraDevice.GetInstance().SetFusionEnable();
				CameraDevice.GetInstance().Start();
                TrackerManager.GetInstance().StartTracker(TrackerManager.TRACKER_TYPE_OBJECT_FUSION);
                StartCoroutine(AddTrackerData());
            }
            else
            {
                TrackerManager.GetInstance().RequestARCoreApk();
            }
		}
	}

	void OnDestroy()
	{
		objectTrackablesMap.Clear();
        CameraDevice.GetInstance().Stop();
		TrackerManager.GetInstance().StopTracker();
		TrackerManager.GetInstance().DestroyTracker();
	}
}