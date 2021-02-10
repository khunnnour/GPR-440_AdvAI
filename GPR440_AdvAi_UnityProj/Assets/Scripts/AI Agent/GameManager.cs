using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text fpsText;

    private UnitManager _unitManager;
    private float _fps;
    private float _timer;
    
    // Start is called before the first frame update
    void Start()
    {
        _unitManager = GameObject.FindGameObjectWithTag("UnitManager").GetComponent<UnitManager>();
        _timer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateFPSCounter();
        UpdateUI();
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
