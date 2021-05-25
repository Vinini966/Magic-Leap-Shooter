//made by Vincent Kyne
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeSensor : MonoBehaviour
{
    public GameObject player;
    public float resolution = 1;//the distance the nodes are from each other
    public float maxDistance; //max sensor distance
    public float minimumForRound;//minimum spawn points to start spawning targets -- not used
    public float minSpawnDist;//minimum distance from the player to spawn targets
    HashSet<Vector3> nodes = new HashSet<Vector3>(); // list of sesor nodes
    Queue<Vector3> openList;
    public HashSet<Vector3> spawnNodes = new HashSet<Vector3>();// list of spawn nodes
    public float deadZone;// the space above and below the player to not sense
    [Range(0, 1.0f)]
    public float refreshTime;

    //debug
    public bool gizmos;
    public int nodeAmount;
    public int spawnAmount;

    Vector3 intOrigin;

    // Start is called before the first frame update
    void Start()
    {
        //Genrates initial feild
        GenerateFeild();
        
        StartCoroutine(RoomUpdate());
    }

    IEnumerator RoomUpdate()
    {
        Debug.Log("Updating Sensor");
        for(; ; )
        {
            GenerateFeild();
            if (gizmos)
            {
                nodeAmount = nodes.Count;
                spawnAmount = spawnNodes.Count;
            }
            //waits for a minimum of 0.1 sec
            yield return new WaitForSeconds(.1f + refreshTime);
        } 
    }

    public void GenerateFeild()
    {
        //clears list
        nodes.Clear();
        spawnNodes.Clear();
        openList = new Queue<Vector3>();
        intOrigin = floor(player.transform.position);
        //queues initial node based on camera/player position
        openList.Enqueue(intOrigin);
        while (openList.Count > 0)
        {
            CreateNeighbors(openList.Dequeue());
        }

    }

    public void CreateNeighbors(Vector3 startPos)
    {
        int layer = findLayer(intOrigin, startPos);
        Vector3[] r = { Vector3.right, -Vector3.right, Vector3.up, -Vector3.up, Vector3.forward, -Vector3.forward };
        Vector3[] m = { Vector3.right, -Vector3.right, Vector3.forward, -Vector3.forward };
        Vector3[] u;

        //if the layer is close enough do not check the up and down position
        if(Mathf.Abs(intOrigin.y - startPos.y) == layer)
            u = m;
        else
            u = r;

        for(int i = 0; i < u.Length; i++)
        {
            Vector3 neigPos = startPos + u[i] * resolution;
            if (Vector3.Distance(neigPos, intOrigin) > maxDistance || nodes.Contains(neigPos) || spawnNodes.Contains(neigPos))
                continue;
            if (vector3FlatDistance(neigPos, intOrigin) < layer + deadZone &&
               (neigPos.y > intOrigin.y || neigPos.y < intOrigin.y))
                continue;

            //Change to a cylinder not a sphere.
            if(Physics.Linecast(intOrigin, neigPos) && vector3FlatDistance(neigPos, intOrigin) > minSpawnDist)
            {
                spawnNodes.Add(neigPos);
                
            }
            else if (!Physics.Linecast(intOrigin, neigPos))
            {
                nodes.Add(neigPos);
                openList.Enqueue(neigPos);
            }
        }
    }

    Vector3 floor(Vector3 input)
    {
        return new Vector3(Mathf.Floor(input.x),
                           Mathf.Floor(input.y),
                           Mathf.Floor(input.z));
    }

    //finds the xz layer of a node
    int findLayer(Vector3 start, Vector3 end)
    {
        Vector3 result = end - start;
        if (Mathf.Abs(result.x) > 0)
            if (Mathf.Abs(result.x) > Mathf.Abs(result.z))
                return (int)Mathf.Abs(result.x) - 1;
            else
                return (int)Mathf.Abs(result.z) -1;
        else if (Mathf.Abs(result.z) > 0)
            return (int)Mathf.Abs(result.z) - 1;
        else
            return 0;
    }

    //finds the xz distance of two nodes.
    float vector3FlatDistance(Vector3 a, Vector3 b)
    {
        return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }

    private void OnDrawGizmos()
    {
        if (gizmos)
        {
            foreach(Vector3 d in nodes)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(d, resolution / 4);
            }
            foreach(Vector3 s in spawnNodes)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(s, resolution / 4);
            }
        }
    }

}
