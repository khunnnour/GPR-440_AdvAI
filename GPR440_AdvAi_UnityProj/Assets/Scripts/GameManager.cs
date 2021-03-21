using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    public Text fpsText;
    public GameObject winPanel;
    public Text t1ScoreText,t2ScoreText;
    
    private Camera _mainCam;
    private UnitManager _unitManager;
    private Grid _grid;
    private float _fps;
    private float _timer;
    private bool _gameStarted;
    private int _t1Towers, _t2Towers;
    private int _t1UnitsKilled, _t2UnitsKilled;
    private int _t1TowersKilled, _t2TowersKilled;
    
    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        _mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _unitManager = GameObject.FindGameObjectWithTag("UnitManager").GetComponent<UnitManager>();
        _grid = GameObject.FindGameObjectWithTag("FlowField").GetComponent<Grid>();
        _timer = 0f;
        
        winPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateFPSCounter();
        GetInput();
        if(_gameStarted)
            UpdateGameState();
        UpdateUI();
    }

    private void UpdateGameState()
    {
        // check if either teams have lost their towers
        if (_t1Towers == 0 || _t2Towers == 0)
            EndGame();
    }

    private void EndGame()
    {
        // calculate scores (killed/number had)
        float t1Score = _t2TowersKilled / _t1UnitsKilled * 100f;
        float t2Score = _t1TowersKilled / _t2UnitsKilled * 100f;

        t1ScoreText.text = t1Score.ToString("F1");
        t2ScoreText.text = t2Score.ToString("F1");
        
        winPanel.SetActive(true);
    }

    private void GetInput()
    {
        Vector2 mousePos = Input.mousePosition;
        // if d key then delete the unit under the cursor
        if (Input.GetKeyDown(KeyCode.D))
        {
            _unitManager.DeleteUnit(_mainCam.ScreenToWorldPoint(mousePos));
        }

        // if n key then spawn a unit under the cursor
        //if (Input.GetKeyDown(KeyCode.N))
        //    _unitManager.SpawnNewUnit(_mainCam.ScreenToWorldPoint(mousePos));

        if (Input.GetKeyDown(KeyCode.Space))
            StartGame();


        // if shift then spawn team 2, else team 1
        int t = Input.GetKey(KeyCode.LeftShift) ? 1 : 0;
        
        // if left click then create unit
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = _mainCam.ScreenToWorldPoint(mousePos);
            worldPos.z = 0f;

            _unitManager.SpawnNewUnit(worldPos, t);
        }

        // if right click then create tower
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 worldPos = _mainCam.ScreenToWorldPoint(mousePos);
            worldPos.z = 0f;

            _unitManager.SpawnNewTower(worldPos, t);
            _grid.ReportTowerMade(_unitManager.GetLastTower());
        }
        

        // middle mouse button sets flow target for team 1
        if (Input.GetMouseButtonDown(2))
        {
            Vector3 worldPos = _mainCam.ScreenToWorldPoint(mousePos);
            worldPos.z = 0f;
            
            _grid.SetTarget(worldPos);
        }

        // clear pathing on c
        if (Input.GetKeyDown(KeyCode.C))
            _unitManager.SetPathing(false);
    }

    private void StartGame()
    {
        // set bool to true
        _gameStarted = true;
        // hide win panel
        winPanel.SetActive(false);
        
        // reset counters
        _t1UnitsKilled = 0;
        _t2UnitsKilled = 0;
        _t1TowersKilled = 0;
        _t2TowersKilled = 0;
        
        // count towers
        Tower[] towers=_unitManager.GetTowers();
        foreach (Tower t in towers)
        {
            if (t.Team == 0) _t1Towers++;
            if (t.Team == 1) _t2Towers++;
        }
    }

    // How a tower reports it died 
    public void TowerDied(Vector3 pos)
    {
        _grid.ReportTowerDied(pos);
    }

    public void UnitDied(Vector3 pos)
    {
        _unitManager.DeleteUnit(pos);
    }
    
    private void UpdateFPSCounter()
    {
        // from: https://forum.unity.com/threads/fps-counter.505495/#post-5287614
        if (Time.unscaledTime > _timer)
        {
            _fps = (1f / Time.unscaledDeltaTime);
            _timer = Time.unscaledTime + 0.5f;
        }
    }

    private void UpdateUI()
    {
        fpsText.text = _fps.ToString("F1");
    }
}
