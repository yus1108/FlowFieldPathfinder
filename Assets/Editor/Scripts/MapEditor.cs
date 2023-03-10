using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

public class MapEditor : EditorWindow
{
    //string myString = "Hello World";
    //bool groupEnabled;
    //bool myBool = true;
    //float myFloat = 1.23f;
    
    //Window 메뉴에 "My Window" 항목을 추가한다.
    [MenuItem("RTS Maker/Map Editor")]
    public static void ShowWindow()
    {
        GetWindow(typeof(MapEditor));
    }

    private void OnEnable()
    {
        
    }

    void OnGUI()
    {
        //    GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        //    myString = EditorGUILayout.TextField("Text Field", myString);

        //    groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        //    myBool = EditorGUILayout.Toggle("Toggle", myBool);
        //    myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        //    TerrainScript.SlopeThreshHold = EditorGUILayout.Slider("SlopeThreshHold", TerrainScript.SlopeThreshHold, 0, 5);
        //    EditorGUILayout.EndToggleGroup();
    }
}