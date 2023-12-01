using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private int totalWorldChunkSize;
    private int totalWorldRenderDistance;

    GameObject player;
    Vector3 playerOldChunk;

    private Dictionary<Vector3Int, ChunkPrefab> chunkPrefabs;
    private Dictionary<Vector3Int, Chunk> chunks;

    void Start()
    {
        Checks(); //Checks to make sure that the sizes of the chunks and cells lines up correctly. 

        totalWorldRenderDistance = chunkRenderDistanceRadius * 2;
        totalWorldRenderDistance = totalWorldRenderDistance * totalWorldRenderDistance * totalWorldRenderDistance;

        chunkPrefabs = new Dictionary<Vector3Int, ChunkPrefab>();
        chunks = new Dictionary<Vector3Int, Chunk>();
        chunkSize = halfChunkSize * 2;
        cellSize = halfCellSize * 2;

        GenerateChunks();
        GenerateChunksObjectPool();
        PositionObjectPool();
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
                    Vector3Int chunkIndex = GridFunctions.QuantizeFloatToInt(new Vector3(x, y, z), chunkSize);
                    Vector3 chunkPosition = new Vector3(x, y, z);
                    Chunk newChunk = new Chunk()
                    {
                        center = new Vector3(x, y, z),
                        //position = chunkPosition,
                        cells = new Dictionary<Vector3Int, Cell>()
                    };

                    //this is specifically for object pool
                    chunks.Add(chunkIndex, newChunk);
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
                                Vector3Int cellIndex = GridFunctions.QuantizeFloatToInt(new Vector3(cx, cy, cz), cellSize);
                                Cell newCell = new Cell()
                                {
                                    center = cellPosition,

                                };
                                //add to cell dic inside chunk
                                newChunk.cells.Add(cellIndex, newCell);

                                
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
        Chunk exampleChunk = chunks.First().Value;
        GameObject chunkHolder = new GameObject();
        chunkHolder.name = "Chunk holder";

        //Loop through all chunks in render distance
        for (int i = 0; i < totalWorldRenderDistance; i++)
        {
            #region Chunks
            ChunkPrefab newChunkPrefab = new ChunkPrefab()
            {
                center = exampleChunk.center,
                cells = new Dictionary<Vector3Int, GameObject>()
            };
            //add chunks to object pool
            chunkPrefabs.Add(new Vector3Int(i,0,0), newChunkPrefab);

            var chunk = new GameObject();
            chunk.transform.parent = chunkHolder.transform;
            chunk.transform.position = chunkPrefabs.First().Value.center;
            chunk.name = "Chunk [" + chunk.transform.position.x + ", " + chunk.transform.position.y + ", " + chunk.transform.position.z + "]";
            newChunkPrefab.parentObject = chunk;

            #endregion Chunks

            #region Cells

            Vector3Int min = new Vector3Int((int)chunk.transform.position.x - halfChunkSize + halfCellSize, (int)chunk.transform.position.y - halfChunkSize + halfCellSize, (int)chunk.transform.position.z - halfChunkSize + halfCellSize);
            Vector3Int max = new Vector3Int((int)chunk.transform.position.x + halfChunkSize - halfCellSize, (int)chunk.transform.position.y + halfChunkSize - halfCellSize, (int)chunk.transform.position.z + halfChunkSize - halfCellSize);

            for (int cx = min.x; cx <= max.x; cx += cellSize)
            {
                for (int cy = min.y; cy <= max.y; cy += cellSize)
                {
                    for (int cz = min.z; cz <= max.z; cz += cellSize)
                    {
                        //create cell
                        Vector3Int cellKey = GridFunctions.QuantizeFloatToInt(new Vector3(cx, cy, cz), cellSize);

                        //create cell object pool
                        var cell = Instantiate(cellPrefab);
                        cell.transform.position = new Vector3(cx, cy, cz);
                        cell.transform.SetParent(chunk.transform);
                        cell.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
                        chunkPrefabs[new Vector3Int(i, 0, 0)].cells.Add(cellKey, cell);
                    }
                }
            }

            #endregion Cells
        }
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

        if(worldChunkRadius == 0)
        {
            Debug.LogError("WorldChunkRadius must be a non-zero value.");
            Application.Quit(1);
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }

    void PositionObjectPool()
    {
        ReturnClosestChunks();
    }

    private HashSet<Vector3Int> ReturnClosestChunks()
    {
        HashSet<Vector3Int> closestChunks = new HashSet<Vector3Int>();
        return closestChunks;
    }

}



