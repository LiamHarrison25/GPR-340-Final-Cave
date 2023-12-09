using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator2 : MonoBehaviour
{
    SpawnGrid SpawnGridScript;

    private void Awake()
    {
        SpawnGridScript = this.gameObject.GetComponent<SpawnGrid>();
    }

    public void GenerateCave()
    {
        //Queue to create back tracking
        //While, if items are still in stack & nothing left to visit (no neighbours)
        //list all neighbours of current cell
        
        //if visitable, return first neighbour,
        //if two visitable, call random (choose 1)
        //when moving, add to stack

        //if no neighbours to viist, pop from queue
        //if neighbours on popped neighbour, go to other neighbours



    }
}
