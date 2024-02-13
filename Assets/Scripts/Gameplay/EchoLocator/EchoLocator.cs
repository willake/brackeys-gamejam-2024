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
    private float _angle = -90;
    [SerializeField]
    private Transform _door;
    [SerializeField]
    private int _maxBounce;
    [SerializeField]
    private Material lineMat;
    [SerializeField]
    private GameObject _pointLightPrefab;
    [SerializeField]
    private GameObject _pointLightParent;

    private float _positionParam = 0.0f;

    private Transform[] _lightPoints;
    private Vector2[] _bouncePoints;
    private float[] _distances;
    private float _pathLength = 0.0f;
    private bool _shot;
    private bool _ended;

    public Vector2 Direction { get => _direction; }

    public Vector2 rayOrigin()
    {
        Vector2 rayOrigin = _door.position;
        rayOrigin.y += _door.localScale.y/2;
        return _door.rotation * rayOrigin;
    }

    public void updateAngle(Vector2 dest)
    {
        float signedAngle = Vector3.SignedAngle(Vector2.up, dest - rayOrigin(), Vector3.forward);
        if(signedAngle > -90 & signedAngle < 90)
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
        RaycastHit hit;

        if (Physics.Raycast(origin, direction, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacles") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Door"))
            {
                Vector2 collisionPoint = origin + direction * hit.distance;
                _distances[bounceID] = hit.distance;
                _pathLength += hit.distance;
                float colAngle = Vector3.SignedAngle(-direction, hit.normal, Vector3.forward);
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

    private Vector2 traceSoundRay(float t)
    {
        float targetLength = t * _pathLength;
        float startLength = 0;
        float endLength = _distances[0];
        int segmentID = 0;

        while (targetLength > endLength)
        {
            startLength += _distances[segmentID];
            endLength += _distances[++segmentID];
        }
        startLength /= _pathLength;
        endLength /= _pathLength;

        if(_lineRenderer.positionCount <= segmentID +1)
            _lineRenderer.positionCount = segmentID + 2;

        Vector2 startPos;
        if (segmentID == 0)
            startPos = rayOrigin();
        else
            startPos = _bouncePoints[segmentID - 1];

        Vector2 endPos = _bouncePoints[segmentID];

        Vector2 pos = startPos + (endPos - startPos) * ((t - startLength) / (endLength - startLength));
        _lineRenderer.SetPosition(segmentID + 1, pos);
        _lightPoints[segmentID + 1].position = new Vector3(pos.x, pos.y, -1.5f);
        return pos;
    }

    // Start is called before the first frame update
    void Start()
    {
        _shot = false;
        _ended = false;

        _lineRenderer.positionCount = 2;

        _lightPoints = new Transform[_maxBounce + 2];
        _bouncePoints = new Vector2[_maxBounce + 1];
        _distances = new float[_maxBounce + 1];

        for (int i = 0; i < _maxBounce + 2; i++)
        {
            _lightPoints[i] = Instantiate(_pointLightPrefab, _pointLightParent.transform).transform;
            _lightPoints[i].position = rayOrigin();
        }

        _lineRenderer.SetPosition(0, rayOrigin() );
        _lightPoints[0].position = new Vector3(rayOrigin().x, rayOrigin().y, -1.5f);
    }

    // Update is called once per frame
    void Update()
    {

        if (!_shot)
        {
            updateAngle(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            updateDir();
            _lineRenderer.SetPosition(1, rayOrigin() + Direction.normalized);
            if (Input.GetMouseButton(0))
                Shoot();
        }

        if (_shot & !_ended)
        {
            _positionParam += Time.deltaTime * 0.33f;

            if(_positionParam > 1.0f)
            {
                _positionParam = 1.0f;
                _ended = true;
            }
            traceSoundRay(_positionParam);
        }
            

    }

}
