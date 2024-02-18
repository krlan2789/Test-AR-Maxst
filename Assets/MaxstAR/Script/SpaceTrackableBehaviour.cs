/*==============================================================================
Copyright 2017 Maxst, Inc. All Rights Reserved.
==============================================================================*/

using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace maxstAR
{
    public class SpaceTrackableBehaviour : AbstractSpaceTrackableBehaviour
    {
        [SerializeField]
        private UnityEngine.Object spaceData;
        public UnityEngine.Object SpaceData
        {
            get
            {
                return spaceData;
            }
            set
            {
                spaceData = value;
            }
        }

        [SerializeField]
        private string spaceDataPath;

        public string SpaceDataPath
        {
            get
            {
                return spaceDataPath;
            }
            set
            {
                spaceDataPath = value;
            }
        }

        public override void OnTrackSuccess(string id, string name, Matrix4x4 poseMatrix)
        {
            Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
            Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

            // Enable renderers
            foreach (Renderer component in rendererComponents)
            {
                component.enabled = true;
            }

            // Enable colliders
            foreach (Collider component in colliderComponents)
            {
                component.enabled = true;
            }

            transform.position = MatrixUtils.PositionFromMatrix(poseMatrix);
            transform.rotation = MatrixUtils.QuaternionFromMatrix(poseMatrix);
            transform.localScale = MatrixUtils.ScaleFromMatrix(poseMatrix);
        }

        public override void OnTrackFail()
        {
            Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
            Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

            // Disable renderer
            foreach (Renderer component in rendererComponents)
            {
                component.enabled = false;
            }

            // Disable collider
            foreach (Collider component in colliderComponents)
            {
                component.enabled = false;
            }
        }
#if UNITY_EDITOR
        public bool LoadSpace3DModels()
        {
            string directoryPath = $"{Application.streamingAssetsPath}/{SpaceDataPath}";

            List<string> objFilePaths = new();
            FindObjFiles(objFilePaths, directoryPath, "obj");

            foreach (var objFilePath in objFilePaths)
            {
                LoadObj(objFilePath);
                Debug.Log(objFilePath);
            }

            return objFilePaths.Count > 0;
        }

        private void LoadObj(string objPath)
        {
            string error;
            {
                if (!File.Exists(objPath))
                {
                    error = "File doesn't exist.";
                }
                else
                {
                    GameObject loadedObject = null;
                    if (loadedObject != null)
                    {
                        Destroy(loadedObject);
                    }

                    FileInfo objInfo = new FileInfo(objPath);
                    string path = objInfo.DirectoryName + "/";
                    string filename = objInfo != null ? Path.GetFileName(objInfo.Name) : "WavefrontObject.obj";
                    string filenameWithOutExtension = objInfo != null ? Path.GetFileNameWithoutExtension(objInfo.Name) : "WavefrontObject";
                    loadedObject = new GameObject(filenameWithOutExtension);
                    loadedObject.transform.localScale = new Vector3(1f, 1f, -1f);
                    ObjectLoader loader = loadedObject.AddComponent<ObjectLoader>();
                    loader.Load(path, filename);

                    loadedObject.transform.parent = transform;
                    error = string.Empty;
                }
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                GUI.color = Color.red;
                GUI.Box(new Rect(0, 64, 256 + 64, 32), error);
                GUI.color = Color.white;
            }

            if (error != string.Empty)
            {
                Debug.Log($"error: {error}");
            }
        }

        private void FindObjFiles(List<string> files, string directoryPath, string extension)
        {
            try
            {
                string[] searchingFiles = Directory.GetFiles(directoryPath, $"*.{extension}");
                foreach (string searchingFile in searchingFiles)
                {
                    files.Add(searchingFile);
                }

                string[] subDirectories = Directory.GetDirectories(directoryPath);
                foreach (string subDirectory in subDirectories)
                {
                    FindObjFiles(files, subDirectory, extension);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error while searching for files: " + e.Message);
            }
        }
#endif
    }
}