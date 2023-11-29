using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridFunctions
{
    public static Vector3Int QuantizeFloatToInt(Vector3 position, float cellSize)
    {
        return new Vector3Int(
            (int)Mathf.Floor(position.x / cellSize),
            (int)Mathf.Floor(position.y / cellSize),
            (int)Mathf.Floor(position.z / cellSize));
    }
}

class Cell
{
    public Vector3Int position;
}

class Chunk
{
    public Vector3Int position;
    public Dictionary<Vector3Int, Cell> cells;
}

class ChunkPrefab
{
    public GameObject parentObject;
    public Vector3Int position;
    public Dictionary<Vector3Int, GameObject> cells;
}