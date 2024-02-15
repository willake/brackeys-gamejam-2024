using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.UIElements;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;
using static UnityEngine.Rendering.HableCurve;
using static UnityEngine.UI.Image;

public class EchoLocator : MonoBehaviour
{

    // private values should with a prefix _
    private Vector2 _direction = Vector2.up;

    [SerializeField]
    private LineRenderer _lineRenderer;
    [SerializeField]
    private GameObject _trailPrefab;
    [SerializeField]
    private LineRenderer[] _trailRenderer;
    [SerializeField]
    private float _angle = -90;
    [SerializeField]
    public Transform _door;
    [SerializeField]
    private int _maxBounce;
    [SerializeField]
    private int _shotNumber;
    [SerializeField]
    private Material lineMat;
    [SerializeField]
    private GameObject _pointLightPrefab;
    [SerializeField]
    private GameObject _pointLightParent;

    private float _positionParam;

    private Transform[] _lightPoints;
    private Vector2[] _bouncePoints;
    private float[] _distances;
    private float _pathLength;
    private int _currentShot;
    private bool _shot;
    private bool _ended;

    [SerializeField]
    private bool _isInitiated;

    public Vector2 Direction { get => _direction; }

    public Vector2 rayOrigin()
    {
        Vector2 rayOrigin = _door.position;
        rayOrigin.y += _door.localScale.y / 2;
        return _door.rotation * rayOrigin;
    }

    public void updateAngle(Vector2 dest)
    {
        float signedAngle = Vector3.SignedAngle(Vector2.up, dest - rayOrigin(), Vector3.forward);
        if (signedAngle > -90 & signedAngle < 90)
            _angle = signedAngle;
    }

    public void updateDir()
    {
        _angle = Mathf.Min(90, Mathf.Max(-90, _angle));
        _direction = Quaternion.Euler(0, 0, _angle) * Vector2.up;
    }

    private void Shoot()
    {
        Vector2 origin;
        Vector2 dir;

        origin = rayOrigin();
        dir = Direction;

        for (int i = 0; i <= _maxBounce; i++)
        {
            if (!bounce(i, origin, dir, out origin, out dir))
                break;
            _bouncePoints[i] = origin;
        }

        _shot = true;
    }

    public bool bounce(int bounceID, Vector2 origin, Vector2 direction, out Vector2 newOrigin, out Vector2 newDir)
    {
        // RaycastHit hit;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction);
        if (hit.collider != null)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacles") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Door"))
            {
                Vector2 collisionPoint = origin + direction * hit.distance;
                _distances[bounceID] = hit.distance;
                _pathLength += hit.distance;
                Debug.Log($"hit.normal {hit.normal}");
                float colAngle = Vector2.SignedAngle(-direction, hit.normal);
                Vector2 colDir = Quaternion.Euler(0, 0, colAngle) * hit.normal;

                newOrigin = collisionPoint;
                newDir = colDir;

                return true;

            }
        }
        newOrigin = Vector2.zero;
        newDir = Vector2.zero;
        return false;

    }

    private void traceSoundRay(float t)
    {
        float frontParam = Mathf.Min(t, 1.0f);
        float trailParam = Mathf.Max(t - 0.2f, 0.0f);

        float targetLength = frontParam * _pathLength;
        float trailLength = trailParam * _pathLength;

        float segmentStart = 0;
        float segmentEnd = _distances[0];

        int segmentID = 0;

        while (trailLength > 0 && trailLength > segmentEnd)
        {
            segmentStart += _distances[segmentID];
            segmentEnd += _distances[++segmentID];
        }
        segmentStart /= _pathLength;
        segmentEnd /= _pathLength;

        Vector2 startTrailSegment;
        Vector2 endTrailSegment = _bouncePoints[segmentID];
        if (segmentID == 0)
            startTrailSegment = rayOrigin();
        else
            startTrailSegment = _bouncePoints[segmentID - 1];

        Vector2 pos = startTrailSegment + (endTrailSegment - startTrailSegment) * ((trailParam - segmentStart) / (segmentEnd - segmentStart));

        for (int i = 0; i <= segmentID; i++)
            _lineRenderer.SetPosition(i, pos);

        if (_trailRenderer[_currentShot].positionCount <= segmentID + 1)
            _trailRenderer[_currentShot].positionCount = segmentID + 2;
        _trailRenderer[_currentShot].SetPosition(segmentID + 1, pos);

        segmentStart = 0;
        segmentEnd = _distances[0];
        segmentID = 0;

        while (targetLength > segmentEnd)
        {
            segmentStart += _distances[segmentID];
            segmentEnd += _distances[++segmentID];
        }
        segmentStart /= _pathLength;
        segmentEnd /= _pathLength;

        if (_lineRenderer.positionCount <= segmentID + 1)
            _lineRenderer.positionCount = segmentID + 2;

        Vector2 startPos;
        Vector2 endPos = _bouncePoints[segmentID];
        if (segmentID == 0)
            startPos = rayOrigin();
        else
            startPos = _bouncePoints[segmentID - 1];

        pos = startPos + (endPos - startPos) * ((frontParam - segmentStart) / (segmentEnd - segmentStart));
        _lineRenderer.SetPosition(segmentID + 1, pos);
        _lightPoints[(_currentShot) * (_maxBounce + 2) + segmentID + 1].position = new Vector3(pos.x, pos.y, -1.5f);
    }

    // Start is called before the first frame update
    public void Init()
    {
        _shot = false;
        _ended = false;
        _currentShot = 0;
        _pathLength = 0.0f;
        _positionParam = 0.0f;

        _trailRenderer = new LineRenderer[_shotNumber];
        for (int i = 0; i < _shotNumber; i++)
            _trailRenderer[i] = Instantiate(_trailPrefab, transform).GetComponent<LineRenderer>();

        _lineRenderer.positionCount = 2;
        _trailRenderer[0].positionCount = 1;
        _trailRenderer[0].SetPosition(0, rayOrigin());

        _lightPoints = new Transform[(_maxBounce + 2) * _shotNumber];
        _bouncePoints = new Vector2[_maxBounce + 1];
        _distances = new float[_maxBounce + 1];

        for (int i = 0; i < (_maxBounce + 2) * _shotNumber; i++)
        {
            _lightPoints[i] = Instantiate(_pointLightPrefab, _pointLightParent.transform).transform;
            _lightPoints[i].position = rayOrigin();
        }

        _lineRenderer.SetPosition(0, rayOrigin());
        _lightPoints[0].position = new Vector3(rayOrigin().x, rayOrigin().y, -1.5f);
        _isInitiated = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isInitiated) return;
        if (!_shot & _currentShot < _shotNumber)
        {
            updateAngle(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            updateDir();
            _lineRenderer.SetPosition(1, rayOrigin() + Direction.normalized * 2);
            if (Input.GetMouseButton(0))
                Shoot();
        }

        if (_shot & !_ended)
        {
            _positionParam += Time.deltaTime * 0.2f;

            if (_positionParam > 1.2f)
            {
                _positionParam = 0.0f;
                _pathLength = 0.0f;
                _lineRenderer.positionCount = 0;
                _ended = false;
                _shot = false;
                _currentShot++;

                if (_currentShot < _shotNumber)
                {
                    _trailRenderer[_currentShot].positionCount = 1;
                    _trailRenderer[_currentShot].SetPosition(0, rayOrigin());
                    _lineRenderer.positionCount = 2;
                    _lineRenderer.SetPosition(0, rayOrigin());
                }

            }
            else
            {
                traceSoundRay(_positionParam);
            }
        }



    }

}
