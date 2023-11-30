using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridFunctions
{
    public static Vector3Int QuantizeFloatToInt(Vector3 position, float sizeOfContainer)
    {
        return new Vector3Int(
            (int)Mathf.Floor(position.x / sizeOfContainer),
            (int)Mathf.Floor(position.y / sizeOfContainer),
            (int)Mathf.Floor(position.z / sizeOfContainer));
    }
}

class Cell
{
    public Vector3 center;
    public Vector3 position;
}

class Chunk
{
    public Vector3 center;
    public Vector3Int position;
    public Dictionary<Vector3, Cell> cells;
}

class ChunkPrefab
{
    public Vector3 center;
    public GameObject parentObject;
    public Vector3Int position;
    public Dictionary<Vector3Int, GameObject> cells;
}