using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class PathMarker
{
    public MapLocation location;
    public float G;
    public float H;
    public float F;
    public GameObject marker;
    public PathMarker parent;
    

    public PathMarker (MapLocation I, float g, float h, float f, GameObject marker, PathMarker p)
    {
        location = I;
        G = g;
        H = h;
        F = f;
        this.marker = marker;
        parent = p;
    }

    public override bool Equals(object obj)
    {
        if((parent == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            return location.Equals(((PathMarker)obj).location);
        }
    }

    public override int GetHashCode()
    {
        return 0;
    }

}

public class FindPathAStar : MonoBehaviour
{
    public Maze maze;
    public Material closedMateral;
    public Material openMaterial;

    List<PathMarker> open = new List<PathMarker>();
    List<PathMarker> closed = new List<PathMarker>();

    public GameObject start;
    public GameObject end;
    public GameObject pathP;

    PathMarker goalNode;
    PathMarker startNode;

    PathMarker lastPos;
    bool done = false;

    GameObject Unit;
    bool isMove = false;
    List<Vector3> movePath = new List<Vector3>();

    void RemoveAllMarkers()
    {
        GameObject[] marker = GameObject.FindGameObjectsWithTag("marker");
        foreach (GameObject m in marker)
        {
            Destroy(m);
        }
    }

    void BeginSearch()
    {
        Debug.Log("test");
        done = false;
        RemoveAllMarkers();

        List<MapLocation> locations = new List<MapLocation>();
        //��ΰ˻�
        for (int z = 1; z < maze.depth - 1; z++)
        {
            for (int x = 1; x < maze.width - 1; x++)
            {
                if (maze.map[x, z] != 1)
                {
                    locations.Add(new MapLocation(x, z));
                }
            }
        }

        locations.Shuffle();

        Vector3 startLocation = new Vector3(locations[0].x * maze.scale, 0, locations[0].z * maze.scale);
        startNode = new PathMarker(new MapLocation(locations[0].x, locations[0].z), 0, 0, 0,
            Instantiate(start, startLocation, Quaternion.identity), null);

        Vector3 goalLocation = new Vector3(locations[1].x * maze.scale, 0, locations[1].z * maze.scale);
        goalNode = new PathMarker(new MapLocation(locations[1].x, locations[1].z), 0, 0, 0,
           Instantiate(end, goalLocation, Quaternion.identity), null);

        open.Clear();
        closed.Clear();

        open.Add(startNode);
        lastPos = startNode;

    }

    void Search(PathMarker thisNode)
    {
        if (thisNode.Equals(goalNode))
        {
            done = true;
            return;
        }

        foreach(MapLocation dir in maze.directions)
        {
            MapLocation neighbor = dir + thisNode.location;

            if (maze.map[neighbor.x, neighbor.z] == 1)
            {
                continue;
            }
            if(neighbor.x < 1 || neighbor.x >= maze.width || neighbor.z <1 || neighbor.z >= maze.depth)
            {
                continue;
            }

            if (IsClosed(neighbor))
            {
                continue;
            }

            float G = Vector2.Distance(thisNode.location.ToVector(), neighbor.ToVector()) + thisNode. G;
            float H = Vector2.Distance(neighbor.ToVector(), goalNode.location.ToVector());
            float F = G + H;
            GameObject PathBlock = Instantiate(pathP, new Vector3(neighbor.x * maze.scale, 0, neighbor.z * maze.scale), Quaternion.identity);
            //TextMesh[] values = PathBlock.GetComponentsInChildren<TextMesh>();
            //values[0].text = "G: " + G.ToString("0.00");
            //values[1].text = "H: " + H.ToString("0.00");
            //values[2].text = "F: " + F.ToString("0.00");

            if(!UpdateMarker(neighbor, G, H, F, thisNode))
            {
                open.Add(new PathMarker(neighbor, G, H, F, PathBlock, thisNode));
            }



        }

        open = open.OrderBy(p=>p.F).ToList<PathMarker>();
        PathMarker pm = (PathMarker)open.ElementAt(0);
        closed.Add(pm);

        open.RemoveAt(0);

        pm.marker.GetComponent<Renderer>().material = closedMateral;

        lastPos = pm;

    }


    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)
    {
        foreach (PathMarker p in open)
        {
            if (prt.location.Equals(pos))
            {
                p.G = g;
                p.H = h;
                p.F = f;
                p.parent = prt;
                return true;
            }
        }
        return false;

    }


    bool IsClosed(MapLocation marker)
    {
        foreach(PathMarker p in closed)
        {
            if (p.location.Equals(marker))
            {
                return true;
            }
        }
        return false;
    }
    
    void Start()
    {
        
    }

    void GetPath()
    {
        RemoveAllMarkers();
        PathMarker begin = lastPos;

        while (!startNode.Equals(begin) && begin != null)
        {
            Instantiate(pathP, new Vector3(begin.location.x * maze.scale, 0, begin.location.z * maze.scale), Quaternion.identity);
             begin = begin.parent;
        }
        Instantiate(pathP, new Vector3(startNode.location.x * maze.scale, 0, startNode.location.z * maze.scale), Quaternion.identity);


    }



    void SetMovePath()
    {
        RemoveAllMarkers ();
        PathMarker begin = lastPos;

        while(!startNode.Equals(begin) && begin != null)
        {
            movePath.Add(new Vector3(begin.location.x * maze.scale, 9, begin.location.z * maze.scale));
            begin = begin.parent;
        }
        movePath.Add(new Vector3(startNode.location.x * maze.scale, 0, startNode.location.z * maze.scale));
        movePath.Reverse();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("P");
            BeginSearch();
        }
        if(Input.GetKeyDown(KeyCode.C) && !done)
        {
            Search(lastPos);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            GetPath();
        }
        if(Input.GetKeyDown (KeyCode.R))
        {
            SetMovePath();
            Unit = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Unit.transform.position = movePath[0];
            isMove = true;
        }
        if (isMove)
        {
            if (movePath.Count > 0)
            {
                if (Vector3.Distance(Unit.transform.position, movePath[0]) > 0.1f)
                {
                    Unit.transform.position = Vector3.MoveTowards(Unit.transform.position, movePath[0], 1.0f * maze.scale * Time.deltaTime);
                }
                else
                {
                    Unit.transform.position = movePath[0];
                    movePath.RemoveAt(0);
                }
            }
        }
        else
        {
            isMove = false;
        }

        
    }



}