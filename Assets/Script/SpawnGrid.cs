using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SpawnGrid : MonoBehaviour
{
    public int worldChunkRadius;  //radius in units of chunks
    [SerializeField] private int chunkRenderDistanceRadius; // radius of chunks that gets rendered. 
    public int halfChunkSize;  //stores in half size, so we can avoid division
    public int halfCellSize;  //half cell size MUST be divisable inside half chunk size
    [SerializeField] private GameObject cellPrefab;

    private int chunkSize;
    private int cellSize;
    private int totalWorldRenderedChunks = 0;

    //test statistics
    private int totalWorldChunks;
    private int totalWorldRenderedCells;
    public int totalWorldCells { get; private set; }

    GameObject player;
    Vector3 playerOldChunk;

    CaveGenerator CaveGeneratorScript;

    private Dictionary<Vector3Int, ChunkPrefab> chunkPrefabs;
    public Dictionary<Vector3Int, Chunk> chunks { get; private set; }

    private void Awake()
    {
        CaveGeneratorScript = GetComponent<CaveGenerator>(); 
    }

    void Start()
    {
        Checks(); //Checks to make sure that the sizes of the chunks and cells lines up correctly. 

        player = GameObject.Find("Player");

        totalWorldChunks = worldChunkRadius * 2;
        totalWorldChunks = (int)Mathf.Pow(totalWorldChunks, 3);

        totalWorldRenderedChunks = chunkRenderDistanceRadius * 2;
        totalWorldRenderedChunks = (int)Mathf.Pow(totalWorldRenderedChunks, 3);

        chunkPrefabs = new Dictionary<Vector3Int, ChunkPrefab>();
        chunks = new Dictionary<Vector3Int, Chunk>();
        chunkSize = halfChunkSize * 2;
        cellSize = halfCellSize * 2;

        GenerateChunks();
        GenerateChunksObjectPool();
        PositionObjectPool(false);

        //generate the cave
        CaveGeneratorScript.GenerateCaves();
        UpdateCells();

        ShowStatistics();
    }
    

    void Update()
    {
        PositionObjectPool(true);
    }

    //Generates the chunks in the world
    void GenerateChunks()
    {
        int x, y, z;
        int middleChunk = chunkSize;
        int min = -(worldChunkRadius * chunkSize) + middleChunk;
        int max = (worldChunkRadius * chunkSize) + middleChunk;

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
                        key = chunkIndex,
                        cells = new Dictionary<Vector3Int, Cell>()
                    };

                    //this is specifically for object pool
                    chunks.Add(chunkIndex, newChunk);
                    #endregion chunks

                    #region cells
                    int middleCell = cellSize;
                    Vector3Int minimum = new Vector3Int(x - halfChunkSize + halfCellSize, y - halfChunkSize + halfCellSize, z - halfChunkSize + halfCellSize);
                    Vector3Int maximum = new Vector3Int(x + halfChunkSize - halfCellSize, y + halfChunkSize - halfCellSize, z + halfChunkSize - halfCellSize);
                    float cx, cy, cz;
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
                                    key = cellIndex,
                                };
                                //add to cell dic inside chunk
                                newChunk.cells.Add(cellIndex, newCell);
                                totalWorldCells++; //Statistics
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
        for (int i = 0; i < totalWorldRenderedChunks; i++)
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

            Color randomColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 0.5f);

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
                        Renderer renderer = cell.gameObject.GetComponent<Renderer>();
                        renderer.material.color = randomColor;
                        totalWorldRenderedCells++;
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

        if(worldChunkRadius == chunkRenderDistanceRadius)
        {
            Debug.LogError("worldChunkRadius cannot be the same number as chunkRenderDistanceRadius");
            Application.Quit();
            UnityEditor.EditorApplication.isPlaying = false;
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
        HashSet<Vector3Int> closestChunksKeys = ReturnClosestChunks();

        //create a list of some sort to find chunks to add
        Stack<Vector3Int> chunkKeysToAdd = new Stack<Vector3Int>();

        //create a list of some sort to find chunks to remove
        Stack<Vector3Int> chunkKeysToRemove = new Stack<Vector3Int>();

        //list of chunk keys we need to add
        foreach(Vector3Int closeChunkkey in closestChunksKeys)
        {
            //if its not inside our object pool add to list
            if (!chunkPrefabs.ContainsKey(closeChunkkey))
            {
                chunkKeysToAdd.Push(closeChunkkey);
            }
        }

        //list of chunk keys we need to remove
        foreach (var entry in chunkPrefabs)
        {
            if (!closestChunksKeys.Contains(entry.Key))
            {
                chunkKeysToRemove.Push(entry.Key);
            }
        }

        int numChunksChanged = chunkKeysToAdd.Count();

        if(chunkKeysToAdd.Count() != chunkKeysToRemove.Count())
        {
            Debug.Log("CHUNKS TO ADD AND CHUNKS TO REMOVE ARE NOT EQUAL! THIS IS A PROBLEMM IN LINE 220");
        }

        int i;
        for (i = 0; i < numChunksChanged; i++)
        {
            //Removing the chunk
            Vector3Int removeChunkKey = chunkKeysToRemove.Pop();
            ChunkPrefab newChunk = chunkPrefabs[removeChunkKey];
            chunkPrefabs.Remove(removeChunkKey);

            //Adding the chunk
            Vector3Int addChunkKey = chunkKeysToAdd.Pop();
            newChunk.center = chunks[addChunkKey].center;
            newChunk.parentObject.transform.position = chunks[addChunkKey].center; // might be an issue later with vector 3 INT position
            chunkPrefabs.Add(addChunkKey, newChunk);
        }
    }

    private HashSet<Vector3Int> ReturnClosestChunks()
    {
        HashSet<Vector3Int> closestChunks = new HashSet<Vector3Int>();

        Vector3 playerPosition = player.transform.position;
        //calculate min/max on the number of chunks we need to add
        int xMax = (int)playerPosition.x + (chunkRenderDistanceRadius * chunkSize);
        int yMax = (int)playerPosition.y + (chunkRenderDistanceRadius * chunkSize);
        int zMax = (int)playerPosition.z + (chunkRenderDistanceRadius * chunkSize);
        int xMin = (int)playerPosition.x - (chunkRenderDistanceRadius * chunkSize);
        int yMin = (int)playerPosition.y - (chunkRenderDistanceRadius * chunkSize);
        int zMin = (int)playerPosition.z - (chunkRenderDistanceRadius * chunkSize);

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
        Vector3Int playerChunk = GridFunctions.QuantizeFloatToInt(playerPosition, chunkSize);
        closestChunks.Add(playerChunk);

        return closestChunks;
    }

    bool HasPlayerMovedChunks()
    {
        Vector3 playerCurrentChunk = GridFunctions.QuantizeFloatToInt(player.transform.position, chunkSize);

        if (playerOldChunk != playerCurrentChunk)
        {
            playerOldChunk = playerCurrentChunk;
            return true;
        }

        return false;
    }

    void ShowStatistics()
    {
        //single chunk / area of 1 cell
        totalWorldRenderedCells = (int)(((Mathf.Pow(chunkSize, 3)) / (Mathf.Pow(cellSize, 3))) * totalWorldRenderedChunks);

        Debug.Log("Total world chunks: " + totalWorldChunks);
        Debug.Log("Total world cells: " + totalWorldCells);

        Debug.Log("Total rendered chunks: " + totalWorldRenderedChunks);
        Debug.Log("Total rendered cells: " + totalWorldRenderedCells);

        int testInt = (totalWorldChunks * (int)(Mathf.Pow(chunkSize, 3) / Mathf.Pow(cellSize, 3)));
        Debug.Log("test: " + testInt);
    }

    void UpdateCells()
    {
        int x, y, z;
        int middleChunk = chunkSize;
        int min = -(worldChunkRadius * chunkSize) + middleChunk;

        int max = (worldChunkRadius * chunkSize) + middleChunk;
        for (x = min; x <= max; x += chunkSize)
        {
            for (y = min; y <= max; y += chunkSize)
            {
                for (z = min; z <= max; z += chunkSize)
                {
                    Vector3Int currentChunkKey = GridFunctions.QuantizeFloatToInt(new Vector3(x, y, z), chunkSize);

                    ChunkPrefab chunkPrefab = chunkPrefabs[currentChunkKey];
                    Chunk chunk = chunks[currentChunkKey];

                    Dictionary<Vector3Int, GameObject> cellsPrefabDic = chunkPrefab.cells;
                    Dictionary<Vector3Int, Cell> cellsDic = chunk.cells;

                    //loop through all real cells
                    foreach (Vector3Int key in cellsDic.Keys)
                    {
                        //find out if the cell isActive or not inside the dictionary of chunks
                        Cell theCell = cellsDic[key];
                        if (theCell.isActive)
                        {
                            cellsPrefabDic[key].SetActive(true);
                            Debug.Log("turning on cell");
                            Debug.Log("key: " + key + "current chunky" + currentChunkKey);
                        }   
                        else
                        {
                            cellsPrefabDic[key].SetActive(false);
                            Debug.Log("turning off cell");
                        }
                    }
                }
            }
        }
    }
}