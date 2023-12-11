using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class GridFunctions
{

    /// <summary>
    ///
    /// Function that quantize a position into a bucket that can store data.
    /// This bucket is used as a key for the dictionaries 
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="sizeOfContainer"></param>
    /// <returns> </returns>
    public static Vector3Int QuantizeFloatToInt(Vector3 position, float sizeOfContainer)
    {
        return new Vector3Int(
            (int)Mathf.Floor(position.x / sizeOfContainer),
            (int)Mathf.Floor(position.y / sizeOfContainer),
            (int)Mathf.Floor(position.z / sizeOfContainer));
    }
}

/// <summary>
/// 
/// This is the cell that is stored for every individual unit in a chunk
/// 
/// </summary>
public class Cell
{
    public Vector3 center;
    public Vector3Int key;
    public bool isCellOn = true;
}

/// <summary>
///
/// This is a chunk that stores a dictionary of cells, this is a segment of the grid
/// 
/// </summary>
public class Chunk
{
    public Vector3 center; 
    public Vector3Int key;
    public Dictionary<Vector3Int, Cell> cells;
}

/// <summary>
/// 
/// This is a chunk PREFAB, this is solely created for the prefab chunks which there is a finite amount, depending on the render distance
/// 
/// </summary>
public class ChunkPrefab
{
    public Vector3 center;
    public Vector3Int key;
    public GameObject parentObject;
    public Dictionary<Vector3Int, GameObject> cells;
}