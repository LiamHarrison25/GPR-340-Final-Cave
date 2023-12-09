using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
    SpawnGrid SpawnGridScript;

    [SerializeField]
    private float noiseScale;

    [SerializeField, Range(0, 1)]
    private float threshold = .5f;

    // Start is called before the first frame update

    private void Awake()
    {
        SpawnGridScript = this.gameObject.GetComponent<SpawnGrid>();
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            //generate new caves
            Debug.Log("updating cells");
            GenerateCaves();
            SpawnGridScript.PositionObjectPool(false);
            SpawnGridScript.UpdateCells();
        }
    }



    public void GenerateCaves()
    {
        int worldChunkRadius = SpawnGridScript.worldChunkRadius;
        int halfChunkSize = SpawnGridScript.halfChunkSize;
        int halfCellSize = SpawnGridScript.halfCellSize;
        int cellSize = halfCellSize * 2;
        int chunkSize = halfChunkSize * 2;

        int x, y, z;
        int middleChunk = chunkSize;
        int min = -(worldChunkRadius * chunkSize) + middleChunk;
        int max = (worldChunkRadius * chunkSize) + middleChunk;

        int dif = (worldChunkRadius * chunkSize);

        for (x = min; x <= max; x += chunkSize)
        {
            for (y = min; y <= max; y += chunkSize)
            {
                for (z = min; z <= max; z += chunkSize)
                {
                    Vector3Int currentChunkKey = GridFunctions.QuantizeFloatToInt(new Vector3(x, y, z), chunkSize);

                    Chunk chunk = SpawnGridScript.chunks[currentChunkKey];

                    Dictionary<Vector3Int, Cell> cells = chunk.cells;

                    float pxShifted = 0;
                    float pyShifted = 0;
                    float pzShifted = 0;

                    foreach (Cell cell in cells.Values)
                    {
                        //set all positive values to be double
                        if (cell.center.x >= 0f)
                        {
                            pxShifted = cell.center.x + dif;
                        }
                        else
                        {
                            pxShifted = Mathf.Abs(cell.center.x);
                        }

                        if (cell.center.y >= 0f)
                        {
                            pyShifted = cell.center.y + dif;
                        }
                        else
                        {
                            pyShifted = Mathf.Abs(cell.center.y);
                        }

                        if (cell.center.z >= 0f)
                        {
                            pzShifted = cell.center.z + dif;
                        }
                        else
                        {
                            pzShifted = Mathf.Abs(cell.center.z);
                        }

                        float noiseValue = Perlin3D(pxShifted * noiseScale, pyShifted * noiseScale, pzShifted * noiseScale);

                        //checks if the noise is above the threshold
                        cell.isCellOn = noiseValue >= threshold;


                        //other noise function:
                        //float noiseValue = Perlin3D(cell.center.x * noiseScale, cell.center.y * noiseScale, cell.center.z * noiseScale);

                    }
                }
            }
        }
    }
    public static float Perlin3D(float x, float y, float z)
    {
        float xy = Mathf.PerlinNoise(x, y);
        float yz = Mathf.PerlinNoise(y, z);
        float xz = Mathf.PerlinNoise(x, z);

        float yx = Mathf.PerlinNoise(y, x);
        float zy = Mathf.PerlinNoise(z, y);
        float zx = Mathf.PerlinNoise(z, x);

        float xyz = xy + yz + xz + yx + zy + zx;
        return xyz / 6f;
    }
}