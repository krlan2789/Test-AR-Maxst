using maxstAR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LAN
{
    public class MenuArManager : ARBehaviour {
        public readonly Dictionary<string, ImageTrackableBehaviour> imageTrackablesMap = new();
        private CameraBackgroundBehaviour cameraBackgroundBehaviour = null;

        private void Awake() {
            Init();

            AndroidRuntimePermissions.Permission[] result = AndroidRuntimePermissions.RequestPermissions("android.permission.WRITE_EXTERNAL_STORAGE", "android.permission.CAMERA");
            if (result[0] == AndroidRuntimePermissions.Permission.Granted && result[1] == AndroidRuntimePermissions.Permission.Granted)
                Debug.Log("We have all the permissions!");
            else
                Debug.Log("Some permission(s) are not granted...");

            cameraBackgroundBehaviour = FindObjectOfType<CameraBackgroundBehaviour>();
            if (cameraBackgroundBehaviour == null) {
                Debug.LogError("Can't find CameraBackgroundBehaviour.");
                return;
            }
        }

        private void Start() {
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 60;

			imageTrackablesMap.Clear();
			ImageTrackableBehaviour[] imageTrackables = FindObjectsOfType<ImageTrackableBehaviour>();
			foreach (var trackable in imageTrackables) {
				imageTrackablesMap.Add(trackable.TrackableName, trackable);
				Debug.Log("Trackable add: " + trackable.TrackableName);
			}

			TrackerManager.GetInstance().StartTracker(TrackerManager.TRACKER_TYPE_IMAGE);
			StartCoroutine(AddTrackerData());

			StartCamera();

			//// For see through smart glass setting
			//if (ConfigurationScriptableObject.GetInstance().WearableType == WearableCalibration.WearableType.OpticalSeeThrough) {
			//	WearableManager.GetInstance().GetDeviceController().SetStereoMode(true);

			//	CameraBackgroundBehaviour cameraBackground = FindObjectOfType<CameraBackgroundBehaviour>();
			//	cameraBackground.gameObject.SetActive(false);

			//	WearableManager.GetInstance().GetCalibration().CreateWearableEye(Camera.main.transform);

			//	// BT-300 screen is splited in half size, but R-7 screen is doubled.
			//	if (WearableManager.GetInstance().GetDeviceController().IsSideBySideType() == true) {
			//		// Do something here. For example resize gui to fit ratio
			//	}
			//}
		}

		private IEnumerator AddTrackerData() {
			yield return new WaitForEndOfFrame();
			foreach (var trackable in imageTrackablesMap) {
				if (trackable.Value.TrackerDataFileName.Length == 0) {
					continue;
				}

				if (trackable.Value.StorageType == StorageType.AbsolutePath) {
					TrackerManager.GetInstance().AddTrackerData(trackable.Value.TrackerDataFileName);
				} else if (trackable.Value.StorageType == StorageType.StreamingAssets) {
					if (Application.platform == RuntimePlatform.Android) {
						List<string> fileList = new List<string>();
						yield return StartCoroutine(MaxstARUtil.ExtractAssets(trackable.Value.TrackerDataFileName, fileList));
						TrackerManager.GetInstance().AddTrackerData(fileList[0], false);
					} else {
						TrackerManager.GetInstance().AddTrackerData(Application.streamingAssetsPath + "/" + trackable.Value.TrackerDataFileName);
					}
				}
			}

			TrackerManager.GetInstance().LoadTrackerData();
		}

		private void DisableAllTrackables() {
			foreach (var trackable in imageTrackablesMap) {
				trackable.Value.OnTrackFail();
			}
		}

		void Update() {
			DisableAllTrackables();

			TrackingState state = TrackerManager.GetInstance().UpdateTrackingState();

			if (state == null) {
				return;
			}

			cameraBackgroundBehaviour.UpdateCameraBackgroundImage(state);

			TrackingResult trackingResult = state.GetTrackingResult();

			for (int i = 0; i < trackingResult.GetCount(); i++) {
				Trackable trackable = trackingResult.GetTrackable(i);
				imageTrackablesMap[trackable.GetName()].OnTrackSuccess(trackable.GetId(), trackable.GetName(), trackable.GetPose());
			}
		}

		public void SetNormalMode() {
			TrackerManager.GetInstance().SetTrackingOption(TrackerManager.TrackingOption.NORMAL_TRACKING);
		}

		public void SetExtendedMode() {
			TrackerManager.GetInstance().SetTrackingOption(TrackerManager.TrackingOption.EXTEND_TRACKING);
		}

		public void SetMultiMode() {
			TrackerManager.GetInstance().SetTrackingOption(TrackerManager.TrackingOption.MULTI_TRACKING);
		}

		private void OnApplicationPause(bool pause) {
			if (pause) {
				TrackerManager.GetInstance().StopTracker();
				StopCamera();
			} else {
				StartCamera();
				TrackerManager.GetInstance().StartTracker(TrackerManager.TRACKER_TYPE_IMAGE);
			}
		}

		private void OnDestroy() {
			imageTrackablesMap.Clear();
			TrackerManager.GetInstance().SetTrackingOption(TrackerManager.TrackingOption.NORMAL_TRACKING);
			TrackerManager.GetInstance().StopTracker();
			TrackerManager.GetInstance().DestroyTracker();
			StopCamera();

			//if (ConfigurationScriptableObject.GetInstance().WearableType == WearableCalibration.WearableType.OpticalSeeThrough) {
			//	WearableManager.GetInstance().GetDeviceController().SetStereoMode(false);
			//}
		}
	}
}