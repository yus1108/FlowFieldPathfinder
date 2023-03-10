using System;
using System.Text;
using UnityEditor;
using UnityEngine;

public class FixedPoint
{
    public const byte MaxPointDigit = 8;
    public const int MultiplerOffset = 100000000;

    public int HighValue { get; private set; }
    public int LowValue { get; private set; }
    public long RawValue
    {
        get { return ((long)HighValue * MultiplerOffset) + LowValue; }
        set { HighValue = (int)(value / MultiplerOffset); LowValue = (int)(value % MultiplerOffset); }
    }

    public FixedPoint()
    {
        HighValue = 0;
        LowValue = 0;
    }
    public FixedPoint(Int32 num)
    {
        HighValue = num;
        LowValue = 0;
    }
    public FixedPoint(float num)
    {
        HighValue = (int)num;
        LowValue = (int)((num - (int)num) * MultiplerOffset);
    }
    public FixedPoint(double num)
    {
        HighValue = (int)num;
        LowValue = (int)((num - (int)num) * MultiplerOffset);
    }

    public static FixedPoint operator -(FixedPoint a, FixedPoint b) => new FixedPoint() { RawValue = a.RawValue - b.RawValue };
    public static FixedPoint operator -(FixedPoint a, int b) => new FixedPoint() { RawValue = a.RawValue - new FixedPoint(b).RawValue };
    public static FixedPoint operator -(int a, FixedPoint b) => new FixedPoint() { RawValue = new FixedPoint(a).RawValue - b.RawValue };

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(HighValue.ToString() + ".");
        string lowValueStr = LowValue.ToString();
        for (int i = 0; i < MaxPointDigit - lowValueStr.Length; i++)
        {
            sb.Append('0');
        }
        sb.Append(lowValueStr);
        return sb.ToString();
    }

    public string ToString(string fmt)
    {
        StringBuilder sb = new StringBuilder(HighValue.ToString() + ".");
        string lowValueStr = LowValue.ToString();
        if (fmt.StartsWith("F") || fmt.StartsWith("f"))
        {
            StringBuilder lowValueStringBuilder = new StringBuilder();
            for (int i = 0; i < MaxPointDigit - lowValueStr.Length; i++)
            {
                lowValueStringBuilder.Append('0');
            }
            lowValueStringBuilder.Append(lowValueStr);
            lowValueStr = lowValueStringBuilder.ToString();

            int digits = Convert.ToInt32(fmt.Substring(1));
            for (int i = 0; i < digits; i++)
            {
                sb.Append(lowValueStr[i]);
            }
            return sb.ToString();
        }

        for (int i = 0; i < MaxPointDigit - lowValueStr.Length; i++)
        {
            sb.Append('0');
        }
        sb.Append(lowValueStr);
        return sb.ToString();
    }

    public int ToInt32()
    {
        return HighValue;
    }

    public float ToFloat()
    {
        float result = HighValue + LowValue / (float)MultiplerOffset;
        return HighValue + LowValue / (float)MultiplerOffset;
    }
}

public class Tile
{
    public Vector2Int GridIndex { get; set; }
    public Vector3 WorldPosition { get; set; }
    public byte Cost { get; set; }
    public ushort BestCost { get; set; }
#if UNITY_EDITOR
    public float HeightForTileCube { get; set; }
#endif

    public Tile(Vector2Int gridIndex, Vector3 worldPosition)
    {
        GridIndex = gridIndex;
        WorldPosition = worldPosition;
        Cost = 1;
        BestCost = ushort.MaxValue;
    }

    public void IncreaseCost(int amount)
    {
        if (Cost == byte.MaxValue) { return; }
        if (amount + Cost >= 255) { Cost = byte.MaxValue; }
        Cost += (byte)amount;
    }
}

public class TerrainScript : MonoBehaviour
{
#if UNITY_EDITOR
    public enum DebugOption
    {
        None,
        ShowCostField,
        ShowBestCostField,
        ShowFlowField,
    }
#endif

    [SerializeField] private float slopeThreshHold = 0.6f;
    [SerializeField] private float slopeMultiplierForCost = 1.0f;
#if UNITY_EDITOR
    [SerializeField] private DebugOption debugOption = DebugOption.None;
#endif
    public Tile[,] tiles { get; private set; }

    private void Start()
    {
        GenerateTileInfo();
    }

    void OnDrawGizmosSelected()
    {
        if (debugOption != DebugOption.ShowCostField)
            return;
        DrawCostFieldCubes();
    }

    public void GenerateTileInfo()
    {
        Terrain terrain = GetComponent<Terrain>();
        if (terrain == null)
            Debug.Log("Terrain 컴포넌트가 있어야 합니다!!!");

        TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.heightmapResolution - 1;
        tiles = new Tile[resolution, resolution];

        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                float maxHeight = Mathf.Max(
                    new float[]
                    {
                        terrainData.GetHeight(i, j),
                        terrainData.GetHeight(i + 1, j),
                        terrainData.GetHeight(i, j + 1),
                        terrainData.GetHeight(i + 1, j + 1)
                    });
                float minHeight = Mathf.Min(
                    new float[]
                    {
                        terrainData.GetHeight(i, j),
                        terrainData.GetHeight(i + 1, j),
                        terrainData.GetHeight(i, j + 1),
                        terrainData.GetHeight(i + 1, j + 1)
                    });
                float heightForTileCube = maxHeight - minHeight;
                Vector3 worldPosition = transform.position + (new Vector3(1.0f, heightForTileCube, 1.0f) * 0.5f) + new Vector3(i, minHeight, j);
                tiles[i, j] = new Tile(new Vector2Int(i, j), worldPosition);
#if UNITY_EDITOR
                tiles[i, j].HeightForTileCube = heightForTileCube;
#endif

                if (heightForTileCube < slopeThreshHold)
                {
                    Collider[] obstacles = Physics.OverlapBox(new Vector3(i + 0.5f, minHeight, j + 0.5f), Vector3.one * 0.49f, Quaternion.identity, LayerMask.GetMask("Obstacle"));
                    bool hasObstacle = false;
                    foreach (Collider collider in obstacles)
                    {
                        if (collider.gameObject.layer == 6)
                        {
                            tiles[i, j].Cost = byte.MaxValue;
                            hasObstacle = true;
                        }
                    }

                    if (hasObstacle == false)
                        tiles[i, j].Cost = (byte)(new Vector2(1.0f, Mathf.Pow(heightForTileCube * slopeMultiplierForCost, 2)).magnitude);
                }
                else
                {
                    tiles[i, j].Cost = byte.MaxValue;
                }
            }
        }
        Debug.Log("Tile Info Generated!!!");
    }

#if UNITY_EDITOR
    public DebugOption GetDebugOption() { return debugOption; }

    public void LabelCostField()
    {
        if (tiles == null)
            GenerateTileInfo();

        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                Handles.Label(tiles[x, y].WorldPosition, tiles[x, y].Cost.ToString());
            }
        }
    }

    private void DrawCostFieldCubes()
    {
        if (tiles == null)
            GenerateTileInfo();

        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                float heightForCube = tiles[x, y].HeightForTileCube + 0.1f;
                Vector3 scale = new Vector3(1, heightForCube, 1);
                float weight = (tiles[x, y].Cost - 1) / 254.0f;
                Gizmos.color = new Color(weight, 1.0f - weight, 0.0f, 0.5f);
                Gizmos.DrawCube(tiles[x, y].WorldPosition, scale);
            }
        }
    }
#endif
}