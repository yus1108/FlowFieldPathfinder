using Codice.Client.BaseCommands;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static TerrainScript;

[CustomEditor(typeof(TerrainScript))]
public class TerrainScriptInspector : Editor
{
    private bool _wasMouseDown = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TerrainScript terrainScript = (TerrainScript)target;
        if (GUILayout.Button("Regenerate Tile Info"))
        {
            terrainScript.GenerateTileInfo();
        }
    }

    private void OnSceneGUI()
    {
        TerrainScript terrainScript = target as TerrainScript;
        switch (terrainScript.GetDebugOption())
        {
            case DebugOption.None:
                break;
            case DebugOption.ShowCostField:
                terrainScript.LabelCostField();
                break;
            case DebugOption.ShowBestCostField:
                terrainScript.LabelBestCostField();
                break;
            case DebugOption.ShowFlowField:
                break;
            default:
                break;
        }
        hoverTileCube();

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
            pickDestination(terrainScript);
            _wasMouseDown = true;
        }
        else if (Event.current.type != EventType.MouseDown && _wasMouseDown)
        {
            _wasMouseDown = false;
        }
    }

    private void hoverTileCube()
    {
        float tempValue = 1.0f;
        Vector3 mousePosition = Event.current.mousePosition;
        mousePosition.y = SceneView.lastActiveSceneView.camera.pixelHeight - mousePosition.y;
        mousePosition.z = 1.0f;
        mousePosition = SceneView.lastActiveSceneView.camera.ScreenToWorldPoint(mousePosition);

        Vector3 yDir = Vector3.zero - new Vector3(0, mousePosition.y, 0);
        Vector3 forward = (mousePosition - SceneView.lastActiveSceneView.camera.transform.position).normalized;
        float m = Vector3.Dot(forward, yDir) / yDir.magnitude;
        float n = mousePosition.y / m;
        Vector3 dest = forward * n + mousePosition;
        dest.x = Mathf.Floor(dest.x) + 0.5f;
        dest.z = Mathf.Floor(dest.z) + 0.5f;

        var color = new Color(1, 0.8f, 0.4f, 1);
        Handles.color = color;
        Handles.DrawWireCube(dest, Vector3.one);
        GUI.color = color;
        Handles.Label(dest, tempValue.ToString("F1"));
        SceneView.RepaintAll();
    }

    private void pickDestination(TerrainScript terrainScript)
    {
        Vector3 mousePosition = Event.current.mousePosition;
        mousePosition.y = SceneView.lastActiveSceneView.camera.pixelHeight - mousePosition.y;
        mousePosition.z = 1.0f;
        mousePosition = SceneView.lastActiveSceneView.camera.ScreenToWorldPoint(mousePosition);

        Vector3 yDir = Vector3.zero - new Vector3(0, mousePosition.y, 0);
        Vector3 forward = (mousePosition - SceneView.lastActiveSceneView.camera.transform.position).normalized;
        float m = Vector3.Dot(forward, yDir) / yDir.magnitude;
        float n = mousePosition.y / m;
        Vector3 dest = forward * n + mousePosition;
        terrainScript.GenerateTileInfo();
        terrainScript.GenerateIntegrationField(new Vector2Int((int)Mathf.Floor(dest.x), (int)Mathf.Floor(dest.z)));
        terrainScript.GenerateFlowField();
    }
}
