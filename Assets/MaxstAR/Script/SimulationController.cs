using UnityEngine;
using UnityEditor;
using maxstAR;
#if UNITY_EDITOR
#endif

#if UNITY_EDITOR
[InitializeOnLoadAttribute]
public static class PlayModeStateChanged
{
    // register an event handler when the class is initialized
    static PlayModeStateChanged()
    {
        EditorApplication.playModeStateChanged += SimulationController.PlayModeState;
    }
}
#endif
public class SimulationController : MaxstSingleton<SimulationController>
{
#if UNITY_EDITOR
    public static void PlayModeState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {

        }
    }
#endif

    [HideInInspector]
    [SerializeField]
    public string simulatePath = "";

    [SerializeField]
    private bool simulationMode;
    public bool SimulationMode
    {
        get
        {
            return simulationMode;
        }
        set
        {
            simulationMode = value;
        }
    }

    private void Start()
    {
    }
}
