using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Test3SpawnGrid : MonoBehaviour
{

    [SerializeField] private int worldChunkRadius; //radius in units of chunks
    [SerializeField] private int chunkRenderDistanceRadius; // radius of chunks that gets rendered. 
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private int halfChunkSize; //stores in half size, so we can avoid division
    [SerializeField] private int halfCellSize; //half cell size MUST be divisable inside half chunk size

    private int chunkSize;
    private int cellSize;
    private int totalWorldChunkSize = 0;
    private int totalWorldRenderDistance = 0;

    GameObject player;
    Vector3 playerOldChunk;

    private Dictionary<Vector3Int, ChunkPrefab> ChunkPrefabs;
    private Dictionary<Vector3Int, Chunk> chunks;

    void Start()
    {
        Checks(); //Checks to make sure that the sizes of the chunks and cells lines up correctly. 

        ChunkPrefabs = new Dictionary<Vector3Int, ChunkPrefab>();
        chunks = new Dictionary<Vector3Int, Chunk>();
        chunkSize = halfChunkSize * 2;
        cellSize = halfCellSize * 2;

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

        for (x = min; x <= max; x += chunkSize)
        {
            for (y = min; y <= max; y += chunkSize)
            {
                for (z = min; z <= max; z += chunkSize)
                {
                    #region chunks
                    Vector3Int chunkPosition = GridFunctions.QuantizeFloatToInt(new Vector3(x, y, z), chunkSize);
                    Chunk newChunk = new Chunk()
                    {
                        center = new Vector3(x, y, z),
                        position = chunkPosition,
                        cells = new Dictionary<Vector3, Cell>()
                    };

                    //this is specifically for object pool
                    chunks.Add(chunkPosition, newChunk);
                    #endregion chunks

                    #region cells
                    int middleCell = cellSize;
                    Vector3Int minimum = new Vector3Int(x - halfChunkSize + halfCellSize, y - halfChunkSize + halfCellSize, z - halfChunkSize + halfCellSize);
                    Vector3Int maximum = new Vector3Int(x + halfChunkSize - halfCellSize, y + halfChunkSize - halfCellSize, z + halfChunkSize - halfCellSize);
                    int cx, cy, cz;
                    for (cx = minimum.x; cx <= maximum.x; cx += cellSize)
                    {
                        for (cy = minimum.y; cy <= maximum.y; cy += cellSize)
                        {
                            for (cz = minimum.z; cz <= maximum.z; cz += cellSize)
                            {
                                //create cell
                                Vector3 cellPosition = new Vector3(cx, cy, cz);
                                //Vector3Int cellPosition = GridFunctions.QuantizeFloatToInt(new Vector3(cx, cy, cz), cellSize);
                                Cell newCell = new Cell()
                                {
                                    position = cellPosition,
                                };
                                //add to cell dic inside chunk
                                newChunk.cells.Add(cellPosition, newCell);

                                var obj = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
                                obj.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
                                Renderer renderer = obj.gameObject.GetComponent<Renderer>();
                                renderer.material.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 0.5f);
                            }
                        }
                    }
                    #endregion cells
                }
            }
        }
    }
   

    void GenerateChunksObjectPool()
    {
        ////Loop through all chunks in render distance
        //for (int i = 0; i < totalWorldRenderDistance; i++)
        //{
        //    //create chunks ----------------------------
        //    Vector3Int chunkPosition = new Vector3Int(i * chunkSize, 0, 0);
        //    ChunkPrefab newChunkPrefab = new ChunkPrefab()
        //    {
        //        position = chunkPosition,
        //        cells = new Dictionary<Vector3Int, GameObject>()
        //    };
        //    //add chunks to object pool
        //    ChunkPrefab.Add(chunkPosition, newChunkPrefab);

        //    //create cells ----------------------------
        //    var newChunkCellsObjectPool = newChunkPrefab.cells;

        //    //create object pool parent chunk game object
        //    var chunkParent = new GameObject();
        //    chunkParent.transform.position = chunkPosition;
        //    chunkParent.name = "Chunk [" + chunkPosition.x + ", " + chunkPosition.y + ", " + chunkPosition.z + "]";

        //    newChunkPrefab.parentObject = chunkParent;

        //    Vector3Int min = chunkPosition;
        //    Vector3Int max = new Vector3Int(chunkPosition.x + chunkSize, chunkPosition.y + chunkSize, chunkPosition.z + chunkSize);

        //    for (int cx = min.x; cx < max.x; cx += cellSize)
        //    {
        //        for (int cy = min.y; cy < max.y; cy += cellSize)
        //        {
        //            for (int cz = min.z; cz < max.z; cz += cellSize)
        //            {
        //                //create cell
        //                Vector3Int cellPosition = GridFunctions.QuantizeFloatToInt(new Vector3(cx, cy, cz), cellSize);

        //                //create cell object pool
        //                var obj = Instantiate(cellPrefab , cellPosition, Quaternion.identity);
        //               // obj.transform.position = cellPosition;
        //                obj.transform.SetParent(chunkParent.transform);
        //                newChunkCellsObjectPool.Add(cellPosition, obj);
        //            }
        //        }
        //    }
        //}
    }

    void OnDrawGizmosSelected()
    {
        foreach (var entry in chunks)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 1f);
            Gizmos.DrawWireCube((new Vector3(entry.Value.center.x, entry.Value.center.y, entry.Value.center.z)),
                new Vector3(chunkSize, chunkSize, chunkSize));
        }
    }

    private void Checks()
    {
        if (halfChunkSize % halfCellSize != 0)
        {
            Debug.LogError("HalfChunkSize must be divisible by HalfCellSize");
            Application.Quit(1);
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }
    
}



