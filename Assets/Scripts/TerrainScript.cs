using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;

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

    [SerializeField] private float _slopeThreshHold = 0.6f;
    [SerializeField] private float _slopeMultiplierForCost = 1.0f;
    [SerializeField] private Grid _destinationGrid;
#if UNITY_EDITOR
    [SerializeField] private DebugOption _debugOption = DebugOption.None;
#endif
    public Grid[,] _grids { get; private set; }

    private void Start()
    {
        GenerateTileInfo();
    }

    public void GenerateTileInfo()
    {
        Terrain terrain = GetComponent<Terrain>();
        if (terrain == null)
            Debug.Log("Terrain 컴포넌트가 있어야 합니다!!!");

        TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.heightmapResolution - 1;
        _grids = new Grid[resolution, resolution];

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
                _grids[i, j] = new Grid(new Vector2Int(i, j), worldPosition);
#if UNITY_EDITOR
                _grids[i, j].HeightForTileCube = heightForTileCube;
#endif

                if (heightForTileCube < _slopeThreshHold)
                {
                    Collider[] obstacles = Physics.OverlapBox(new Vector3(i + 0.5f, minHeight, j + 0.5f), Vector3.one * 0.49f, Quaternion.identity, LayerMask.GetMask("Obstacle"));
                    bool hasObstacle = false;
                    foreach (Collider collider in obstacles)
                    {
                        if (collider.gameObject.layer == 6)
                        {
                            _grids[i, j].Cost = byte.MaxValue;
                            hasObstacle = true;
                        }
                    }

                    if (hasObstacle == false)
                        _grids[i, j].Cost = (byte)(new Vector2(1.0f, Mathf.Pow(heightForTileCube * _slopeMultiplierForCost, 2)).magnitude);
                }
                else
                {
                    _grids[i, j].Cost = byte.MaxValue;
                }
            }
        }
        Debug.Log("Tile Info Generated!!!");
    }

    public void GenerateIntegrationField(Vector2Int destination)
    {
        //if (_grids[destination.x, destination.y].Cost == byte.MaxValue)
        //return;

        _grids[destination.x, destination.y].Cost = 0;
        _grids[destination.x, destination.y].BestCost = 0;
        _destinationGrid = _grids[destination.x, destination.y];

        Queue<Grid> gridQueue = new Queue<Grid>();
        gridQueue.Enqueue(_destinationGrid);

        while (gridQueue.Count > 0)
        {
            Grid currentGrid = gridQueue.Dequeue();
            List<Grid> neighbourGrids = GetNeighbourGrids(currentGrid.Index, GridDirection.CardinalDirections);
            foreach (Grid neighbourGrid in neighbourGrids)
            {
                if (neighbourGrid.Cost == byte.MaxValue)
                    continue;

                if (neighbourGrid.Cost + currentGrid.BestCost < neighbourGrid.BestCost)
                {
                    neighbourGrid.BestCost = (ushort)(neighbourGrid.Cost + currentGrid.BestCost);
                    gridQueue.Enqueue(neighbourGrid);
                }
            }
        }
    }

    public void GenerateFlowField()
    {
        foreach (Grid currentGrid in _grids)
        {
            List<Grid> neighbourGrids = GetNeighbourGrids(currentGrid.Index, GridDirection.AllDirections);

            int bestCost = currentGrid.BestCost;
            foreach (Grid neighbourGrid in neighbourGrids)
            {
                if (neighbourGrid.BestCost < bestCost)
                {
                    bestCost = neighbourGrid.BestCost;
                    if (currentGrid.BestCost == ushort.MaxValue)
                        currentGrid.BestDirection = GridDirection.None;
                    else
                        currentGrid.BestDirection = GridDirection.GetDirectionFromV2I(neighbourGrid.Index - currentGrid.Index);
                }
            }
        }
    }

    private List<Grid> GetNeighbourGrids(Vector2Int gridIndex, List<GridDirection> directions)
    {
        List<Grid> neighbourGrids = new List<Grid>();
        foreach (Vector2Int currentDirection in directions)
        {
            Grid neighbourGrid = GetGridAtRelativePos(gridIndex, currentDirection);
            if (neighbourGrid != null)
                neighbourGrids.Add(neighbourGrid);
        }
        return neighbourGrids;
    }

    private Grid GetGridAtRelativePos(Vector2Int originalPos, Vector2Int relativePos)
    {
        Vector2Int position = originalPos + relativePos;
        if (position.x < 0 || position.x >= _grids.GetLength(0) || position.y < 0 || position.y >= _grids.GetLength(1))
            return null;
        return _grids[position.x, position.y];
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        switch (_debugOption)
        {
            case DebugOption.None:
                break;
            case DebugOption.ShowCostField:
                DrawCostFieldCubes();
                break;
            case DebugOption.ShowBestCostField:
                DrawBestCostField();
                break;
            case DebugOption.ShowFlowField:
                DrawFlowField();
                break;
            default:
                break;
        }
    }

    public DebugOption GetDebugOption() { return _debugOption; }

    public void LabelCostField()
    {
        if (_grids == null)
            GenerateTileInfo();

        for (int x = 0; x < _grids.GetLength(0); x++)
        {
            for (int y = 0; y < _grids.GetLength(1); y++)
            {
                Handles.Label(_grids[x, y].WorldPosition, _grids[x, y].Cost.ToString());
            }
        }
    }

    public void LabelBestCostField()
    {
        if (_grids == null)
            GenerateTileInfo();

        for (int x = 0; x < _grids.GetLength(0); x++)
        {
            for (int y = 0; y < _grids.GetLength(1); y++)
            {
                Handles.Label(_grids[x, y].WorldPosition, _grids[x, y].BestCost.ToString());
            }
        }
    }

    private void DrawCostFieldCubes()
    {
        if (_grids == null)
            GenerateTileInfo();

        for (int x = 0; x < _grids.GetLength(0); x++)
        {
            for (int y = 0; y < _grids.GetLength(1); y++)
            {
                float heightForCube = _grids[x, y].HeightForTileCube + 0.1f;
                Vector3 scale = new Vector3(1, heightForCube, 1);
                float weight = (_grids[x, y].Cost - 1) / (byte.MaxValue - 1.0f);
                Gizmos.color = new UnityEngine.Color(weight, 1.0f - weight, 0.0f, 0.5f);
                Gizmos.DrawCube(_grids[x, y].WorldPosition, scale);
            }
        }
    }

    private void DrawBestCostField()
    {
        if (_grids == null)
            GenerateTileInfo();

        for (int x = 0; x < _grids.GetLength(0); x++)
        {
            for (int y = 0; y < _grids.GetLength(1); y++)
            {
                float heightForCube = _grids[x, y].HeightForTileCube + 0.1f;
                Vector3 scale = new Vector3(1, heightForCube, 1);
                float weight = (_grids[x, y].BestCost) / short.MaxValue;
                Gizmos.color = new UnityEngine.Color(weight, 1.0f - weight, 0.0f, 0.5f);
                Gizmos.DrawCube(_grids[x, y].WorldPosition, scale);
            }
        }
    }

    private void DrawFlowField()
    {
        if (_grids == null)
            GenerateTileInfo();

        for (int x = 0; x < _grids.GetLength(0); x++)
        {
            for (int y = 0; y < _grids.GetLength(1); y++)
            {
                Vector3 position = _grids[x, y].WorldPosition;
                if (_grids[x, y].BestDirection == GridDirection.None)
                {
                    if (_grids[x, y].Cost == 0)
                    {
                        Gizmos.DrawIcon(position, "destination.png", false);
                    }
                    else 
                    {
                        Gizmos.DrawIcon(position, "none.jpg", false);
                    }
                }
                else if (_grids[x, y].BestDirection == GridDirection.East)
                    Gizmos.DrawIcon(position, "east.jpg", false);
                else if (_grids[x, y].BestDirection == GridDirection.SouthEast)
                    Gizmos.DrawIcon(position, "southeast.jpg", false);
                else if (_grids[x, y].BestDirection == GridDirection.South)
                    Gizmos.DrawIcon(position, "south.jpg", false);
                else if (_grids[x, y].BestDirection == GridDirection.SouthWest)
                    Gizmos.DrawIcon(position, "southwest.jpg", false);
                else if (_grids[x, y].BestDirection == GridDirection.West)
                    Gizmos.DrawIcon(position, "west.jpg", false);
                else if (_grids[x, y].BestDirection == GridDirection.NorthWest)
                    Gizmos.DrawIcon(position, "northwest.jpg", false);
                else if (_grids[x, y].BestDirection == GridDirection.North)
                    Gizmos.DrawIcon(position, "north.jpg", false);
                else if (_grids[x, y].BestDirection == GridDirection.NorthEast)
                    Gizmos.DrawIcon(position, "northeast.jpg", false);
                float heightForCube = _grids[x, y].HeightForTileCube + 0.1f;
                Vector3 scale = new Vector3(1, heightForCube, 1);
                float weight = (_grids[x, y].BestCost) / short.MaxValue;
                Gizmos.color = new UnityEngine.Color(weight, 1.0f - weight, 0.0f, 0.5f);
                Gizmos.DrawCube(_grids[x, y].WorldPosition, scale);
            }
        }
    }
#endif
}