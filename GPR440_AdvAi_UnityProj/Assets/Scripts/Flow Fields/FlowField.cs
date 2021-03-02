using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FlowField : MonoBehaviour
{
    public bool showArrows;
    
    [Tooltip("The number of nodes on each side of the map")]
    public Vector3Int dimensions;

    [Tooltip("Space between individual nodes")]
    public float spacing = 2f;

    private Vector3 _target;
    private Vector3 _centerOffset,_halfDims;
    private Node[] _map;
    private GameObject[] _mapArrows;
    private int _numNodes;
    private float _worldToMap;
    private readonly Vector3[] _offsets =
    {
        Vector3.left, 
        Vector3.right, 
        Vector3.up, 
        Vector3.down,
        Vector3.left+Vector3.up,
        Vector3.left+Vector3.down,
        Vector3.right+Vector3.up,
        Vector3.right+Vector3.down
    };

    public Vector3 HalfDims => _halfDims;
    
    // Start is called before the first frame update
    void Start()
    {
        // calculate inverse of spacing to convert world coord to map coord
        _worldToMap = 1f / spacing;

        // calculate offset to center the map
        _centerOffset = (Vector3) (dimensions) * 0.5f;
        _centerOffset -= (Vector3.one*0.5f);
        _centerOffset *= spacing;
        //Debug.Log("Center Offset: " + _centerOffset);
        
        // calculate the half dims
        _halfDims = Vector3.one * spacing * 0.5f;
        //Debug.Log(_halfDims.ToString("F1"));

        // init the map
        _numNodes = dimensions.x * dimensions.y * dimensions.z;
        _map = new Node[_numNodes];
        _mapArrows = new GameObject[_numNodes];
        // populate the map
        for (int i = 0; i < _numNodes; i++)
        {
            // create the node
            Vector3 mapCoord = IndexToMap(i);
            _map[i] = new Node(this, MapToWorldPos(mapCoord));
            // update its weight
            _map[i].UpdateWeight();

            // rotate the arrow and color it
            _mapArrows[i] = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/Arrow"), MapToWorldPos(mapCoord),
                Quaternion.identity, transform);
            _mapArrows[i].transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            _mapArrows[i].GetComponent<SpriteRenderer>().color =
                new Color(_map[i].FlowDir.x, _map[i].FlowDir.y, _map[i].FlowDir.z);
        }
    }

    private void CalculateFlowField()
    {
        Debug.Log("=== Begin Calculating Flow Field ===");
        // create the open and closed lists
        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();

        // get starting node
        Node start = _map[MapToIndex(_target)];
        start.Distance = 0f; // set its distance to 0
        
        // start off open list with target node
        openList.Add(start);

        // process until the open list is empty
        while (openList.Count > 0)
        {
            Debug.Log("Processing node");
            // get node to process from front of list
            Node curr = openList[0];

            // move from open to closed
            openList.RemoveAt(0);
            closedList.Add(curr);

            // get the connections
            List<Node> neighbors = new List<Node>();
            GetConnections(ref neighbors, WorldPosToMap(curr.Position));

            // check all the connections
            foreach (Node neighbor in neighbors)
            {
                /*
                // calculate cost to this node (add cost so far to weight to next one)
                float cost = curr.Distance + neighbor.Weight+1f;

                // check if this neighbor path has a lower cost
                if (cost < neighbor.Distance)
                {
                    // check if in open already
                    if (InList(ref openList, neighbor) == -1)
                    {
                        // if not then add it
                        openList.Add(neighbor);
                    }

                    // update neighbor with lower distance
                    neighbor.Distance = cost;
                   // Debug.Log("Node " + MapToIndex(neighbor.Position) + " dist = " + neighbor.Distance);
                }
                */
                // check if not in open or closed already
                if (InList(ref openList, neighbor) == -1 && InList(ref closedList, neighbor) == -1)
                {
                    // if not then add it
                    openList.Add(neighbor);
                    neighbor.Distance = curr.Distance + 1f + neighbor.Weight;
                }
            }
        }

        // remove goal from closed list -- do not need to process that one
        closedList.Remove(start);

        // calculate flow direction for all nodes
        foreach (Node node in closedList)
        {
            // get neighbors
            List<Node> neighbors = new List<Node>();
            GetConnections(ref neighbors, WorldPosToMap(node.Position));

            // index of the neighbor in list
            int lowestIndex = 0;
            // distance of that neighbor
            float lowestDist = neighbors[0].Distance;
            // cycle thru the rest of the neighbors to find the 'closest'
            for (int i = 1; i < neighbors.Count; i++)
            {
                // if the neighbor's distance is less than the lowest then log it
                if (neighbors[i].Distance < lowestDist)
                {
                    lowestIndex = i;
                    lowestDist = neighbors[i].Distance;
                }
            }

            // make current node's direction face the closest one
            node.FlowDir = (neighbors[lowestIndex].Position - node.Position).normalized;
            // update the arrow if necessary
            if (showArrows)
            {
                int index = MapToIndex(WorldPosToMap(node.Position));
                Vector3 dir = _map[index].FlowDir;
                float rot = Mathf.Atan2(-dir.x, dir.y)*Mathf.Rad2Deg;
                //Debug.Log("Node " + index + " points " + dir + "(" + rot + ")");
                _mapArrows[index].transform.rotation = Quaternion.Euler(0f, 0f, rot);
                //_mapArrows[index].GetComponent<SpriteRenderer>().color =
                //    new Color(dir.x, dir.y, dir.z);
                float col = node.Distance / dimensions.x;
                _mapArrows[index].GetComponent<SpriteRenderer>().color =
                    new Color(col, col, (float)node.Weight / 2f);
            }
        }
        Debug.Log("=== End Calculating Flow Field ===");
    }

    /// <summary>
    /// Converts a world position into a map coordinate
    /// </summary>
    /// <param name="pos">world position</param>
    /// <returns>map coordinate</returns>
    Vector3 WorldPosToMap(Vector3 pos)
    {
        Vector3 r = (pos + _centerOffset) * _worldToMap;
        r.x = Mathf.Round(r.x);
        r.y = Mathf.Round(r.y);
        r.z = Mathf.Round(r.z);
        return r;
    }
    
    Vector3 MapToWorldPos(Vector3 map)
    {
        return map * spacing - _centerOffset;
    }
    
    public void SetTarget(Vector3 t)
    {
        // convert to map coord
        /*
        // un-offset it
        t += _centerOffset;
        // account for spacing
        t *= _worldToMap;

        // floor values to get map coord
        t.x = Mathf.Floor(t.x);
        t.y = Mathf.Floor(t.y);
        t.z = Mathf.Floor(t.z);
        */
        Vector3 mapT = WorldPosToMap(t);
        
        // check if new map coord; if a new target map coord then calculate a new flow field
        if (_target != mapT)
        {
            _target = mapT;
            CalculateFlowField();
        }
    }

    // helpers --
    void GetConnections(ref List<Node> conns, Vector3 coord)
    {
        // cycle thru every offset
        foreach (Vector3 off in _offsets)
        {
            // find the offset coord
            Vector3 testCoord = coord + off;

            // ensure it is in the boundaries
            if (testCoord.x >= 0 && testCoord.x < dimensions.x &&
                testCoord.y >= 0 && testCoord.y < dimensions.y &&
                testCoord.z >= 0 && testCoord.z < dimensions.z)
            {
                conns.Add(_map[MapToIndex(testCoord)]);
            }
        }
    }

    /// <summary>
    /// Returns the index of an item in a list
    /// </summary>
    /// <param name="list">List to search</param>
    /// <param name="search">Item to search for</param>
    /// <returns>Index of search item (returns -1 if not in list)</returns>
    private int InList(ref List<Node> list, Node search)
    {
        int index = -1;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == search)
            {
                index = i;
                break;
            }
        }

        return index;
    }
    
    /// <summary>
    /// Converts a map coord to an index for the map array
    /// </summary>
    /// <param name="coord">Map coord</param>
    /// <returns>Returns int index</returns>
    int MapToIndex(Vector3 coord)
    {
        return (int)(coord.x + coord.z * dimensions.x + coord.y * dimensions.x * dimensions.z);
    }

    /// <summary>
    /// Converts an index into a map coord
    /// </summary>
    /// <param name="index">index in array</param>
    /// <returns>Returns vec3 coord</returns>
    Vector3 IndexToMap(int index)
    {
        Vector3 coord = new Vector3();

        coord.y = Mathf.FloorToInt(index / (dimensions.x * dimensions.z));
        index = index % (dimensions.x * dimensions.z);
        coord.z = Mathf.FloorToInt(index / dimensions.x);
        coord.x = index % dimensions.x;

        return coord;
    }
}
