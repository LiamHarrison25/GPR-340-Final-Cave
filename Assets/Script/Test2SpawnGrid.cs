using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test2SpawnGrid : MonoBehaviour
{
    [SerializeField] private int worldChunkRadius;
    [SerializeField] private int chunkRenderDistanceRadius;
    [SerializeField] private GameObject cellPrefab;

    private int chunkSize = 2;
    private int cellSize = 1;
    private int totalWorldChunkSize = 0;
    private int totalWorldRenderDistance = 0;

    GameObject player;
    Vector3 playerOldChunk;

    private Dictionary<Vector3Int, ChunkPrefab> ChunkPrefab;
    private Dictionary<Vector3Int, Chunk> chunks;

    // Start is called before the first frame update
    void Start()
    {
        chunks = new Dictionary<Vector3Int, Chunk>();
        ChunkPrefab = new Dictionary<Vector3Int, ChunkPrefab>();

        totalWorldChunkSize = worldChunkRadius * 2;
        totalWorldChunkSize = totalWorldChunkSize * totalWorldChunkSize * totalWorldChunkSize;

        totalWorldRenderDistance = chunkRenderDistanceRadius * 2;
        totalWorldRenderDistance = totalWorldRenderDistance * totalWorldRenderDistance * totalWorldRenderDistance;

        //initially set to itself
        playerOldChunk = player.transform.position;

        GenerateChunks();
        GenerateChunksObjectPool();
        PositionObjectPool(false);

        OnDrawGizmosSelected();
    }

    // Update is called once per frame
    void Update()
    {
        //PositionObjectPool(true);
    }

    private void Awake()
    {
        player = GameObject.Find("Player");
    }

    private void GenerateChunks()
    {
        //Loop through all chunks in world
        for(int x = -worldChunkRadius * chunkSize; x < worldChunkRadius * chunkSize; x += chunkSize)
        {
            for(int y = -worldChunkRadius * chunkSize; y < worldChunkRadius * chunkSize; y += chunkSize)
            {
                for (int z = -worldChunkRadius * chunkSize; z < worldChunkRadius * chunkSize; z += chunkSize)
                {
                    //every single chunk passes through here
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
                    var newChunkCells = newChunk.cells;

                    Vector3Int min = chunkPosition;
                    Vector3Int max = new Vector3Int(chunkPosition.x + chunkSize, chunkPosition.y + chunkSize, chunkPosition.z + chunkSize);

                    for (int cx = min.x; cx < max.x; cx += cellSize)
                    {
                        for (int cy = min.y; cy < max.y; cy += cellSize)
                        {
                            for (int cz = min.z; cz < max.z; cz += cellSize)
                            {
                                //create cell
                                Vector3Int cellPosition = GridFunctions.QuantizeFloatToInt(new Vector3(cx, cy, cz), cellSize);
                                Cell newCell = new Cell()
                                {
                                    position = cellPosition,
                                };
                                //add to cell dic inside chunk
                                newChunkCells.Add(cellPosition, newCell);
                            }
                        }
                    }
                }
            }
        }
    }

    void GenerateChunksObjectPool()
    {
        //Loop through all chunks in render distance
        for (int i = 0; i < totalWorldRenderDistance; i++)
        {
            //create chunks ----------------------------
            Vector3Int chunkPosition = new Vector3Int(i * chunkSize, 0, 0);
            ChunkPrefab newChunkPrefab = new ChunkPrefab()
            {
                position = chunkPosition,
                cells = new Dictionary<Vector3Int, GameObject>()
            };
            //add chunks to object pool
            ChunkPrefab.Add(chunkPosition, newChunkPrefab);

            //create cells ----------------------------
            var newChunkCellsObjectPool = newChunkPrefab.cells;

            //create object pool parent chunk game object
            var chunkParent = new GameObject();
            chunkParent.transform.position = chunkPosition;
            chunkParent.name = "Chunk [" + chunkPosition.x + ", " + chunkPosition.y + ", " + chunkPosition.z + "]";

            newChunkPrefab.parentObject = chunkParent;

            Vector3Int min = chunkPosition;
            Vector3Int max = new Vector3Int(chunkPosition.x + chunkSize, chunkPosition.y + chunkSize, chunkPosition.z + chunkSize);

            for (int cx = min.x; cx < max.x; cx += cellSize)
            {
                for (int cy = min.y; cy < max.y; cy += cellSize)
                {
                    for (int cz = min.z; cz < max.z; cz += cellSize)
                    {
                        //create cell
                        Vector3Int cellPosition = GridFunctions.QuantizeFloatToInt(new Vector3(cx, cy, cz), cellSize);

                        //create cell object pool
                        var obj = Instantiate(cellPrefab);
                        obj.transform.position = cellPosition;
                        obj.transform.SetParent(chunkParent.transform);
                        newChunkCellsObjectPool.Add(cellPosition, obj);
                    }
                }
            }
        }
    }

    void PositionObjectPool(bool checkForChunkChange)
    {
        //check if player has moved, if so, continue
        if (checkForChunkChange)
        {
            if (!HasPlayerMovedChunks())
            {
                return;
            }
        }

        HashSet<Vector3Int> closestChunks = ReturnClosestChunks();
        List<Vector3Int> addThisChunkPosition = new List<Vector3Int>();
        Stack<ChunkPrefab> removeChunkPositions = new Stack<ChunkPrefab>();

        int removeNum = 0, addNum = 0;

        foreach(var c in closestChunks)
        {
            Debug.Log(c);
        }

        //check which chunks have changed
        foreach (Vector3Int closeChunk in closestChunks)
        {
            //if its not inside our object pool add to list
            if (!ChunkPrefab.ContainsKey(closeChunk))
            {
                //add chunks to change
                addThisChunkPosition.Add(closeChunk);
                addNum++;
                
            }
        }

        //check which chunks to remove from object pool
        foreach (var entry in ChunkPrefab)
        {
            //if pool does not contain the key 
            if (!closestChunks.Contains(entry.Key))
            {
                removeChunkPositions.Push(entry.Value);
                removeNum++;
            }
        }

        Debug.Log("add " + addNum);
        Debug.Log("remove " + removeNum);


        //change position of objects in object pool
        foreach (Vector3Int addThisPosition in addThisChunkPosition)
        {
            //grab a ChunkPrefab from the remove list. steal it
            //set it to a different value

            ChunkPrefab chunkObject = new ChunkPrefab(); // Declare a variable for the output
            Vector3Int removedChunkKey = removeChunkPositions.Pop().position; // Get the key from the popped ChunkPrefab

            //steal deleted chunks items
            chunkObject = ChunkPrefab[removedChunkKey];

            //correct it's position
            chunkObject.position = addThisPosition;
            chunkObject.parentObject.transform.position = addThisPosition;

            //add new chunk with the stolen items, and set the required position
            ChunkPrefab.Add(addThisPosition, chunkObject);

            // Remove the old entry and add the new one
            ChunkPrefab.Remove(removedChunkKey);
        }
    }

    private HashSet<Vector3Int> ReturnClosestChunks()
    {
        HashSet<Vector3Int> closestChunks = new HashSet<Vector3Int>();
        Vector3Int playerChunk = GridFunctions.QuantizeFloatToInt(player.transform.position, chunkSize);

        //calculate min/max on the number of chunks we need to add
        int xMax = playerChunk.x + (chunkRenderDistanceRadius * chunkSize);
        int yMax = playerChunk.y + (chunkRenderDistanceRadius * chunkSize);
        int zMax = playerChunk.z + (chunkRenderDistanceRadius * chunkSize);
        int xMin = playerChunk.x - (chunkRenderDistanceRadius * chunkSize);
        int yMin = playerChunk.y - (chunkRenderDistanceRadius * chunkSize);
        int zMin = playerChunk.z - (chunkRenderDistanceRadius * chunkSize);

        //Find which chunks depending on the position of where we are to render
        //Go through number of chunks in world
        for (int x = xMin; x < xMax; x += chunkSize)
        {
            for (int y = yMin; y < yMax; y += chunkSize)
            {
                for (int z = zMin; z < zMax; z += chunkSize)
                {
                    Vector3Int thisChunkPosition = GridFunctions.QuantizeFloatToInt(new Vector3(x, y, z), chunkSize);
                    closestChunks.Add(thisChunkPosition);
                }
            }
        }

        //remember to add player chunk
        closestChunks.Add(playerChunk);

        return closestChunks;
    }
    
    //if player has changed chunks
    bool HasPlayerMovedChunks()
    {
        Vector3 playerCurrentChunk = GridFunctions.QuantizeFloatToInt(player.transform.position, chunkSize);
   
        if(playerOldChunk != playerCurrentChunk)
        {
            playerOldChunk = playerCurrentChunk;
            return true;
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        foreach(var entry in chunks)
        {
            Gizmos.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 0.5f);

            int chunkSizeRadius = (chunkSize / 2);
            Gizmos.DrawCube((new Vector3Int(entry.Key.x + chunkSizeRadius, entry.Key.y + chunkSizeRadius, entry.Key.z + chunkSizeRadius)),
                new Vector3(chunkSize, chunkSize, chunkSize));
        }
    }
}   