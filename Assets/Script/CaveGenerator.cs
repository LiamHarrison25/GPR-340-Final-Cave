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
        //Debug.Log(Perlin3D(4f * noiseScale, 4f * noiseScale, 4f * noiseScale));
        //Debug.Log(Perlin3D(7f * noiseScale, 7f * noiseScale, 7f * noiseScale));
        //Debug.Log(Perlin3D(5f * noiseScale, 3f * noiseScale, 8f * noiseScale));
        //int x, y, z;
        //int min = -50;
        //int max = 50;
        //for (x = min; x <= max; x += 1)
        //{
        //    for (y = min; y <= max; y += 1)
        //    {
        //        for (z = min; z <= max; z += 1)
        //        {
        //            float noiseValue = Perlin3D(Mathf.Abs(x * noiseScale), Mathf.Abs(y * noiseScale), Mathf.Abs(z * noiseScale));
        //            Debug.Log("noise value: " + noiseValue);
        //        }
        //    }
        //}
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

        for (x = min; x <= max; x += chunkSize)
        {
            for (y = min; y <= max; y += chunkSize)
            {
                for (z = min; z <= max; z += chunkSize)
                {
                    Vector3Int currentChunkKey = GridFunctions.QuantizeFloatToInt(new Vector3(x, y, z), chunkSize);

                    Chunk chunk = SpawnGridScript.chunks[currentChunkKey];

                    Dictionary<Vector3Int, Cell> cells = chunk.cells;

                    float px, py, pz;
                    foreach (Cell cell in cells.Values)
                    {
                        px = (cell.center.x - min) * noiseScale;
                        py = (cell.center.y - min) * noiseScale;
                        pz = (cell.center.z - min) * noiseScale;

                        float noiseValue = Perlin3D(px, py, pz);
                        //Debug.Log("noise value: " + noiseValue);

                        if (noiseValue >= threshold) //checks if the noise is above the threshold
                        {
                            cell.isCellOn = true;
                            //Debug.Log("cell");
                        }
                        else
                        {
                            cell.isCellOn = false;
                            //Debug.Log("no cell");
                        }
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