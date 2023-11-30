//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.UIElements;

////struct Cell
////{
////    public bool isVisited;
////    public Vector3Int position;
////    public List<GameObject> stuff;
////}

////struct Chunk
////{
////    public Vector3Int position;
////    public Dictionary<Vector3Int, Cell> cells;
////}

//public class SpawnGrid : MonoBehaviour
//{
//    int visableCellNumber = 0;
//    int visableChunkNumber = 0;
//    int totalChunks = 0;
//    int totalCells = 0;


//    [SerializeField] private int chunkWorldRadius;
//    private int totalWorldSizeInChunks;
//    private int chunkSize = 10;
//    private int cellSize = 1;
//    [SerializeField] private int chunkRenderDistance;

//    [SerializeField] private GameObject cellPrefab;

//    private Dictionary<Vector3Int, GameObject> chunkPrefabObjectPool;
//    private Dictionary<Vector3Int, Chunk> chunks;
//    HashSet<Chunk> renderedChunks = new HashSet<Chunk>();
//    Vector3Int currentPlayerChunk = Vector3Int.zero;
//    Vector3Int oldPlayerChunk = Vector3Int.zero;


//    GameObject player;

//    private void Awake()
//    {
//        player = GameObject.Find("Player");
//    }

//    void Start()
//    {
//        //needs to exist before adding to it
//        chunks = new Dictionary<Vector3Int, Chunk>();
//        HashSet<Vector3Int> newRenderedChunks = new HashSet<Vector3Int>();
//        chunkPrefabObjectPool = new Dictionary<Vector3Int, GameObject>();

//        //set world Size
//        totalWorldSizeInChunks = chunkWorldRadius * 2;

//        //create grid
//        CreateGrid();
//        RenderGrid();
//    }

//    private void Update()
//    {
//        //Debug.Log("Visable Cells: " + visableCellNumber);
//        //Debug.Log("Visable Chunks: " + visableChunkNumber);
//        //Debug.Log("Total Chunks: " + totalChunks);
//        //Debug.Log("Total Cells: " + totalCells);

//        ////Calculate position of player
//        //currentPlayerChunk = QuantizeFloatToInt(player.transform.position, chunkSize);

//        ////check whether we need to check for chunk changes
//        ////if we haven't moved chunks, do not render new
//        //if (currentPlayerChunk != oldPlayerChunk)
//        //{
//        //    oldPlayerChunk = QuantizeFloatToInt(player.transform.position, chunkSize);
//        //    RenderGrid();
//        //}
//    }

//    void CreateGrid()
//    {
//        CreateChunksAndCells();
//    }

//    void CreateChunksAndCells()
//    {
//        //Go through number of chunks in world
//        for (int x = -chunkWorldRadius; x < chunkWorldRadius; x++)
//        {
//            for (int y = -chunkWorldRadius; y < chunkWorldRadius; y++)
//            {
//                for (int z = -chunkWorldRadius; z < chunkWorldRadius; z++)
//                {
//                    //chunks /////////////////////////////////////////////
//                    totalChunks++;

//                    Vector3Int chunkPosition = GridFunctions.QuantizeFloatToInt(new Vector3(x * chunkSize, y * chunkSize, z * chunkSize), chunkSize);
//                    Chunk newChunk = new Chunk()
//                    {
//                        position = chunkPosition,
//                        cells = new Dictionary<Vector3Int, Cell>()
//                    };

//                    //add the chunks to the dic
//                    chunks.Add(chunkPosition, newChunk);

//                    //cells //////////////////////////////
//                    //Get cell dict from chunk
//                    Dictionary<Vector3Int, Cell> cellList = newChunk.cells;

//                    //create all cells within this chunk
//                    List<Cell> createdCells = CreateCells(chunkPosition, new Vector3Int(chunkPosition.x + chunkSize, chunkPosition.y + chunkSize, chunkPosition.z + chunkSize));

//                    //set position to cell dict in chunk
//                    foreach (Cell cell in createdCells)
//                    {
//                        cellList.Add(cell.position, cell);
//                    }
//                }
//            }
//        }
//    }

//    List<Cell> CreateCells(Vector3Int min, Vector3Int max)
//    {
//        List<Cell> cellList = new List<Cell>();

//        for (int x = min.x; x < max.x; x += cellSize)
//        {
//            for (int y = min.y; y < max.y; y += cellSize)
//            {
//                for (int z = min.z; z < max.z; z += cellSize)
//                {
//                    Cell newCell = new Cell()
//                    {
//                        position = GridFunctions.QuantizeFloatToInt(new Vector3(x, y, z), cellSize),
//                    };

//                    //add to cell dic inside chunk
//                    cellList.Add(newCell);

//                    //create actual object into cell object pool
//                    var obj = Instantiate(cellPrefab);
//                    obj.transform.position = newCell.position;
//                    chunkPrefabObjectPool.Add(newCell.position, obj);

//                    totalCells++;
//                }
//            }
//        }

//        return cellList;
//    }

//    private void RenderGrid()
//    {
//        //Find closest chunks to player
//        HashSet<Chunk> closeChunks = ReturnClosestChunks(chunkRenderDistance, player.transform.position);
//        HashSet<Chunk> updatedRenderedChunks = new HashSet<Chunk>();

//        //create list for chunks to move
//        List<Chunk> chunksToMove = new List<Chunk>();

//        //Find the rendered chunks that are not inside closest chunks
//        //These are the ones we need to change position
//        foreach (Chunk renderedChunk in renderedChunks)
//        {
//            if (!closeChunks.Contains(renderedChunk))
//            {
//                chunksToMove.Add(renderedChunk);
//            }
//        }

//        //Find chunks that are not being rendered
//        //Chunks in closest but not in rendered
//        foreach (Chunk closeChunk in closeChunks)
//        {
//            if (!renderedChunks.Contains(closeChunk))
//            {
//                MoveChunk(closeChunk, closeChunk.position); // Assuming MoveChunk renders the chunk
//            }
//            // Add the chunk to the updated list as it's now rendered
//            updatedRenderedChunks.Add(closeChunk);
//        }

//        //update rendered chunks
//        renderedChunks = updatedRenderedChunks;
//    }

//    HashSet<Chunk> ReturnClosestChunks(int renderDistance, Vector3 playerPosition)
//    {
//        HashSet<Chunk> closestChunksList = new HashSet<Chunk>();
//        Chunk playerChunk = chunks[GridFunctions.QuantizeFloatToInt(player.transform.position, chunkSize)];

//        //calculate min/max on the number of chunks we need to add
//        int xMax = playerChunk.position.x + (chunkRenderDistance * chunkSize);
//        int yMax = playerChunk.position.y + (chunkRenderDistance * chunkSize);
//        int zMax = playerChunk.position.z + (chunkRenderDistance * chunkSize);

//        int xMin = playerChunk.position.x - (chunkRenderDistance * chunkSize);
//        int yMin = playerChunk.position.y - (chunkRenderDistance * chunkSize);
//        int zMin = playerChunk.position.z - (chunkRenderDistance * chunkSize);

//        //Find which chunks depending on the position of where we are to render
//        //Go through number of chunks in world
//        for (int x = xMin; x < xMax; x += chunkSize)
//        {
//            for (int y = yMin; y < yMax; y += chunkSize)
//            {
//                for (int z = zMin; z < zMax; z += chunkSize)
//                {
//                    Vector3Int thisChunkPosition = GridFunctions.QuantizeFloatToInt(new Vector3(x, y, z), chunkSize);

//                    // If the chunk does not exist, skip to the next iteration
//                    if (!chunks.ContainsKey(thisChunkPosition))
//                    {
//                        continue;
//                    }

//                    //add to closest chunks position
//                    Chunk thisChunk = chunks[thisChunkPosition];
//                    closestChunksList.Add(thisChunk);
//                }
//            }
//        }


//        return closestChunksList;
//    }

//    //moves chunk to another region
//    void MoveChunk(Chunk chunk, Vector3Int newPosition)
//    {
//        chunk.position = newPosition;
//        MoveCells(chunk, newPosition, new Vector3Int(newPosition.x + chunkSize, newPosition.y + chunkSize, newPosition.z + chunkSize));
//    }

//    //moves cells to another region, depending on position of chunk
//    void MoveCells(Chunk chunk, Vector3Int min, Vector3Int max)
//    {
//        for (int x = min.x; x < max.x; x += cellSize)
//        {
//            for (int y = min.y; y < max.y; y += cellSize)
//            {
//                for (int z = min.z; z < max.z; z += cellSize)
//                {
//                    //TODO: this
//                    //chunk.cells[new Vector3Int(x,y,z)] = new Vector3Int(x, y, z);
//                }
//            }
//        }
//    }
//}