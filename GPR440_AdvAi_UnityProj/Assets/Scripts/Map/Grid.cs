using System;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [Header("Grid Settings")] [Tooltip("The number of nodes on each side of the map")]
    public Vector3Int dimensions;

    [Tooltip("Space between individual nodes")]
    public float spacing = 2f;

    [Header("Processing Settings")] public int flowNodeBatchSize = 25;
    public int inflTowerBatchSize = 2;

    [Header("Influence Map Settings")] public float maxInf = 10f;
    public Color team1Color, team2Color;

    [Header("Debug Settings")] public bool drawFlowField;

    public enum InfluenceViz
    {
        INFLUENCE,
        TENSION,
        VULNERABILITY
    }

    public bool drawInflMap;
    public InfluenceViz vizualize = InfluenceViz.INFLUENCE;

    private List<Node> _flowOpenList, _flowClosedList, _flowProcessedList;
    private List<Node> _inflOpenList, _inflClosedList;
    private List<Tower> _inflTowerList;
    private bool _playerFlowInProgress, _enemyFlowInProgress, _inflInProgress;
    //private bool _dirty;
    private Vector3 _target;
    private Vector3 _centerOffset, _halfDims;
    private Node[] _map;
    private UnitManager _unitManager;
    private GameObject[] _gridCells, _t2Arrows, _t1Arrows;
    private int _numNodes;
    private int _lastNodeProcessed; //used for ai flow field batching
    private int _lastNodeValidated; //used for flow field validation
    private float _worldToMap;
    private float _invMaxInf;

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
        _centerOffset -= (Vector3.one * 0.5f);
        _centerOffset *= spacing;

        // calculate the half dims
        _halfDims = Vector3.one * spacing * 0.5f;

        // init the map
        _numNodes = dimensions.x * dimensions.y * dimensions.z;
        _map = new Node[_numNodes];
        _gridCells = new GameObject[_numNodes];
        _t1Arrows = new GameObject[_numNodes];
        _t2Arrows = new GameObject[_numNodes];
        // populate the map
        for (int i = 0; i < _numNodes; i++)
        {
            // create the node
            Vector3 mapCoord = IndexToMap(i);
            _map[i] = new Node(this, MapToWorldPos(mapCoord));
            // update its weight
            _map[i].UpdateWeight();

            // instantiate grid cells ---
            // create the grid cells
            _gridCells[i] = Instantiate(Resources.Load<GameObject>("Prefabs/GridCell"),
                MapToWorldPos(mapCoord),
                Quaternion.identity, transform);
            // scale to just under the size of a cell (to make a kind of border)
            float newScale = spacing * 0.95f;
            _gridCells[i].transform.localScale = new Vector3(newScale, newScale, newScale);
            // set start color to white
            _gridCells[i].GetComponent<SpriteRenderer>().color = Color.white;
            // turn off if not drawing
            if (!drawInflMap)
                _gridCells[i].SetActive(false);

            // instantiate arrows ---
            // create both arrows for each cells
            _t1Arrows[i] = Instantiate(Resources.Load<GameObject>("Prefabs/Arrow"),
                MapToWorldPos(mapCoord),
                Quaternion.identity, transform);
            _t2Arrows[i] = Instantiate(Resources.Load<GameObject>("Prefabs/Arrow"),
                MapToWorldPos(mapCoord),
                Quaternion.identity, transform);
            // scale to just under the size of a cell (to make a kind of border)
            newScale = spacing * 0.5f;
            _t1Arrows[i].transform.localScale = new Vector3(newScale, newScale, newScale);
            _t2Arrows[i].transform.localScale = new Vector3(newScale, newScale, newScale);
            // set start color to white
            _t1Arrows[i].GetComponentInChildren<SpriteRenderer>().color = team1Color;
            _t2Arrows[i].GetComponentInChildren<SpriteRenderer>().color = team2Color;
            if (!drawFlowField)
            {
                _t1Arrows[i].SetActive(false);
                _t2Arrows[i].SetActive(false);
            }
        }

        _invMaxInf = 1f / maxInf;

        _enemyFlowInProgress = false;
        _lastNodeProcessed = -1;

        _flowOpenList = new List<Node>();
        _flowClosedList = new List<Node>();
        _flowProcessedList = new List<Node>();
        _playerFlowInProgress = false;

        _inflOpenList = new List<Node>();
        _inflClosedList = new List<Node>();
        _inflTowerList = new List<Tower>();
        _inflInProgress = false;
        
        _unitManager = GameObject.FindGameObjectWithTag("UnitManager").GetComponent<UnitManager>();
    }

    private void Update()
    {
        if (_inflInProgress)
            CalculateInfluenceMap();

        if (_playerFlowInProgress)
            CalculateFlowField();

        if (_enemyFlowInProgress)
            CalculateAIFlowField();

        
        ValidateMap();
    }

    private void OnValidate()
    {
        // hopefully only draw settings are changed in runtime
        // go all nodes and turn on/off the relevant ones
        for (int i = 0; i < _numNodes; i++)
        {
            _gridCells[i].SetActive(drawInflMap);
            if (drawInflMap)
            {
                // if drawing the influence map, set the color of the cell based on map viz
                _gridCells[i].GetComponent<SpriteRenderer>().color = CalcCellColor(_map[i]);
            }
            
            _t1Arrows[i].SetActive(drawFlowField);
            _t2Arrows[i].SetActive(drawFlowField);
        }
    }

    private void CalculateFlowField()
    {
        // process until the open list is empty
        for (int i = 0; i < flowNodeBatchSize; i++)
        {
            // make sure there's still stuff in the open list
            if (_flowOpenList.Count <= 0)
                break;
            //Debug.Log("Processing node");
            // get node to process from front of list
            Node curr = _flowOpenList[0];

            // move from open to closed
            _flowOpenList.RemoveAt(0);
            _flowClosedList.Add(curr);

            // get the connections
            List<Node> neighbors = new List<Node>();
            GetConnections(ref neighbors, WorldPosToMap(curr.Position));

            // check all the connections
            foreach (Node neighbor in neighbors)
            {
                // check if not in any list already
                if (InList(ref _flowOpenList, neighbor) == -1 &&
                    InList(ref _flowClosedList, neighbor) == -1 &&
                    InList(ref _flowProcessedList, neighbor) == -1)
                {
                    // if not then add it
                    _flowOpenList.Add(neighbor);
                    neighbor.Distance = curr.Distance + 1f;
                }
            }
        }

        // remove goal from closed list -- do not need to process that one
        //_flowClosedList.Remove(start);

        // calculate flow direction for all nodes
        while (_flowClosedList.Count > 0)
        {
            // get front node
            Node node = _flowClosedList[0];

            // if the target node set direction to 0 and continue
            if (node.Distance == 0f)
            {
                node.Team1FlowDir = Vector3.zero;
                int index = MapToIndex(WorldPosToMap(node.Position));
                _t1Arrows[index].transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                _flowClosedList.RemoveAt(0);
                _flowProcessedList.Add(node);
                continue;
            }
            
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
                    // minus infl because if enemy owns square it increases cost
                    lowestDist = neighbors[i].Distance + neighbors[i].Weight - neighbors[i].NetInfluence * _invMaxInf;
                }
            }

            // make current node's direction face the closest one
            node.Team1FlowDir = (neighbors[lowestIndex].Position - node.Position).normalized;
            // update the arrow if necessary
            if (drawFlowField)
            {
                int index = MapToIndex(WorldPosToMap(node.Position));

                Vector3 dir = _map[index].Team1FlowDir;
                float rot = Mathf.Atan2(-dir.x, dir.y) * Mathf.Rad2Deg;
                _t1Arrows[index].transform.rotation = Quaternion.Euler(0f, 0f, rot);

                //dir = _map[index].Team2FlowDir;
                //rot = Mathf.Atan2(-dir.x, dir.y) * Mathf.Rad2Deg;
                //_t2Arrows[index].transform.rotation = Quaternion.Euler(0f, 0f, rot);
            }

            // remove processed node from closed to processed
            _flowClosedList.RemoveAt(0);
            _flowProcessedList.Add(node);
        }
    }


    // Calculates flow field for enemy AI (team 2) by moving to high tension areas 
    private void CalculateAIFlowField()
    {
        // process until the open list is empty
        for (int i = 1; i <= flowNodeBatchSize; i++)
        {
            int currIndex = _lastNodeProcessed + i;
            // return out and reset if gone thru the whole map
            if (currIndex >= _map.Length)
            {
                _enemyFlowInProgress = false;
                _lastNodeProcessed = -1;
                return;
            }

            Node curr = _map[currIndex];

            // get all neighbors
            List<Node> neighbors = new List<Node>();
            GetConnections(ref neighbors, WorldPosToMap(curr.Position));

            // want low weight and high player influence
            float highestTen = neighbors[0].TotInfluence - Mathf.Abs(neighbors[0].NetInfluence);
            int wantIndex = 0;

            // cycle thru neighbors to find the one with highest enemy influence/lowest weight
            for (int j = 1; j < neighbors.Count; j++)
            {
                float nTen = neighbors[j].TotInfluence - Mathf.Abs(neighbors[j].NetInfluence);
                // if the neighbor's cost is less than the lowest then log it
                if (nTen > highestTen)
                {
                    wantIndex = j;
                    highestTen = nTen;
                }
            }

            // if high ten ==0, then go left, otherwise go to highest ten neighbor
            curr.Team2FlowDir = highestTen == 0 ? Vector3.left : (neighbors[wantIndex].Position - curr.Position).normalized;

            // update the arrow if necessary
            if (drawFlowField)
            {
                int index = MapToIndex(WorldPosToMap(curr.Position));

                Vector3 dir = _map[index].Team2FlowDir;
                float rot = Mathf.Atan2(-dir.x, dir.y) * Mathf.Rad2Deg;
                _t2Arrows[index].transform.rotation = Quaternion.Euler(0f, 0f, rot);
            }
        }

        _lastNodeProcessed += flowNodeBatchSize;
    }

    // Resets values to make a new flow field
    private void ResetFlowField()
    {
        _playerFlowInProgress = true;
        //_dirty = true;

        // reset the lists
        _flowOpenList.Clear();
        _flowClosedList.Clear();
        _flowProcessedList.Clear();

        // get starting node
        Node start = _map[MapToIndex(_target)];
        start.Distance = 0f; // set its distance to 0

        // start off open list with target node
        _flowOpenList.Add(start);
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
        while (upClosed.Count > 0)
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
            node.Team2FlowDir = (neighbors[lowestIndex].Position - node.Position).normalized;
            // update the arrow if necessary
            if (drawFlowField)
            {
                int index = MapToIndex(WorldPosToMap(node.Position));

                Vector3 dir = _map[index].Team1FlowDir;
                float rot = Mathf.Atan2(-dir.x, dir.y) * Mathf.Rad2Deg;
                _t1Arrows[index].transform.rotation = Quaternion.Euler(0f, 0f, rot);

                dir = _map[index].Team2FlowDir;
                rot = Mathf.Atan2(-dir.x, dir.y) * Mathf.Rad2Deg;
                _t2Arrows[index].transform.rotation = Quaternion.Euler(0f, 0f, rot);
            }

            // remove processed node from closed to processed
            upClosed.RemoveAt(0);
        }
    }

    // Calculates whole influence map -- should be used sparingly
    private void CalculateInfluenceMap()
    {
        //Debug.Log("CalculateInfluenceMap called");
        int towersProcessed = 1;
        // Dijkstra's out starting from every 'seed' node (one with a tower on it)
        //for (int i = 0; i < inflTowerBatchSize; i++)
        while (_inflTowerList.Count > 0)
        {
            // break if processes enough
            if (towersProcessed > inflTowerBatchSize)
                break;

            //Debug.Log("Doing tower " + towersProcessed);

            // get current tower
            Tower currTower = _inflTowerList[0];
            // and remove it from the list to be processed
            _inflTowerList.RemoveAt(0);

            // get the map coord of the seed
            Vector3 originWorldPos = currTower.transform.position;

            // process the node under the tower
            int seedIndex = MapToIndex(WorldPosToMap(originWorldPos));
            Node seed = _map[seedIndex];
            seed.AddInfluence(currTower.Influence * (currTower.Team == 0 ? 1 : -1));
            Color seedCol =  CalcCellColor(seed);
            _gridCells[seedIndex].GetComponent<SpriteRenderer>().color = seedCol;

            // add the node under the tower to the open list
            _inflOpenList.Add(seed);

            // go until no more neighbors
            while (_inflOpenList.Count > 0)
            {
                // get node to process from front of list
                Node curr = _inflOpenList[0];

                // move from open to closed
                _inflOpenList.RemoveAt(0);
                _inflClosedList.Add(curr);

                // get the connections
                List<Node> neighbors = new List<Node>();
                GetConnections(ref neighbors, WorldPosToMap(curr.Position));

                // check all the connections
                foreach (Node neighbor in neighbors)
                {
                    // first check if in lists already
                    if (InList(ref _inflOpenList, neighbor) == -1 &&
                        InList(ref _inflClosedList, neighbor) == -1)
                    {
                        // get world position of neighbor
                        Vector3 nWorldPos = neighbor.Position;

                        // y = -|mx+b|+k
                        // calculate influence: I = -BD^2/Dm^2+B
                        // B = base infl, D = dist, Dm = max dist
                        float inf = -(currTower.Influence * (originWorldPos - nWorldPos).sqrMagnitude) /
                            (currTower.maxDist * currTower.maxDist) + currTower.Influence;
                        // only add to open if influence > 0
                        if (inf > 0)
                        {
                            // add to the open list
                            _inflOpenList.Add(neighbor);

                            // check team of tower
                            if (currTower.Team == 1)
                                inf *= -1;
                            neighbor.AddInfluence(inf);

                            // update the node's color
                            int nIndex = MapToIndex(WorldPosToMap(neighbor.Position));
                            Color col = CalcCellColor(neighbor);

                            _gridCells[nIndex].GetComponent<SpriteRenderer>().color = col;
                        }
                    }
                }
            }

            // increment number of towers processed
            towersProcessed++;
        }

        // check if here bc no more towers
        if (_inflTowerList.Count == 0)
        {
            _inflInProgress = false;
            //Debug.Log("Finished");
        }
    }

    // Resets values to make a new flow field
    private void StartInfluenceMapCalc()
    {
        //Debug.Log("== Influence mapping started ==");
        _inflInProgress = true;
        _enemyFlowInProgress = true;

        // reset the lists
        _inflOpenList.Clear();
        _inflClosedList.Clear();
        //_inflTowerList.Clear();
        // get the towers from the unit manager
        //_inflTowerList = _unitManager.GetTowers().ToList();
    }

    void ValidateMap()
    {
        int batchSize = Mathf.CeilToInt(flowNodeBatchSize * 0.4f);

        Tower[] allTowers = new Tower[] { };

        for (int i = 0; i < batchSize; i++)
        {
            int currIndex = _lastNodeValidated + i;
            // break if finished map
            if (currIndex >= _numNodes)
            {
                _lastNodeValidated = 0;
                return;
            }

            //Debug.Log("Processed node #" + currIndex);

            // get current node
            Node currNode = _map[currIndex];
            // get node world pos
            Vector3 originWorldPos = currNode.Position;

            // compare to every tower to verify node's influence
            float correctInfl = 0f;
            float totInf = 0f;
            foreach (Tower tower in allTowers)
            {
                float distSqr = (tower.transform.position - originWorldPos).sqrMagnitude;
                if (distSqr <= tower.maxDist * tower.maxDist)
                {
                    float infl = -(tower.Influence * distSqr) / (tower.maxDist * tower.maxDist) + tower.Influence;
                    
                    totInf += infl;

                    if (tower.Team == 1)
                        infl *= -1;
                    correctInfl += infl;
                }
            }

            // check if calculated influence matches actual
            if (correctInfl != currNode.NetInfluence)
            {
                currNode.SetInfluence(correctInfl, totInf);
                Color seedCol = CalcCellColor(currNode);
                _gridCells[currIndex].GetComponent<SpriteRenderer>().color = seedCol;
            }
        }

        _lastNodeValidated += batchSize;
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
        return (int) (coord.x + coord.z * dimensions.x + coord.y * dimensions.x * dimensions.z);
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

    // calculate cell color based on visualization settings
    Color CalcCellColor(Node cell)
    {
        switch (vizualize)
        {
            case InfluenceViz.INFLUENCE:
                return Color.Lerp(Color.white, cell.NetInfluence > 0 ? team1Color : team2Color,
                    Mathf.Abs(cell.NetInfluence) * _invMaxInf);
            case InfluenceViz.TENSION:
                return Color.Lerp(Color.white, Color.black, cell.TotInfluence * _invMaxInf);
            case InfluenceViz.VULNERABILITY:
                return Color.Lerp(Color.white, Color.red, (cell.TotInfluence-Mathf.Abs(cell.NetInfluence)) * _invMaxInf);
        }
        // returns magenta if something goes wrong
        return Color.magenta;
    }

    /// <summary>
    /// Get the flow direction of the node at that position 
    /// </summary>
    /// <param name="pos">World position</param>
    /// <param name="team">Team direction wanted</param>
    /// <returns>Flow direction of the node at that position</returns>
    public Vector3 GetFlowDir(Vector3 pos, int team)
    {
        // convert world to map pos then to index
        // return that node's flow direction
        return team == 0 ? _map[MapToIndex(WorldPosToMap(pos))].Team1FlowDir : _map[MapToIndex(WorldPosToMap(pos))].Team2FlowDir;
    }

    // spiral out adding appropriate influence
    public void ReportTowerMade(Tower t)
    {
        // add tower to list to be processed
        _inflTowerList.Add(t);
        // if not already being calculated then start
        if (!_inflInProgress)
            StartInfluenceMapCalc();
    }

    // spiral out removing appropriate influence
    public void ReportTowerDied(Vector3 pos)
    {
        _lastNodeProcessed = 0;
        //_dirty = true;
    }
}
