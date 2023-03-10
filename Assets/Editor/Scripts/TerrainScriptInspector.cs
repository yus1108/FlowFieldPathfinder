using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(TerrainScript))]
public class TerrainScriptInspector : Editor
{
    private static Vector2 MousePosition => Event.current.mousePosition;
    private static bool isShowWireCube = false;
    private bool isRepaint = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TerrainScript terrainScript = (TerrainScript)target;
        string buttonName = isShowWireCube == false ? "Show wire cube on hovering tile" : "Hide wire cube on hovering tile";
        if (GUILayout.Button(buttonName))
        {
            isShowWireCube = !isShowWireCube;
        }

        if (GUILayout.Button("Regenerate Tile Info"))
        {
            terrainScript.GenerateTileInfo();
        }
    }

    private void OnSceneGUI()
    {
        TerrainScript script = target as TerrainScript;
        if (script.GetDebugOption() == TerrainScript.DebugOption.ShowCostField) { script.LabelCostField(); }
        

        if (isShowWireCube)
        {
            hoverTileCube();
        }

        if (isRepaint)
        {
            isRepaint = false;
            SceneView.RepaintAll();
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
        isRepaint = true;
    }
}
