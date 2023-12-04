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

public class Cell
{
    public Vector3 center;
    public Vector3Int key;
    public bool isActive = true;
}

public class Chunk
{
    public Vector3 center; 
    public Vector3Int key;
    public Dictionary<Vector3Int, Cell> cells;
}

public class ChunkPrefab
{
    public Vector3 center;
    public Vector3Int key;
    public GameObject parentObject;
    public Dictionary<Vector3Int, GameObject> cells;
}