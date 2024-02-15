using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;


public class EchoLocator : MonoBehaviour
{

    // private values should with a prefix _
    private Vector2 _direction = Vector2.up;

    [SerializeField]
    private LineRenderer[] _trailRenderer;
    [SerializeField]
    private bool _isInitiated;
    [SerializeField]
    private int _maxBounce;
    [SerializeField]
    private int _shotNumber;
    [SerializeField]
    private float _angle = -90;
    [SerializeField]
    private Transform _door;
    [SerializeField]
    private LineRenderer _lineRenderer;
    [SerializeField]
    private GameObject _trailPrefab;
    [SerializeField]
    private GameObject _pointLightPrefab;
    [SerializeField]
    private GameObject _pointLightParent;
    [SerializeField]
    private Material lineMat;
    [SerializeField]
    private LayerMask _layerMask;

    private Transform[] _lightPoints;
    private Vector2[] _bouncePoints;

    private bool _shot;
    private bool _done;
    private int _currentShot;
    private float[] _distances;
    private float _pathLength;
    private float _tracingPos;

    public UnityEvent LocationEndEvent;

    public bool Done { get => _done; }
    public Transform Door { get => _door; set => _door = value; }

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
        dir = _direction;

        for (int i = 0; i <= _maxBounce; i++)
        {
            if (bounce(i, origin, dir, out origin, out dir))
                _bouncePoints[i] = origin;
            else
                break;
        }

        _shot = true;
    }

    public bool bounce(int bounceID, Vector2 origin, Vector2 direction, out Vector2 newOrigin, out Vector2 newDir)
    {

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, Mathf.Infinity, _layerMask);
        if (hit.collider != null)
        {
            Vector2 collisionPoint = origin + direction * hit.distance;
            float angle = Vector2.SignedAngle(-direction, hit.normal);

            _distances[bounceID] = hit.distance;
            _pathLength += hit.distance;

            newOrigin = collisionPoint + hit.normal * 0.001f;
            newDir = Quaternion.Euler(0, 0, angle) * hit.normal;

            return true;
        }

        newOrigin = Vector2.zero;
        newDir = Vector2.zero;
        return false;
    }

    private bool traceSoundRay(float t)
    {
        float frontParam = Mathf.Min(t, 1.0f);

        float targetLength = frontParam * _pathLength;

        float trailLength = Mathf.Max((t * _pathLength - 10.0f), 0.0f);
        float trailParam = trailLength / _pathLength;

        float segmentStart = 0;
        float segmentEnd = _distances[0];

        int segmentID = 0;

        if (trailLength > _pathLength)
        {

            return false;
        }

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

        Vector2 trailPos = startTrailSegment + (endTrailSegment - startTrailSegment) * ((trailParam - segmentStart) / (segmentEnd - segmentStart));
        for (int i = 0; i <= segmentID; i++)
            _lineRenderer.SetPosition(i, trailPos);

        if (segmentID < 1)
        {
            _trailRenderer[_currentShot].positionCount = 2;
            _trailRenderer[_currentShot].SetPosition(1, trailPos);
        }

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

        Vector2 pos = startPos + (endPos - startPos) * ((frontParam - segmentStart) / (segmentEnd - segmentStart));
        _lineRenderer.SetPosition(segmentID + 1, pos);
        for(int i = 0; i < segmentID; i++)
        {
            _lightPoints[(_currentShot) * (_maxBounce + 2) + segmentID].GetComponent<Light2D>().intensity = 1;
            _lightPoints[(_currentShot) * (_maxBounce + 2) + segmentID].position = _bouncePoints[i];
        }
        if (t>1.0f)
        {
            _lightPoints[(_currentShot) * (_maxBounce + 2) + _maxBounce + 1].GetComponent<Light2D>().intensity = 1;
            _lightPoints[(_currentShot) * (_maxBounce + 2) + _maxBounce + 1].position = _bouncePoints[_maxBounce];
        }



        return true;
    }

    // Start is called before the first frame update
    public void Init()
    {
        _shot = false;
        _done = false;
        _currentShot = 0;
        _pathLength = 0.0f;
        _tracingPos = 0.0f;

        _trailRenderer = new LineRenderer[_shotNumber];
        for (int i = 0; i < _shotNumber; i++)
            _trailRenderer[i] = Instantiate(_trailPrefab, transform).GetComponent<LineRenderer>();

        _trailRenderer[0].positionCount = 1;
        _trailRenderer[0].SetPosition(0, rayOrigin());

        _lightPoints = new Transform[(_maxBounce + 2) * _shotNumber +1];
        _bouncePoints = new Vector2[_maxBounce + 1];
        _distances = new float[_maxBounce + 1];

        for (int i = 0; i < (_maxBounce + 2) * _shotNumber; i++)
        {
            _lightPoints[i] = Instantiate(_pointLightPrefab, _pointLightParent.transform).transform;
            _lightPoints[i].position = rayOrigin();
            _lightPoints[i].GetComponent<Light2D>().intensity = 0;
        }

        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, rayOrigin());

        _lightPoints[0].position = new Vector3(rayOrigin().x, rayOrigin().y, -1.5f);
        _lightPoints[0].GetComponent<Light2D>().intensity = 1;

        LocationEndEvent = new UnityEvent();

        _isInitiated = true;
    }

    void nextShot()
    {
        _tracingPos = 0.0f;
        _pathLength = 0.0f;
        _lineRenderer.positionCount = 0;
        _shot = false;
        _currentShot++;

        if (_currentShot < _shotNumber)
        {
            _trailRenderer[_currentShot].positionCount = 1;
            _trailRenderer[_currentShot].SetPosition(0, rayOrigin());
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0, rayOrigin());
        }
        else
        {
            _done = true;
            LocationEndEvent.Invoke();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isInitiated) return;
        if (!_shot & !_done)
        {
            updateAngle(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            updateDir();
            _lineRenderer.SetPosition(1, rayOrigin() + _direction.normalized * 2);
            if (Input.GetMouseButton(0))
                Shoot();
        }

        if (_shot)
        {
            _tracingPos += Time.deltaTime / _pathLength * 6.0f;

            if (!traceSoundRay(_tracingPos))
                nextShot();
        }
    }

}
