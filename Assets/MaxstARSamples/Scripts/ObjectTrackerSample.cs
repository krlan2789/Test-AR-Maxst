/*==============================================================================
Copyright 2017 Maxst, Inc. All Rights Reserved.
==============================================================================*/

using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using maxstAR;

public class ObjectTrackerSample : ARBehaviour
{
	private Dictionary<string, ObjectTrackableBehaviour> objectTrackablesMap =
	new Dictionary<string, ObjectTrackableBehaviour>();

	private CameraBackgroundBehaviour cameraBackgroundBehaviour = null;

	void Awake()
    {
		Init();

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

		StartCamera();
        TrackerManager.GetInstance().StartTracker(TrackerManager.TRACKER_TYPE_OBJECT);
		StartCoroutine(AddTrackerData());

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

			if(objectTrackablesMap.ContainsKey(trackable.GetName()))
            {
				objectTrackablesMap[trackable.GetName()].OnTrackSuccess(trackable.GetId(), trackable.GetName(),
																   trackable.GetPose());
			}
		}
	}

	void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			TrackerManager.GetInstance().StopTracker();
			StopCamera();
		}
		else
		{
			StartCamera();
			TrackerManager.GetInstance().StartTracker(TrackerManager.TRACKER_TYPE_OBJECT);
			StartCoroutine(AddTrackerData());
		}
	}

	void OnDestroy()
	{
		objectTrackablesMap.Clear();
		TrackerManager.GetInstance().StopTracker();
		TrackerManager.GetInstance().DestroyTracker();
		StopCamera();

        if (ConfigurationScriptableObject.GetInstance().WearableType == WearableCalibration.WearableType.OpticalSeeThrough)
        {
            WearableManager.GetInstance().GetDeviceController().SetStereoMode(false);
        }
    }
}