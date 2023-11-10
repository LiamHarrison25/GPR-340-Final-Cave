using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnGrid : MonoBehaviour
{
    [SerializeField] private int worldSize = 10;
    [SerializeField] private float halfWidth = 0.5f;
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private Material[] materialList;
    
    void Start()
    {
        int x , y, z;
        int index = 0;
        int numAxis = 0;
        List<Vector3> grid = new List<Vector3>();
        
        for (x = 0; x < worldSize; x++)
        {
            for (y = 0; y < worldSize; y++)
            {
                for (z = 0; z < worldSize; z++)
                {
                    numAxis = 0;
                    grid.Add(new Vector3(x, y, z));
                    
                    if (z % 10 == 0)
                    {
                        numAxis++;
                    }
                    
                    if (x % 10 == 0)
                    {
                        numAxis++;
                    }
                    
                    if (y % 10 == 0)
                    {
                        numAxis++;
                    }
                    
                    if (numAxis == 3)
                    {
                        var obj = Instantiate(cubePrefab);
                        obj.transform.position = new Vector3(x, y, z);
                    }
                }
            }
        }
    }
}
