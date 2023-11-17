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

    IForceModel _forceModel;

    public GameObject motionModel;

    public void Awake()
    {
        if (motionModel == null) Debug.Log("ForcePoints require a motionModel object with a rigidbody or child articulation bodies to function!");
        _rigidbody = motionModel.GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            _articulationBody = motionModel
                .GetComponentsInChildren<ArticulationBody>()
                .First(body => body.isRoot);
            _articulationBody.useGravity = false;
        }
        else
        {
            _rigidbody.useGravity = false;
        }

        if (motionModel == null && _articulationBody == null)
        {
            Debug.Log("No child articulation bodies or rigidbody found!");
        }

        _waterModel = FindObjectsByType<WaterQueryModel>(FindObjectsSortMode.None)[0];
        _forceModel = motionModel.GetComponent<IForceModel>();

        _pointCount = transform.parent.gameObject.GetComponentsInChildren<ForcePoint>().Length;
    }

    private void FixedUpdate()
    {
        var forcePointPosition = transform.position;
        if (_rigidbody != null)
        {
            _rigidbody.AddForceAtPosition(Physics.gravity / _pointCount, forcePointPosition);
        }
        else
        {
            _articulationBody.AddForceAtPosition(Physics.gravity / _pointCount, forcePointPosition);
        }


        float waterSurfaceLevel = _waterModel.GetWaterLevelAt(forcePointPosition);
        if (forcePointPosition.y < waterSurfaceLevel)
        {
            float displacementMultiplier = Mathf.Clamp01((waterSurfaceLevel - forcePointPosition.y) / depthBeforeSubmerged) * displacementAmount;

            if (_rigidbody != null)
            {
                _rigidbody.AddForceAtPosition(
                    new Vector3(0, Math.Abs(Physics.gravity.y) * displacementMultiplier / _pointCount, 0),
                    forcePointPosition);
            }
            else
            {
                _articulationBody.AddForceAtPosition(
                    new Vector3(0, Math.Abs(Physics.gravity.y) * displacementMultiplier / _pointCount, 0),
                    forcePointPosition);
            }


            if (_forceModel != null)
            {
                //    _rigidbody.AddRelativeForce(_forceModel.GetForceDamping() / _pointCount, ForceMode.Force);
                //    _rigidbody.AddRelativeTorque(_forceModel.GetTorqueDamping() / _pointCount, ForceMode.Force);
            }
        }
    }
}