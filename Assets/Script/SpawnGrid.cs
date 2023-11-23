using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    int cellNumber = 0;
    int chunkNumber = 0;

    [SerializeField] private int chunkWorldRadius = 10;
    private int totalWorldSizeInChunks;
    private int chunkSize = 10;
    private float cellSize = 1;
    private int chunkRenderDistance = 2;

    [SerializeField] private GameObject cellPrefab;


    private Dictionary<Vector3Int, Chunk> chunks;
    //change the value to whatever you want to hold

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

        //set world Size
        totalWorldSizeInChunks = chunkWorldRadius * 2;

        //create grid
        CreateGrid();

        RenderGrid();
    }

    private void Update()
    {
        //RenderGrid();
        Debug.Log("Cells: " + cellNumber);
        Debug.Log("Chunks: " + chunkNumber);
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
                    Chunk newChunk = new Chunk();
                    newChunk.position = QuantizeFloatToInt(new Vector3(x * chunkSize, y * chunkSize, z * chunkSize), chunkSize);

                    // Initialize the cells dictionary for the new chunk
                    newChunk.cells = new Dictionary<Vector3Int, Cell>();

                    chunks.Add(newChunk.position, newChunk);

                    //cells //////////////////////////////
                    //Get cell dict from chunk
                    Dictionary<Vector3Int, Cell> cellList = chunks[new Vector3Int(x, y, z)].cells;

                    //create all cells within this chunk
                    List<Cell> createdCells = CreateCells(new Vector3Int(x, y, z), new Vector3Int(x * chunkSize, y * chunkSize, z * chunkSize));

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
        for (int x = min.x; x < max.x; x++)
        {
            for (int y = min.y; y < max.y; y++)
            {
                for (int z = min.z; z < max.z; z++)
                {
                    Cell newCell = new Cell();

                    //set cell position
                    newCell.position = QuantizeFloatToInt(new Vector3(x, y, z), cellSize);

                    cellList.Add(newCell);
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
                    chunkNumber++;
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
                cellNumber++;
            }
        }
    }

    public void OnDrawGismos()
    {

    }
}