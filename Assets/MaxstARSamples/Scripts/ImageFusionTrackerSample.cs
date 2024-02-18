/*==============================================================================
Copyright 2017 Maxst, Inc. All Rights Reserved.
==============================================================================*/

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using maxstAR;
using System.IO;

public class ImageFusionTrackerSample : ARBehaviour
{
	private Dictionary<string, ImageTrackableBehaviour> imageTrackablesMap =
		new Dictionary<string, ImageTrackableBehaviour>();

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

		imageTrackablesMap.Clear();
		ImageTrackableBehaviour[] imageTrackables = FindObjectsOfType<ImageTrackableBehaviour>();
		foreach (var trackable in imageTrackables)
		{
			imageTrackablesMap.Add(trackable.TrackableName, trackable);
			Debug.Log("Trackable add: " + trackable.TrackableName);
		}

		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
		{
			TrackerManager.GetInstance().StartTracker(TrackerManager.TRACKER_TYPE_IMAGE_FUSION);
			StartCoroutine(AddTrackerData());
		}
		else
		{
			if (TrackerManager.GetInstance().IsFusionSupported())
			{
				CameraDevice.GetInstance().SetARCoreTexture();
				CameraDevice.GetInstance().SetFusionEnable();
				CameraDevice.GetInstance().Start();
				TrackerManager.GetInstance().StartTracker(TrackerManager.TRACKER_TYPE_IMAGE_FUSION);
				StartCoroutine(AddTrackerData());
			}
			else
			{
				TrackerManager.GetInstance().RequestARCoreApk();
			}
		}
    }

	private IEnumerator AddTrackerData()
	{
		yield return new WaitForEndOfFrame();
		foreach (var trackable in imageTrackablesMap)
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
		foreach (var trackable in imageTrackablesMap)
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

		for(int i = 0; i < trackingResult.GetCount(); i++)
		{
			Trackable trackable = trackingResult.GetTrackable(i);
			imageTrackablesMap[trackable.GetName()].OnTrackSuccess(
				trackable.GetId(), trackable.GetName(), trackable.GetPose());
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
					TrackerManager.GetInstance().StartTracker(TrackerManager.TRACKER_TYPE_IMAGE_FUSION);
					StartCoroutine(AddTrackerData());
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
		imageTrackablesMap.Clear();
		CameraDevice.GetInstance().Stop();
		TrackerManager.GetInstance().SetTrackingOption(TrackerManager.TrackingOption.NORMAL_TRACKING);
		TrackerManager.GetInstance().StopTracker();
		TrackerManager.GetInstance().DestroyTracker();
	}
}