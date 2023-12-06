using BezierSolution;
using DG.Tweening;
using LevelDesign;
using UnityEngine;

namespace Highway
{
    public class CarController : MonoBehaviour
    {
        private const float INITIAL_VELOCITY = 3.5f;
        public const float ACCELERATION = .7f;
        private const float DECELERATION = 22f;
        
        [SerializeField] private Transform _FrontAnchor;
        [SerializeField] private Transform _RearAnchor;
        
        public float SplinePos => _splinePos;
        public float CarLength => Vector3.Distance(_FrontAnchor.position, _RearAnchor.position);
        
        private BezierSpline _spline;
        private Transform _splineContainer;
        private float _laneOffset;
        private bool _accelerating;
        private float _acceleration;
        private float _velocity;
        private float _splinePos;
        private float _bob;
        private float _bobDir;
        private float _bobSpeed;
        
        public void Initialize(HighwayBuilder highwayData, float initialSplinePos, float laneOffset)
        {
            _spline = highwayData._Spline;
            _splineContainer = highwayData._Spline.transform.parent;
            _laneOffset = highwayData._RoadWidth * laneOffset;
            _accelerating = true;
            _acceleration = Random.Range(ACCELERATION, ACCELERATION * 6f);
            _velocity = INITIAL_VELOCITY;
            _splinePos = initialSplinePos;
            transform.position = GetLanePos(_splinePos);
            _bob = Random.Range(0, 1f);
            _bobDir = Random.Range(0, 2) == 0 ? -1 : 1;
            _bobSpeed = Random.Range(.25f, .7f);
        }
        
        public void UpdateCar(CarController carInFront, out float resultPos)
        {
            if (carInFront == null)
            {
                var distToPortal = _spline.GetLengthApproximately(_splinePos, 1f);
                _accelerating = distToPortal > HighwayController.CAR_SPACING * 1f;
            }
            else 
            {
                var distToNextBumper = _spline.GetLengthApproximately(_splinePos, carInFront.SplinePos) - carInFront.CarLength;
                if (_accelerating && distToNextBumper < HighwayController.CAR_SPACING * .4f) _accelerating = false;
                else if (!_accelerating && distToNextBumper > HighwayController.CAR_SPACING * .65f) _accelerating = true;
            }
            if (_accelerating) _velocity += _acceleration * Time.deltaTime;
            else _velocity -= DECELERATION * Time.deltaTime;
            _velocity = Mathf.Clamp(_velocity, carInFront == null ? INITIAL_VELOCITY : 0, INITIAL_VELOCITY * 3.5f);
            
            // Update position
            var priorPos = GetLanePos(_splinePos);
            _spline.MoveAlongSpline(ref _splinePos, _velocity * Time.deltaTime);
            var newPos = GetLanePos(_splinePos);
            resultPos = _splinePos;
            
            // Update orientation
            var forwardAxis = (newPos - priorPos).normalized;
            var rightAxis = Vector3.Cross(forwardAxis, _splineContainer.up).normalized;
            var upAxis = Vector3.Cross(rightAxis, forwardAxis);
            if (forwardAxis.sqrMagnitude > .001f)
                transform.rotation = Quaternion.LookRotation(forwardAxis, upAxis);
            
            // Update bob
            const float BOB_WIDTH = .4f;
            _bob += _bobDir * _bobSpeed * Time.deltaTime;
            if (_bob > 1f)
            {
                _bob = 1f - Mathf.Repeat(_bob, 1f);
                _bobDir = -1f;
            }
            else if (_bob < 0f)
            {
                _bob = 1f - Mathf.Repeat(_bob, 1f);
                _bobDir = 1f;
            }
            transform.position = newPos + transform.up * DOVirtual.EasedValue(-BOB_WIDTH * .5f, BOB_WIDTH * .5f, _bob, Ease.InOutSine);
        }
        
        private Vector3 GetLanePos(float splinePos)
        {
            var widthOffset = Vector3.Cross(_spline.GetTangent(splinePos), Vector3.up).normalized * _laneOffset;
            return _spline.GetPoint(splinePos) + widthOffset;
        }
    }
}