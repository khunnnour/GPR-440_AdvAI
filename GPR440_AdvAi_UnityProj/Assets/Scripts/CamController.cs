using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CamController : MonoBehaviour
{
    [Range(0.1f, 1.0f)] public float moveSpeed = 0.8f;

    public float zoomSpeed = 1f;
    public Vector2 zoomBounds = new Vector2(15f, 45f);

    private Camera _cam;
    private Vector3 _prevMousePos;
    private bool _dragging;

    private void Start()
    {
        _dragging = false;
        _cam = GetComponent<Camera>();
    }

    private void Update()
    {
        GetInput();
        if (_dragging)
            Drag();
        Zoom();
    }

    private void GetInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _dragging = true;
            _prevMousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
            _dragging = false;
    }

    void Drag()
    {
        Vector3 mouseWorldPos = _cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 diff = _prevMousePos - mouseWorldPos;

        transform.position += diff * moveSpeed;

        _prevMousePos = mouseWorldPos;
    }

    void Zoom()
    {
        // calculate a new size for camera
        float newSize = _cam.orthographicSize - Input.mouseScrollDelta.y * zoomSpeed;

        // clamp and set
        _cam.orthographicSize = Mathf.Max(zoomBounds.x, Mathf.Min(newSize, zoomBounds.y));
    }
}
