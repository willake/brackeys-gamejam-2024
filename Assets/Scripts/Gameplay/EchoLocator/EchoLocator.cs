using DG.Tweening.Core.Easing;
using Game;
using Game.Audios;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using static Obstacles;


public class EchoLocator : MonoBehaviour
{

    // private values should with a prefix _
    private Vector2 _direction = Vector2.up;

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
    [SerializeField]
    private bool _designMode = false;
    [SerializeField]
    private int _designRayNB = 36;

    private LineRenderer[] _trailRenderer;
    private Transform[] _lightPoints;
    private Vector2[] _bouncePoints;
    private ObstacleType[] _obstaclesType;

    private LineRenderer[] _designRays;
    private Transform[] _designLights;
    private int _drawIdx = 0;

    private bool _shot;
    private bool _isEnable = false;
    private bool _isInitiated = false;
    private bool _hitLast = false;

    private bool _done;
    private int _currentShot;
    private int _segmentTraced;
    private float[] _distances;
    private float _pathLength;
    private float _tracingPos;

    public UnityEvent LocationEndEvent = new();

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

    private void designShoot()
    {
        Vector2 dir;
        Vector2 origin;
        Vector2[] bouncePoints = new Vector2[_maxBounce + 1];

        int interval = Mathf.FloorToInt(180 / _designRayNB);
        int bounceNb = 0;

        for (int i = 0; i < _designRayNB * (_maxBounce + 1); i++)
            _designLights[i].GetComponent<Light2D>().intensity = 0;

        for (int i = 0; i < _designRayNB; i++)
            _designRays[i].positionCount = 0;

        for (int i = -90 + interval; i <= 90 - interval; i += interval)
        {
            int idx = ((i + 90) / interval);
            dir = Quaternion.Euler(0, 0, i) * Vector2.up;
            origin = rayOrigin();
            bounceNb = 0;

            for (int j = 0; j <= _maxBounce; j++)
            {
                RaycastHit2D hit = Physics2D.Raycast(origin, dir, Mathf.Infinity, _layerMask);
                if (hit.collider != null)
                {
                    Vector2 collisionPoint = origin + dir * hit.distance;
                    float angle = Vector2.SignedAngle(-dir, hit.normal);

                    bouncePoints[j] = collisionPoint;

                    origin = collisionPoint + hit.normal * 0.001f;
                    dir = Quaternion.Euler(0, 0, angle) * hit.normal;

                    SpawnDesignLight(idx * (_maxBounce + 1) + j, origin);
                    bounceNb = j;
                }
                else
                {
                    bounceNb = j;
                    break;
                }
            }

            if (idx >= _drawIdx && idx < _drawIdx + 3)
            {
                _designRays[idx].positionCount = bounceNb + 1;
                _designRays[idx].SetPosition(0, rayOrigin());
                switch (idx - _drawIdx)
                {
                    case 0:
                        _designRays[idx].GetComponent<Renderer>().material.color = Color.cyan;
                        break;
                    case 1:
                        _designRays[idx].GetComponent<Renderer>().material.color = Color.magenta;
                        break;
                    case 2:
                        _designRays[idx].GetComponent<Renderer>().material.color = Color.yellow;
                        break;
                }
                for (int j = 0; j < bounceNb; j++)
                {



                    _designRays[idx].SetPosition(j + 1, bouncePoints[j]);
                }
            }


        }

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

            if (hit.transform.GetComponent<Obstacles>() != null)
                _obstaclesType[bounceID] = hit.transform.GetComponent<Obstacles>().type;
            else
                _obstaclesType[bounceID] = ObstacleType.Empty;

            newOrigin = collisionPoint + hit.normal * 0.001f;
            newDir = Quaternion.Euler(0, 0, angle) * hit.normal;

            return true;
        }

        newOrigin = Vector2.zero;
        newDir = Vector2.zero;
        return false;
    }

    private int traceSoundRay(float t)
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

            return -1;
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

        for (int i = 0; i < segmentID; i++)
            SpawnLight(_currentShot * (_maxBounce + 2) + segmentID, _bouncePoints[i]);

        if (t > 1.0f)
            SpawnLight(_currentShot * (_maxBounce + 2) + _maxBounce + 1, _bouncePoints[_maxBounce]);


        return segmentID;
    }

    private void SpawnLight(int lightIdx, Vector2 pos)
    {
        for (int i = 0; i < _lightPoints.Length; i++)
            if (_lightPoints[i].GetComponent<Light2D>().intensity > 0 && Vector2.Distance(_lightPoints[i].position, pos) < _lightPoints[i].GetComponent<Light2D>().pointLightOuterRadius * 0.75f)
                return;


        _lightPoints[lightIdx].GetComponent<Light2D>().intensity = 1;
        _lightPoints[lightIdx].position = pos;
    }

    private void SpawnDesignLight(int lightIdx, Vector2 pos)
    {
        for (int i = 0; i < _designLights.Length; i++)
            if (_designLights[i].GetComponent<Light2D>().intensity > 0 && Vector2.Distance(_designLights[i].position, pos) < _designLights[i].GetComponent<Light2D>().pointLightOuterRadius * 0.75f)
                return;


        _designLights[lightIdx].GetComponent<Light2D>().intensity = 1;
        _designLights[lightIdx].position = pos;
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
        {
            _trailRenderer[i] = Instantiate(_trailPrefab, transform).GetComponent<LineRenderer>();
            _trailRenderer[i].GetComponent<Renderer>().material.color = Color.red;
        }

        _lightPoints = new Transform[(_maxBounce + 2) * _shotNumber];
        _bouncePoints = new Vector2[_maxBounce + 1];
        _obstaclesType = new ObstacleType[_maxBounce + 1];
        _distances = new float[_maxBounce + 1];

        for (int i = 0; i < (_maxBounce + 2) * _shotNumber; i++)
        {
            _lightPoints[i] = Instantiate(_pointLightPrefab, _pointLightParent.transform).transform;
            _lightPoints[i].position = rayOrigin();
            _lightPoints[i].GetComponent<Light2D>().intensity = 0;
        }

        if (_designMode)
        {
            _designLights = new Transform[_designRayNB * (_maxBounce + 1)];
            for (int i = 0; i < _designRayNB * (_maxBounce + 1); i++)
            {
                _designLights[i] = Instantiate(_pointLightPrefab, _pointLightParent.transform).transform;
                _designLights[i].position = rayOrigin();
                _designLights[i].GetComponent<Light2D>().intensity = 0;
                _designLights[i].GetComponent<Light2D>().color = Color.magenta;
            }

            _designRays = new LineRenderer[_designRayNB];
            for (int i = 0; i < _designRayNB; i++)
            {
                _designRays[i] = Instantiate(_trailPrefab, transform).GetComponent<LineRenderer>();
                _designRays[i].name = $"Design Ray {i}";
            }
        }

        _isInitiated = true;
    }

    public void Enable()
    {
        _shot = false;
        _done = false;
        _hitLast = false;
        _currentShot = 0;
        _pathLength = 0.0f;
        _tracingPos = 0.0f;

        if (!_designMode)
        {
            _trailRenderer[0].positionCount = 1;
            _trailRenderer[0].SetPosition(0, rayOrigin());

            _lightPoints[0].position = new Vector3(rayOrigin().x, rayOrigin().y, -1.5f);
            _lightPoints[0].GetComponent<Light2D>().intensity = 1;

            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0, rayOrigin());
            _lineRenderer.SetPosition(1, rayOrigin() + _direction.normalized * 2);
        }

        _isEnable = true;
    }

    public void Disable()
    {
        for (int i = 0; i < _shotNumber; i++)
            _trailRenderer[i].positionCount = 0;

        _isEnable = false;
    }

    public void NextLevel(int rayNb, int bounceNb)
    {
        _shotNumber = rayNb;
        _maxBounce = bounceNb;

        _trailRenderer = new LineRenderer[_shotNumber];
        for (int i = 0; i < _shotNumber; i++)
            _trailRenderer[i] = Instantiate(_trailPrefab, transform).GetComponent<LineRenderer>();

        _lightPoints = new Transform[(_maxBounce + 2) * _shotNumber];

        for (int i = 0; i < (_maxBounce + 2) * _shotNumber; i++)
        {
            _lightPoints[i] = Instantiate(_pointLightPrefab, _pointLightParent.transform).transform;
            _lightPoints[i].position = rayOrigin();
            _lightPoints[i].GetComponent<Light2D>().intensity = 0;
        }

        _bouncePoints = new Vector2[_maxBounce + 1];
        _distances = new float[_maxBounce + 1];
        _obstaclesType = new ObstacleType[_maxBounce + 1];

        if (_designMode)
        {
            _designLights = new Transform[_designRayNB * (_maxBounce + 1)];
            for (int i = 0; i < _designRayNB * (_maxBounce + 1); i++)
            {
                _designLights[i] = Instantiate(_pointLightPrefab, _pointLightParent.transform).transform;
                _designLights[i].position = rayOrigin();
                _designLights[i].GetComponent<Light2D>().intensity = 0;
                _designLights[i].GetComponent<Light2D>().color = Color.magenta;
            }
        }
    }

    void nextShot()
    {
        _tracingPos = 0.0f;
        _pathLength = 0.0f;
        _lineRenderer.positionCount = 0;
        _shot = false;
        _hitLast = false;
        _currentShot++;
        _segmentTraced = 0;

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

    void PlaySound(int i)
    {
        int rand = Random.Range(0, 2);
        WrappedAudioClip sound = ResourceManager.instance.audioResources.gameplayAudios.hitWall1;
        switch (_obstaclesType[i])
        {
            case ObstacleType.Wall:
                sound = (rand == 0 ? ResourceManager.instance.audioResources.gameplayAudios.hitWall1 : ResourceManager.instance.audioResources.gameplayAudios.hitWall2);
                break;
            case ObstacleType.Cupboard:
                sound = (rand == 0 ? ResourceManager.instance.audioResources.gameplayAudios.hitCupboard1 : ResourceManager.instance.audioResources.gameplayAudios.hitCupboard2);
                break;
            case ObstacleType.Ennemy:
                sound = (rand == 0 ? ResourceManager.instance.audioResources.gameplayAudios.hitEnnemy1 : ResourceManager.instance.audioResources.gameplayAudios.hitEnnemy2);
                break;
            case ObstacleType.Empty:
                Debug.LogWarning("Collided obstacles don't have an obstacles componenent and type attached");
                return;
        }
        AudioManager.instance.PlaySFX(sound.clip, sound.volume);
    }


    // Update is called once per frame
    void Update()
    {
        if (!_isInitiated || !_isEnable) return;

        if (_designMode)
        {
            designShoot();
            if (Input.GetKeyDown(KeyCode.UpArrow))
                _drawIdx++;
            if (Input.GetKeyDown(KeyCode.DownArrow))
                _drawIdx--;

            _drawIdx = Mathf.Min(Mathf.Max(0, _drawIdx), _designRayNB - 1);
        }
        else
        {
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

                int currSegment = traceSoundRay(_tracingPos);

                if (currSegment > _segmentTraced)
                {
                    PlaySound(_segmentTraced);
                    _segmentTraced = currSegment;
                }

                if (_tracingPos > 1.0f && !_hitLast)
                {
                    PlaySound(_segmentTraced);
                    _hitLast = true;
                }

                if (currSegment == -1)
                    nextShot();
            }
        }
    }

}
