using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	// public variables
	public static GameManager instance;

	public Text fpsText;

	// public properties
	public GameData GameDataObj => _gameDataObj;

	// private variables
	[SerializeField]
	private GameData _gameDataObj;
	private Camera _mainCam;
	private UnitManager _unitManager;
	private Grid _grid;
	private float _fps;
	private float _timer;

	private void Awake()
	{
		_gameDataObj.LoadData();//update data
		instance = this;
		Application.targetFrameRate = -1;
	}

	// Start is called before the first frame update
	void Start()
	{
		_mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		_unitManager = GameObject.FindGameObjectWithTag("UnitManager").GetComponent<UnitManager>();
		_grid = GameObject.FindGameObjectWithTag("FlowField").GetComponent<Grid>();
		_timer = 0f;
	}

	// Update is called once per frame
	void Update()
	{
		UpdateFPSCounter();
        GetInput();
	}

	private void GetInput()
	{
		// if d key then delete the unit under the cursor
		if (Input.GetKeyDown(KeyCode.D))
		{
			_unitManager.DeleteUnit(_mainCam.ScreenToWorldPoint(Input.mousePosition));
		}
	}

	public void UnitDied(Vector3 pos)
	{
		_unitManager.DeleteUnit(pos);
	}

	private void UpdateFPSCounter()
	{
		// from: https://forum.unity.com/threads/fps-counter.505495/#post-5287614
		if (!(Time.unscaledTime >= _timer)) return;
		_fps = (1f / Time.unscaledDeltaTime);
		_timer = Time.unscaledTime + 0.333f;
		fpsText.text = _fps.ToString("F1");
	}
}
