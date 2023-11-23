using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

struct Cell
{
    public bool isVisited;
    public Vector3Int position;
    public List<GameObject> stuff;
}

struct Chunk
{
    public Vector3Int position;
    public Dictionary<Vector3Int, Cell> cells;
}

public class SpawnGrid : MonoBehaviour
{
    int visableCellNumber = 0;
    int visableChunkNumber = 0;
    int totalChunks = 0;
    int totalCells = 0;

    [SerializeField] private int chunkWorldRadius = 10;
    private int totalWorldSizeInChunks;
    private int chunkSize = 10;
    private float cellSize = 1;
    private int chunkRenderDistance = 3;

    [SerializeField] private GameObject cellPrefab;

    private Dictionary<Vector3Int, Chunk> chunks;

    private HashSet<Vector3Int> renderedChunks = new HashSet<Vector3Int>();

    GameObject player;

    private void Awake()
    {
        player = GameObject.Find("Player");
    }

    //convert vector3f to vector3int
    Vector3Int QuantizeFloatToInt(Vector3 position, float cellSize)
    {
        return new Vector3Int(
            (int)Mathf.Floor(position.x / cellSize),
            (int)Mathf.Floor(position.y / cellSize),
            (int)Mathf.Floor(position.z / cellSize));
    }

    void Start()
    {
        //needs to exist before adding to it
        chunks = new Dictionary<Vector3Int, Chunk>();
        HashSet<Vector3Int> newRenderedChunks = new HashSet<Vector3Int>();

        //set world Size
        totalWorldSizeInChunks = chunkWorldRadius * 2;

        //create grid
        CreateGrid();

        RenderGrid();
    }

    private void Update()
    {
        //RenderGrid();
        Debug.Log("Visable Cells: " + visableCellNumber);
        Debug.Log("Visable Chunks: " + visableChunkNumber);
        Debug.Log("Total Chunks: " + totalChunks);
        Debug.Log("Total Cells: " + totalCells);
    }

    void CreateGrid()
    {
        CreateChunksAndCells();
    }

    void CreateChunksAndCells()
    {
        //Go through number of chunks in world
        for (int x = -chunkWorldRadius; x < chunkWorldRadius; x++)
        {
            for (int y = -chunkWorldRadius; y < chunkWorldRadius; y++)
            {
                for (int z = -chunkWorldRadius; z < chunkWorldRadius; z++)
                {
                    //chunks
                    totalChunks++;

                    Vector3Int chunkPosition = QuantizeFloatToInt(new Vector3(x * chunkSize, y * chunkSize, z * chunkSize), chunkSize);
                    Chunk newChunk = new Chunk()
                    {
                        position = chunkPosition,
                        cells = new Dictionary<Vector3Int, Cell>()
                    };

                    chunks.Add(chunkPosition, newChunk);

                    //cells //////////////////////////////
                    //Get cell dict from chunk
                    Dictionary<Vector3Int, Cell> cellList = newChunk.cells;

                    //create all cells within this chunk
                    List<Cell> createdCells = CreateCells(chunkPosition, new Vector3Int(chunkPosition.x * chunkSize, chunkPosition.y * chunkSize, chunkPosition.z * chunkSize));

                    //set position to cell dict in chunk
                    foreach (Cell cell in createdCells)
                    {
                        cellList.Add(cell.position, cell);
                    }
                }
            }
        }
    }

    List<Cell> CreateCells(Vector3Int min, Vector3Int max)
    {
        List<Cell> cellList = new List<Cell>();

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                for (int z = min.z; z <= max.z; z++)
                {
                    Cell newCell = new Cell()
                    {
                        position = QuantizeFloatToInt(new Vector3(x, y, z), cellSize),
                    };

                    cellList.Add(newCell);
                    totalCells++;
                }
            }
        }

        return cellList;
    }

    private void RenderGrid()
    {
        //calculate which chunks are close
        HashSet<Chunk> closeChunks = new HashSet<Chunk>();

        //use chunk render distance to add to closeChunks
        Chunk currentChunk = chunks[QuantizeFloatToInt(player.transform.position, chunkSize)];

        int xMax = currentChunk.position.x + (chunkRenderDistance * chunkSize);
        int yMax = currentChunk.position.y + (chunkRenderDistance * chunkSize);
        int zMax = currentChunk.position.z + (chunkRenderDistance * chunkSize);

        int xMin = currentChunk.position.x - (chunkRenderDistance * chunkSize);
        int yMin = currentChunk.position.y - (chunkRenderDistance * chunkSize);
        int zMin = currentChunk.position.z - (chunkRenderDistance * chunkSize);

        //add chunks to render
        //Go through number of chunks in world
        for (int x = xMin; x < xMax; x += chunkSize)
        {
            for (int y = yMin; y < yMax; y += chunkSize)
            {
                for (int z = zMin; z < zMax; z += chunkSize)
                {
                    Chunk thisChunk = chunks[QuantizeFloatToInt(new Vector3(x, y, z), chunkSize)];
                    closeChunks.Add(thisChunk);
                    visableChunkNumber++;
                }
            }
        }

        //Use list of closeChunks and render stuff
        foreach (Chunk chunk in closeChunks)
        {
            //for every cell in cell dict in chunk, render
            foreach (KeyValuePair<Vector3Int, Cell> cellPair in chunk.cells)
            {
                Cell cell = cellPair.Value;
                var obj = Instantiate(cellPrefab);
                obj.transform.position = cell.position;
                visableCellNumber++;
            }
        }
    }
}