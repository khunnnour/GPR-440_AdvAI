﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text fpsText;

    private Camera _mainCam;
    private UnitManager _unitManager;
    private FlowField _flowField;
    private float _fps;
    private float _timer;
    
    // Start is called before the first frame update
    void Start()
    {
        _mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _unitManager = GameObject.FindGameObjectWithTag("UnitManager").GetComponent<UnitManager>();
        _flowField = GameObject.FindGameObjectWithTag("FlowField").GetComponent<FlowField>();
        _timer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateFPSCounter();
        UpdateUI();
        GetInput();
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
        if (Input.GetKeyDown(KeyCode.N))
            _unitManager.SpawnNewUnit(_mainCam.ScreenToWorldPoint(mousePos));

        // if d key then delete the unit under the cursor
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = _mainCam.ScreenToWorldPoint(mousePos);
            worldPos.z = 0f;
            if (_flowField)
                _flowField.SetTarget(worldPos);
            _unitManager.SetTargetPoint(worldPos);
        }
        
        // clear pathing on c
        if (Input.GetKeyDown(KeyCode.C))
            _unitManager.SetPathing(false);
    }

    private void UpdateFPSCounter()
    {
        // from: https://forum.unity.com/threads/fps-counter.505495/#post-5287614
        if (Time.unscaledTime > _timer)
        {
            _fps = (1f / Time.unscaledDeltaTime);
            _timer = Time.unscaledTime + 1f;
        }
    }
    
    private void UpdateUI()
    {
        fpsText.text = _fps.ToString("F2");
    }
}
