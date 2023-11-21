using System;
using System.Linq;
using DefaultNamespace;
using DefaultNamespace.Water;
using UnityEngine;

// This is a very simple example of how we could compute a buoyancy force at variable points along the body.
// Its not really accurate per se.
// [RequireComponent(typeof(Rigidbody))]
// [RequireComponent(typeof(IForceModel))]
public class ForcePoint : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private ArticulationBody _articulationBody;
    private int _pointCount;

    private WaterQueryModel _waterModel;

    public float depthBeforeSubmerged = 1.5f;
    public float displacementAmount = 1f;

    public GameObject motionModel;
    public bool addGravity = false;
    public void Awake()
    {
        if (motionModel == null) Debug.Log("ForcePoints require a motionModel object with a rigidbody to function!");
        _rigidbody = motionModel.GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            _articulationBody = motionModel
                .GetComponentsInChildren<ArticulationBody>()
                .First(body => body.isRoot);
            addGravity = !_articulationBody.useGravity;
        }
        else
        {
            addGravity = !_rigidbody.useGravity;
        }

        if (motionModel == null && _articulationBody == null)
        {
            Debug.Log("No child articulation bodies or rigidbody found!");
        }

        _waterModel = FindObjectsByType<WaterQueryModel>(FindObjectsSortMode.None)[0];
        _pointCount = transform.parent.gameObject.GetComponentsInChildren<ForcePoint>().Length;
       
    }

    private void FixedUpdate()
    {
        var forcePointPosition = transform.position;
        if (addGravity)
        {
            if (_rigidbody != null)
            {
                _rigidbody.AddForceAtPosition(_rigidbody.mass * Physics.gravity / _pointCount, forcePointPosition);
            }
            else
            {
                _articulationBody.AddForceAtPosition(_articulationBody.mass * Physics.gravity / _pointCount, forcePointPosition);
            }

        }


        float waterSurfaceLevel = _waterModel.GetWaterLevelAt(forcePointPosition);
        if (forcePointPosition.y < waterSurfaceLevel)
        {
            float displacementMultiplier = Mathf.Clamp01((waterSurfaceLevel - forcePointPosition.y) / depthBeforeSubmerged) * displacementAmount;

            if (_rigidbody != null)
            {
                _rigidbody.AddForceAtPosition(
                    new Vector3(0, _rigidbody.mass *Math.Abs(Physics.gravity.y) * displacementMultiplier / _pointCount, 0),
                    forcePointPosition);
            }
            else
            {
                _articulationBody.AddForceAtPosition(
                    new Vector3(0, _articulationBody.mass *Math.Abs(Physics.gravity.y) * displacementMultiplier / _pointCount, 0),
                    forcePointPosition);
            }
        }

    }
}
