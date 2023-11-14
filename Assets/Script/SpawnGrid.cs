using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnGrid : MonoBehaviour
{
    [SerializeField] private int worldSize = 10;
    [SerializeField] private float halfWidth = 0.5f;
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private Material[] materialList;

    struct Bucket{
        bool isVisited;
        Vector3 pos;
        List<GameObject> objects;
    }

    //convert vector3f to vector3int

    Vector3Int QuantizeFloatToInt(Vector3 position, float cellSize) {
        return new Vector3Int(
            (int)Mathf.Floor(position.x/cellSize),
            (int)Mathf.Floor(position.y/cellSize),
            (int)Mathf.Floor(position.z/cellSize));
    }
    
    //change the value to whatever you want to hold
    private Dictionary<Vector3Int, Bucket> dataGrid; 
    
    void Start()
    {
        //int numAxis = 0;
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                for (int z = 0; z < worldSize; z++)
                {
                    //numAxis = 0;
                    dataGrid.Add(QuantizeFloatToInt(new Vector3(x, y, z), 1), new Bucket()); 

                    //if (z % 10 == 0)
                    //{
                    //    numAxis++;
                    //}

                    //if (x % 10 == 0)
                    //{
                    //    numAxis++;
                    //}

                    //if (y % 10 == 0)
                    //{
                    //    numAxis++;
                    //}

                    //if (numAxis == 3)
                    //{
                    //var obj = Instantiate(cubePrefab);
                    //obj.transform.position = new Vector3(x, y, z);

                    //}
                }
            }
        }
    }

    public void OnDrawGismos()
    {

    }
}




