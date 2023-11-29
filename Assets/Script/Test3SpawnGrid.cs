using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Test3SpawnGrid : MonoBehaviour
{

    [SerializeField] private int worldChunkRadius; //radius in units of chunks
    [SerializeField] private int chunkRenderDistanceRadius; // radius of chunks that gets rendered. 
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private int halfChunkSize = 2; //stores in half size, so we can avoid division
    [SerializeField] private int cellSize = 1;

    private int chunkSize;

    //private Dictionary<Vector3Int, ChunkPrefab> chunkObjectPool;
    private Dictionary<Vector3Int, Chunk> chunks;

    void Start()
    {
        chunkSize = halfChunkSize * 2;
        chunks = new Dictionary<Vector3Int, Chunk>();
        //ChunkPrefab = new Dictionary<Vector3Int, ChunkPrefab>();

        GenerateChunks();
    }


    void Update()
    {

    }

    //Generates the chunks in the world
    void GenerateChunks()
    {
        //TODO: Generate the chunks
        //Loop through all chunks in world

        int x, y, z;
        int middleChunk = chunkSize;
        int min = -(worldChunkRadius * (chunkSize + middleChunk));
        int max = worldChunkRadius * (chunkSize + middleChunk);

        for (x = min; x <= max; x += chunkSize + middleChunk)
        {
            for (y = min; y <= max; y += chunkSize + middleChunk)
            {
                for (z = min; z < max; z += chunkSize + middleChunk)
                {

                    //create chunks ----------------------------
                    Vector3Int chunkPosition = GridFunctions.QuantizeFloatToInt(new Vector3(x, y, z), chunkSize);
                    Chunk newChunk = new Chunk()
                    {
                        position = chunkPosition,
                        cells = new Dictionary<Vector3Int, Cell>()
                    };

                    //this is specifically for object pool
                    chunks.Add(chunkPosition, newChunk);

                    //create cells ----------------------------

                    int middleCell = cellSize;
                    Vector3Int minimum = new Vector3Int(x - halfChunkSize, y - halfChunkSize, z - halfChunkSize);
                    Vector3Int maximum = new Vector3Int(x + halfChunkSize, y + halfChunkSize, z + halfChunkSize);
                    int cx, cy, cz;
                    for (cx = minimum.x; cx <= maximum.x; cx += cellSize + middleCell)
                    {
                        //for (cy = minimum.y; cy < maximum.y; cy += cellSize)
                        //{
                        //    for (cz = minimum.z; cz < maximum.z; cz += cellSize)
                        //    {
                        //        //create cell
                        //        Vector3Int cellPosition = GridFunctions.QuantizeFloatToInt(new Vector3(cx, cy, cz), cellSize);
                        //        Cell newCell = new Cell()
                        //        {
                        //            position = cellPosition,
                        //        };
                        //        //add to cell dic inside chunk
                        //        newChunk.cells.Add(cellPosition, newCell);

                        //        ////SHOW IT ---------------------------------------------------------------------
                        //        //var obj = Instantiate(cellPrefab, new Vector3(cx, cy, cz), Quaternion.identity);
                        //        //obj.transform.localScale = new Vector3(cellSize * 2, cellSize * 2, cellSize * 2);
                        //        //Renderer renderer = obj.gameObject.GetComponent<Renderer>();
                        //        //renderer.material.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 0.5f);
                        //    }
                        //}

                        var obj = Instantiate(cellPrefab, new Vector3(cx, 0, 0), Quaternion.identity);
                        obj.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
                        Renderer renderer = obj.gameObject.GetComponent<Renderer>();
                        renderer.material.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 0.5f);
                    }











                }
            }
        }
    }


    //var obj = Instantiate(cellPrefab, new Vector3(x, y, z), Quaternion.identity);
    //obj.transform.localScale = new Vector3(chunkSize* 2, chunkSize* 2, chunkSize* 2);
    //Renderer renderer = obj.gameObject.GetComponent<Renderer>();
    //renderer.material.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 0.5f);

    void OnDrawGizmosSelected()
    {
        foreach (var entry in chunks)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 1f);
            Gizmos.DrawWireCube((new Vector3Int(entry.Key.x , entry.Key.y , entry.Key.z)),
                new Vector3(chunkSize * 2, chunkSize * 2, chunkSize * 2));
        }
        //foreach (var entry in cells)
        //{
        //    Gizmos.color = new Color(0f, 1f, 0f, 1f);
        //
        //}
    }
}
