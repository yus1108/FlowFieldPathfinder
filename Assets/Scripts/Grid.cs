using UnityEngine;

public class Grid
{
    public Vector3 WorldPosition { get; set; }
    public Vector2Int Index { get; set; }
    public GridDirection BestDirection { get; set; }
    public ushort BestCost { get; set; }
    public byte Cost { get; set; }
#if UNITY_EDITOR
    public float HeightForTileCube { get; set; }
#endif

    public Grid(Vector2Int index, Vector3 worldPosition)
    {
        Index = index;
        WorldPosition = worldPosition;
        Cost = 1;
        BestCost = ushort.MaxValue;
        BestDirection = GridDirection.None;
    }

    public void IncreaseCost(int amount)
    {
        if (Cost == byte.MaxValue) { return; }
        if (amount + Cost >= 255) { Cost = byte.MaxValue; }
        Cost += (byte)amount;
    }
}
