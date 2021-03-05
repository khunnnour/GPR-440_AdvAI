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

    public int batchSize = 25;
    
    private List<Node> _openList ;
    private List<Node> _closedList;
    private List<Node> _processedList;
    private bool _inProgress;
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
        Vector3.left + Vector3.up,
        Vector3.left + Vector3.down,
        Vector3.right + Vector3.up,
        Vector3.right + Vector3.down
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

        _openList = new List<Node>();
        _closedList = new List<Node>();
        _processedList= new List<Node>();
        _inProgress = true;
    }

    private void Update()
    {
        if (_inProgress)
            CalculateFlowField();
    }

    private void CalculateFlowField()
    {
        //Debug.Log("=== Begin Calculating Flow Field Batch ===");

        // process until the open list is empty
        for (int i = 0; i < batchSize; i++)
        {
            // make sure there's still stuff in the open list
            if (_openList.Count <= 0)
                break;
            //Debug.Log("Processing node");
            // get node to process from front of list
            Node curr = _openList[0];

            // move from open to closed
            _openList.RemoveAt(0);
            _closedList.Add(curr);

            // get the connections
            List<Node> neighbors = new List<Node>();
            GetConnections(ref neighbors, WorldPosToMap(curr.Position));

            // check all the connections
            foreach (Node neighbor in neighbors)
            {
                // check if not in any list already
                if (InList(ref _openList, neighbor) == -1 && 
                    InList(ref _closedList, neighbor) == -1 &&
                    InList(ref _processedList, neighbor) == -1)
                {
                    // if not then add it
                    _openList.Add(neighbor);
                    neighbor.Distance = curr.Distance + 1f;
                }
            }
        }

        // remove goal from closed list -- do not need to process that one
        //_closedList.Remove(start);

        // calculate flow direction for all nodes
        while (_closedList.Count>0)
        {
            // get front node
            Node node = _closedList[0];
            
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
                if (neighbors[i].Distance + neighbors[i].Weight < lowestDist)
                {
                    lowestIndex = i;
                    lowestDist = neighbors[i].Distance + neighbors[i].Weight;
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
                
                _mapArrows[index].transform.rotation = Quaternion.Euler(0f, 0f, rot);
            }

            // remove processed node from closed to processed
            _closedList.RemoveAt(0);
            _processedList.Add(node);
        }
        //Debug.Log("=== End Calculating Flow Field ===");
    }

    // Resets values to make a new flow field
    private void ResetFlowField()
    {
        _inProgress = true;
        
        // reset the lists
        _openList.Clear();
        _closedList.Clear();
        _processedList.Clear();

        // get starting node
        Node start = _map[MapToIndex(_target)];
        start.Distance = 0f; // set its distance to 0
        
        // start off open list with target node
        _openList.Add(start);
    }

    /// <summary>
    /// Updates the region at the position provided
    /// </summary>
    /// <param name="wPos">World position that's the origin of the region updated</param>
    public void UpdateFlowRegion(Vector3 wPos)
    {
        // Is basically calculate flow field, but only adds neighbors
        // get the origin node
        List<Node> upOpen = new List<Node> {_map[MapToIndex(WorldPosToMap(wPos))]};
        List<Node> upClosed = new List<Node>();
        
        //update that node
        upOpen[0].UpdateWeight();

        while (upOpen.Count > 0)
        {
            // get node to process from front of list
            Node curr = upOpen[0];

            // move from open to closed
            upOpen.RemoveAt(0);
            upClosed.Add(curr);

            // get the connections
            List<Node> neighbors = new List<Node>();
            GetConnections(ref neighbors, WorldPosToMap(curr.Position));

            // check all the connections
            foreach (Node neighbor in neighbors)
            {
                // first check if in lists already
                if (InList(ref upOpen, neighbor) == -1 &&
                    InList(ref upClosed, neighbor) == -1)
                {
                    // now check if its weight changed
                    // record old weight
                    int oldW = neighbor.Weight;
                    // update weight
                    neighbor.UpdateWeight();

                    //  if this node's weight changed, neighbors may have as well, so add to open
                    if (oldW != neighbor.Weight)
                    {
                        upOpen.Add(neighbor);
                    }
                }
            }
        }
        
        // calculate flow direction for all updated nodes
        Debug.Log("Updating " + upClosed.Count + " nodes");
        while (upClosed.Count>0)
        {
            // get front node
            Node node = upClosed[0];
            
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
                
                _mapArrows[index].transform.rotation = Quaternion.Euler(0f, 0f, rot);
            }

            // remove processed node from closed to processed
            upClosed.RemoveAt(0);
        }
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
            ResetFlowField();
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

    /// <summary>
    /// Get the flow direction of the node at that position 
    /// </summary>
    /// <param name="pos">World position</param>
    /// <returns>Flow direction of the node at that position</returns>
    public Vector3 GetFlowDir(Vector3 pos)
    {
        // convert world to map pos then to index
        // return that node's flow direction
        return _map[MapToIndex(WorldPosToMap(pos))].FlowDir;
    }
}
