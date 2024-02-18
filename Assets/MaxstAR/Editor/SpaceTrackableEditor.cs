/*==============================================================================
Copyright 2017 Maxst, Inc. All Rights Reserved.
==============================================================================*/

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

namespace maxstAR
{
    [CustomEditor(typeof(SpaceTrackableBehaviour))]
    public class SpaceTrackableEditor : Editor
    {
        private SpaceTrackableBehaviour trackableBehaviour = null;

        public void OnEnable()
        {
            if (PrefabUtility.GetPrefabType(target) == PrefabType.Prefab)
            {
                return;
            }
        }

        public override void OnInspectorGUI()
        {
            if (PrefabUtility.GetPrefabType(target) == PrefabType.Prefab)
            {
                return;
            }

            bool isDirty = false;

            trackableBehaviour = (SpaceTrackableBehaviour)target;

            EditorGUILayout.Separator();

            StorageType oldType = trackableBehaviour.StorageType;
            StorageType newType = (StorageType)EditorGUILayout.EnumPopup("Storage type", trackableBehaviour.StorageType);

            if (oldType != newType)
            {
                trackableBehaviour.StorageType = newType;
                isDirty = true;
            }

            if (trackableBehaviour.StorageType == StorageType.StreamingAssets)
            {
                // directory
                EditorGUILayout.Separator();
                EditorGUILayout.HelpBox("Drag&drop a folder with scan data from your project view here, and click load Button to load 3D model files.", MessageType.Info);
                {
                    serializedObject.Update();
                    EditorGUI.BeginChangeCheck();

                    UnityEngine.Object oldDataObject = trackableBehaviour.SpaceData;
                    UnityEngine.Object newDataObject = EditorGUILayout.ObjectField(trackableBehaviour.SpaceData, typeof(UnityEngine.Object), true);

                    if (EditorGUI.EndChangeCheck())
                    {
                        // Check if the dragged object is the same as the target object
                        if (oldDataObject != newDataObject || DragAndDrop.objectReferences.Length > 0 && DragAndDrop.objectReferences[0] == trackableBehaviour.SpaceData)
                        {
                            isDirty = true;

                            if (newDataObject == null)
                            {
                                trackableBehaviour.SpaceData = null;
                                trackableBehaviour.SpaceDataPath = "";
                            }
                            else
                            {
                                string spaceDataPath = AssetDatabase.GetAssetPath(newDataObject);

                                if (Directory.Exists(spaceDataPath) && (File.GetAttributes(spaceDataPath) & FileAttributes.Directory) == FileAttributes.Directory!)
                                {
                                    trackableBehaviour.SpaceData = newDataObject;
                                    trackableBehaviour.SpaceDataPath = spaceDataPath.Replace("Assets/StreamingAssets/", "");

                                    string[] spaceMapPaths = Directory.GetFiles(spaceDataPath, "*.mmap");
                                    if (spaceMapPaths.Length > 0)
                                    {
                                        string spaceMapPath = spaceMapPaths[0].Replace($"{Application.streamingAssetsPath}/", "");
                                        spaceMapPath = spaceMapPath.Replace('\\', '/');
                                        trackableBehaviour.TrackerDataFileObject = AssetDatabase.LoadAssetAtPath<Object>(spaceMapPath);
                                        trackableBehaviour.TrackerDataFileName = spaceMapPath.Replace("Assets/StreamingAssets/", "");
                                    }
                                    isDirty = true;
                                }
                                else
                                {
                                    Debug.Log("spaceDataPath: " + spaceDataPath);
                                    Debug.LogError("Not a directory.");
                                }
                            }
                        }
                    }

                    serializedObject.ApplyModifiedProperties();
                }

                // Load Button
                GUILayout.BeginHorizontal(GUILayout.Width(0));
                GUILayout.FlexibleSpace();
                GUIContent content = new GUIContent("Load");

                if (GUILayout.Button(content, GUILayout.Width(100)))
                {
                    if (string.IsNullOrEmpty(trackableBehaviour.SpaceDataPath))
                    {
                        Debug.LogError("No directories were added.");
                    }
                    else if (!Directory.Exists($"{Application.streamingAssetsPath}/{trackableBehaviour.SpaceDataPath}"))
                    {
                        Debug.LogError("The directory does not exist in the path.");
                    }
                    else
                    {
                        if (trackableBehaviour != null)
                        {
                            trackableBehaviour.LoadSpace3DModels();
                        }

                        isDirty = true;
                    }
                }
                GUILayout.EndHorizontal();

                // mmap
                {
                    EditorGUILayout.Separator();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Space map", GUILayout.Width(EditorGUIUtility.labelWidth));

                    EditorGUI.BeginDisabledGroup(false);
                    UnityEngine.Object oldDataObject = trackableBehaviour.TrackerDataFileObject;
                    UnityEngine.Object newDataObject = EditorGUILayout.ObjectField(trackableBehaviour.TrackerDataFileObject, typeof(UnityEngine.Object), true, GUILayout.ExpandWidth(true));
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();

                    if (oldDataObject != newDataObject)
                    {
                        if (newDataObject == null)
                        {
                            isDirty = true;
                            trackableBehaviour.TrackerDataFileObject = null;
                            trackableBehaviour.TrackerDataFileName = "";
                        }
                        else
                        {
                            string trackerDataFileName = AssetDatabase.GetAssetPath(newDataObject);
                            if (!trackerDataFileName.EndsWith(".mmap"))
                            {
                                Debug.Log("trackerDataFileName: " + trackerDataFileName);
                                Debug.LogError("It's not proper tracker data file!!. File's extension should be .mmap");
                            }
                            else
                            {
                                trackableBehaviour.TrackerDataFileObject = newDataObject;
                                trackableBehaviour.TrackerDataFileName = trackerDataFileName.Replace("Assets/StreamingAssets/", "");
                                isDirty = true;
                            }
                        }
                    }
                }

                if (GUI.changed && isDirty)
                {
                    EditorUtility.SetDirty(trackableBehaviour);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    SceneManager.Instance.SceneUpdated();
                }
            }
        }
    }
}