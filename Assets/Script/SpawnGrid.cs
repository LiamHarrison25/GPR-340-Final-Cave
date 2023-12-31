using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnGrid : MonoBehaviour
{
    public int worldChunkRadius;  // total world radius in units of chunks
    [SerializeField] private int chunkRenderDistanceRadius; // radius of chunks that gets rendered around the player
    public int halfChunkSize;  //stores in half size, so we can avoid division
    public int halfCellSize;  //half cell size MUST be divisible inside half chunk size
    [SerializeField] private GameObject cellPrefab;

    [SerializeField] private bool isStatisticsEnabled = false;

    private int chunkSize;
    private int cellSize;
    private int totalWorldRenderedChunks = 0;

    private bool isGizmosEnabled = false;

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

        foreach(var chunkPrefab in chunkPrefabs)
        {
            foreach(var cellKey in chunkPrefab.Value.cells.Keys)
            {
                //check if each cell in prefabs exists in our other dic
                if (chunks.ContainsKey(chunkPrefab.Key))
                {
                    //the chunk we need in dictionary
                    Chunk dictionaryChunk = chunks[chunkPrefab.Key];
                    if (!dictionaryChunk.cells.ContainsKey(cellKey))
                    {
                        Debug.Log("Chunk Key does not exist");
                    }
                }
                else
                {
                    Debug.Log("Chunk Key does not exist");
                }
            }
        }
        //Only occurs if the user wants to show the statistics
        if (isStatisticsEnabled) ShowStatistics();
    }
    
    void Update()
    {
        PositionObjectPool(true);

        if (Input.GetKeyDown(KeyCode.F))
        {
            isGizmosEnabled = !isGizmosEnabled;
        }
    }

    /// <summary>
    ///
    /// This function generates the chunks.
    /// These are the chunk objects, not the Unity Object Chunks.
    /// 
    /// </summary>
    void GenerateChunks()
    {
        //create the minimum and maximum positions of where the chunk should spawn (includes negative values in unity world grid)
        int middleChunk = chunkSize;
        int min = -(worldChunkRadius * chunkSize) + middleChunk; //add middle chunk to fix grid alignment issue
        int max = (worldChunkRadius * chunkSize) + middleChunk;

        int x, y, z;
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
   
    /// <summary>
    ///
    /// This function creates the cells in each of the chunks.
    /// The positions of these cells are initially set to junk
    /// 
    /// </summary>
    void GenerateChunksObjectPool()
    {
        //Debug.Log("example chunk key, initial = " + chunks.First().Key);
        GameObject chunkHolder = new GameObject();
        chunkHolder.name = "Chunk holder";

        Chunk exampleChunk = chunks.First().Value;

        //Loop through all chunks in render distance
        int i;
        for (i = 0; i < totalWorldRenderedChunks; i++)
        {
            #region Chunks
            ChunkPrefab newChunkPrefab = new ChunkPrefab()
            {
                center = exampleChunk.center,
                cells = new Dictionary<Vector3Int, GameObject>()
            };
            //add chunks to object pool
            chunkPrefabs.Add(new Vector3Int(999999999,i,777777777), newChunkPrefab);

            //create new Chunk, to swap out the old
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
            int cx, cy, cz;
            for (cx = min.x; cx <= max.x; cx += cellSize)
            {
                for (cy = min.y; cy <= max.y; cy += cellSize)
                {
                    for (cz = min.z; cz <= max.z; cz += cellSize)
                    {
                        //create cell
                        Vector3Int cellKey = GridFunctions.QuantizeFloatToInt(new Vector3(cx, cy, cz), cellSize);

                        //create cell object pool
                        var cell = Instantiate(cellPrefab);
                        cell.transform.position = new Vector3(cx, cy, cz);
                        cell.transform.SetParent(chunk.transform);
                        cell.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
                        chunkPrefabs[new Vector3Int(999999999, i, 777777777)].cells.Add(cellKey, cell);
                        Renderer renderer = cell.gameObject.GetComponent<Renderer>();
                        renderer.material.color = randomColor;
                        totalWorldRenderedCells++;
                    }
                }
            }
            #endregion Cells
        }
    }

    /// <summary>
    ///
    /// This function draws a grid that displays the total world size in chunks
    /// 
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (isGizmosEnabled)
        {
            foreach (var entry in chunks)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.1f);
                Gizmos.DrawWireCube((new Vector3(entry.Value.center.x, entry.Value.center.y, entry.Value.center.z)),
                    new Vector3(chunkSize, chunkSize, chunkSize));
            }
        }
        
    }

    /// <summary>
    ///
    /// This function ensures that the user inputs correct values into the serialized fields
    /// If invalid values are inputted, the program will quit and send an error
    /// 
    /// </summary>
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

        if (chunkRenderDistanceRadius >= worldChunkRadius)
        {
            Debug.LogError("worldChunkRadius must be larger than chunkWorldRenderDistance");
            Application.Quit(1);
            UnityEditor.EditorApplication.isPlaying = false;
        }

    }

    /// <summary>
    ///
    /// This function moves the chunks and cells to their correct positions.
    /// 
    /// </summary>
    /// <param name="checkForChunkChange"></param>
    public void PositionObjectPool(bool checkForChunkChange)
    {
        CheckIfPlayerIsInGrid();

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
            Debug.Log("CHUNKS TO ADD AND CHUNKS TO REMOVE ARE NOT EQUAL! THIS IS A PROBLEM IN POSITION GAME OBJECT");
        }

        int i;
        for (i = 0; i < numChunksChanged; i++)
        {
            //Adding the chunk
            Vector3Int addChunkKey = chunkKeysToAdd.Pop();

            //check if the chunk should move in case it spawns outside grid
            if (!chunks.TryGetValue(addChunkKey, out Chunk chunk))
            {
                continue;
            }

            //Removing the chunk
            Vector3Int removeChunkKey = chunkKeysToRemove.Pop();
            ChunkPrefab newChunk = chunkPrefabs[removeChunkKey];
            chunkPrefabs.Remove(removeChunkKey);

            newChunk.center = chunks[addChunkKey].center;
            newChunk.parentObject.transform.position = chunks[addChunkKey].center; // might be an issue later with vector 3 INT position
            chunkPrefabs.Add(addChunkKey, newChunk);

            //automatically rename the chunk depending on where it is in the hierarchy
            newChunk.parentObject.name = "Chunk [" + addChunkKey.x + ", " + addChunkKey.y + ", " + addChunkKey.z + "]";

            //set keys for all cells
            //use the dictionary that already exists as we know the chunk we need
            var cellPrefabDict = chunkPrefabs[addChunkKey].cells;
            var cellDict = chunks[addChunkKey].cells;

            Stack<GameObject> cellPrefabObjectStack = new Stack<GameObject>();

            //grab all keys in cell prefabs, cuz they all need to be changed
            foreach (var cellPrefab in cellPrefabDict) 
            {
                cellPrefabObjectStack.Push(cellPrefab.Value);
            }
            cellPrefabDict.Clear();

            //set cell keys correctly:
            foreach (var cellKey in cellDict.Keys) //Maybe change this from var to gameobject
            {
                //steal gameObject
                GameObject stolenGameobject = cellPrefabObjectStack.Pop();
                cellPrefabDict.Add(cellKey, stolenGameobject);
            }
        }
    }


    /// <summary>
    ///
    /// This function returns the closest chunks to the player GameObject
    /// 
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    ///
    /// This function detects if the player has moved chunks
    /// 
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    ///
    /// Optional function that will log how many of each object there is in the scene
    /// 
    /// </summary>
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


    /// <summary>
    ///
    /// Function that detects if a cell should be turned on or off
    /// 
    /// </summary>
    public void UpdateCells()
    {
        //loop through all rendered chunk prefabs
        //loop through all rendered cells
        foreach(var chunkPrefab in chunkPrefabs)
        {
            Vector3Int chunkKey = chunkPrefab.Key;

            Dictionary<Vector3Int, GameObject> cellDict = chunkPrefab.Value.cells;

            foreach (var cell in cellDict)
            {
                //find out if this cell is on or off
                Cell worldDictionaryCell = chunks[chunkKey].cells[cell.Key];
                bool isCellOn = worldDictionaryCell.isCellOn;
                cell.Value.gameObject.SetActive(isCellOn);
            }
        }
    }


    /// <summary>
    ///
    /// Function that checks if the player is in the grid.
    /// Used for preventing the player from getting outside the world border
    /// 
    /// </summary>
    private void CheckIfPlayerIsInGrid()
    {
        //if key doesn't exist
        Vector3Int playerPos = GridFunctions.QuantizeFloatToInt(player.transform.position, chunkSize);

        Vector3Int upCheck = Vector3Int.one * chunkRenderDistanceRadius;
        
        if (!chunks.TryGetValue(playerPos, out Chunk x)) 
        {
            RecallPlayerToLastChunk();
        }
    }


    /// <summary>
    ///
    /// Function that moves the player back to their previous chunk
    /// 
    /// </summary>
    private void RecallPlayerToLastChunk()
    {
        player.transform.position = playerOldChunk;
        Debug.Log("Moved player back");
    }
}