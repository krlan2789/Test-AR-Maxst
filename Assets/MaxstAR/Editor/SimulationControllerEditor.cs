using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(SimulationController))]
public class SimulationControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        SimulationController simulationController = (SimulationController)target;

        bool isDirty = false;

        EditorGUILayout.LabelField("Simulation Directory Path");

        string enteredSimuatePath = EditorGUILayout.TextField(simulationController.simulatePath);
        if (enteredSimuatePath != simulationController.simulatePath)
        {
            simulationController.simulatePath = enteredSimuatePath;
            isDirty = true;
        }

        GUIContent content = new GUIContent("Open");
        if (GUILayout.Button(content, GUILayout.MaxWidth(Screen.width), GUILayout.MaxHeight(25)))
        {
            string selectedPathName = EditorUtility.OpenFolderPanel("", "열기", "");
            if (selectedPathName.Length > 0)
            {
                simulationController.simulatePath = selectedPathName;
                isDirty = true;
            }
        }

        if (GUI.changed || isDirty)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
