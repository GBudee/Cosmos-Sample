using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using Cinemachine;
using Cinemachine.Utility;
using Managers;
using UI;
using UI.Menus;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;

namespace NavigationMode
{
    public class NavigationController : MonoBehaviour, ISaveable
    {
        [Header("References")]
        [SerializeField] private Transform _CameraAnchor;
        [SerializeField] private CinemachineVirtualCamera _VirtualCamera;
        [SerializeField] private CapsuleCollider _CapsuleCollider;
        [SerializeField] private LayerMask _CollisionMask;
        [SerializeField] private LayerMask _GroundMask;
        [SerializeField] private PlanetDescription _Planet;
        [SerializeField] private GameObject _Flashlight;
        [Header("Values")]
        [SerializeField] private float _MoveSpeed = 12f;
        [SerializeField] private float _TimeToMaxSpeed = .2f;
        [SerializeField] private float _FootstepInterval = 1f; // Unity units of travel per footstep sound
        [Tooltip("Instantaneous Speed Change")][SerializeField] private float _JumpImpulse = 16f;
        [SerializeField] private float _Gravity = -16f;
        
        public event System.Action<bool> OnEnabledChange;

        public Transform CameraAnchor => _CameraAnchor;
        public bool FlashlightOn => _Flashlight.activeSelf;
        public bool Run { get; private set; }
        public bool ShowObjectiveMenu { get; set; }
        public bool ShowCheatDeckMenu { get; set; }
        
        private bool _hasFocus;
        
        private Vector2 _screenLook;
        private Vector3 _velocity;
        private float? _footstepTracker;
        private enum State { Grounded, Jumping, Falling }
        private bool _jumping;
        
        void OnEnable()
        {
            _VirtualCamera.enabled = true;
            _Flashlight.SetActive(false);
            OnEnabledChange?.Invoke(true);
            Run = false;
        }
        
        void OnDisable()
        {
            _VirtualCamera.enabled = false;
            UpdateFocus();
            _Flashlight.SetActive(false);
            OnEnabledChange?.Invoke(false);
            _velocity = Vector3.zero;
            Run = false;
        }
        
#if UNITY_EDITOR
        public void OnValidate()
        {
            if (_VirtualCamera != null) _VirtualCamera.enabled = enabled;
        }
#endif
        
        public void Save(int version, BinaryWriter writer, bool changingScenes)
        {
            writer.Write(changingScenes);
            if (!changingScenes)
            {
                writer.Write(transform.position);
                writer.Write(transform.eulerAngles);
                writer.Write(_screenLook);
            }
        }
        
        public void Load(int version, BinaryReader reader)
        {
            var changedScenes = reader.ReadBoolean();
            if (!changedScenes)
            {
                transform.position = reader.ReadVector3();
                transform.eulerAngles = reader.ReadVector3();
                _screenLook = reader.ReadVector2();
            }
        }
        
        void Update()
        {
            _VirtualCamera.m_Lens.FieldOfView = Settings.Current.FOV;
            
            UpdateFocus();
            if (!_hasFocus) return;
            
            // Toggle flashlight enabled
            if (Input.GetKeyDown(KeyCode.Q)) _Flashlight.SetActive(!_Flashlight.activeSelf);
            Run = Input.GetKey(KeyCode.LeftShift);
            
            // Translation input
            var inputDir = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) inputDir += Vector3.forward;
            if (Input.GetKey(KeyCode.A)) inputDir += Vector3.left;
            if (Input.GetKey(KeyCode.S)) inputDir += Vector3.back;
            if (Input.GetKey(KeyCode.D)) inputDir += Vector3.right;
            
            // Jumping input
            // TODO: Jumping needs in-air collisions and the ground-sphere-cast bug fixed
            //if (!_jumping && Input.GetKey(KeyCode.Space))
            //{
            //    _velocity += transform.up * _JumpImpulse;
            //    _jumping = true;
            //}
            
            // Update position
            var position = transform.position;
            WalkableSurface walkableSurface = null;
            if (_jumping)
            {
                // Jumping movement
                _velocity += transform.up * _Gravity * Time.deltaTime;
                position += _velocity * Time.deltaTime;
                
                // TODO: Test for collisions in the air, and detect ground w/ spherecast
                var groundPos = _Planet.ProjectOnSurface(position, _GroundMask).point;
                bool touchingGround = Vector3.Distance(groundPos, _Planet.Origin) + .001f >= Vector3.Distance(position, _Planet.Origin);
                if (touchingGround)
                {
                    position = groundPos;
                    _velocity = Vector3.ProjectOnPlane(_velocity, transform.up);
                    _jumping = false;
                }
            }
            else
            {
                // Ground movement
                var cameraSpaceInput = _CameraAnchor.TransformDirection(inputDir);
                var moveSpeed = _MoveSpeed;
                if (Run) moveSpeed *= 1.75f;
                var targetVelocity = Vector3.ProjectOnPlane(cameraSpaceInput, transform.position - _Planet.Origin).normalized * moveSpeed;
                if (_velocity.magnitude > _MoveSpeed) _velocity = _velocity.normalized * moveSpeed;
                _velocity = Vector3.MoveTowards(_velocity, targetVelocity, (moveSpeed / _TimeToMaxSpeed) * Time.deltaTime);
                
                // Test for collision
                TestForCollision(position, transform.up, ref _velocity);
                position += _velocity * Time.deltaTime;
                
                // Stay exactly on surface (TODO: After fixing spherecast bug, turn this back on)
                //if (!PlaceOnGround(ref position, transform.up, probeDist: 1f))
                //{
                //    _jumping = true;
                //}
                var surfaceHit = _Planet.ProjectOnSurface(position, _GroundMask);
                position = surfaceHit.point;
                walkableSurface = surfaceHit.collider?.GetComponent<WalkableSurface>();
            }
            var posDelta = Vector3.Distance(transform.position, position);
            transform.position = position;
            
            // Play footstep sounds
            if (inputDir == Vector3.zero)
            {
                if (_footstepTracker != null) // Input stopped
                {
                    Service.AudioController.FadeOut("Footstep");
                    _footstepTracker = null;
                }
            }
            else
            {
                bool playFootstep = false;
                if (_footstepTracker == null) // Input begun
                {
                    _footstepTracker = 0f;
                    playFootstep = true;
                }
                _footstepTracker += posDelta;
                if (_footstepTracker > _FootstepInterval) // Footstep interval passed
                {
                    _footstepTracker -= _FootstepInterval;
                    playFootstep = true;
                }
                if (playFootstep) Service.AudioController.Play(walkableSurface?.FootstepSound ?? "Footstep", position, randomizer: walkableSurface?.SoundCount ?? 3);
            }
            
            // Update orientation (based on planet "gravity")
            var upVec = (position - _Planet.Origin).normalized;
            foreach (var flatZone in _Planet.FlatZones) upVec = flatZone.Evaluate(position, upVec); // Use non-spherical up if in a "flat zone"
            var forwardVec = Vector3.ProjectOnPlane(transform.forward, upVec);
            transform.rotation = Quaternion.LookRotation(forwardVec, upVec);
            
            // Camera mouse look
            const float MOUSE_SENSITIVITY = 3f;
            Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            _screenLook += mouseInput * MOUSE_SENSITIVITY * Settings.Current.MouseSensitivity;
            _screenLook.x = Mathf.Repeat(_screenLook.x, 360f);
            _screenLook.y = Mathf.Clamp(_screenLook.y, -90f, 90f);
            _CameraAnchor.localEulerAngles = new Vector3(-_screenLook.y, _screenLook.x, 0);
        }
        
        private void UpdateFocus()
        {
            bool value = !(EscapeMenu.Paused || ShowObjectiveMenu || ShowCheatDeckMenu) && enabled;
            if (value != _hasFocus)
            {
                Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !value;
                
                _hasFocus = value;
            }
        }
        
        private void TestForCollision(Vector3 position, Vector3 upVec, ref Vector3 velocity)
        {
            var posDelta = velocity * Time.deltaTime;
            
            // Check whether there are any overlapping colliders
            Vector3 p1 = position + upVec * _CapsuleCollider.radius; // Lower capsule end
            Vector3 p2 = p1 + upVec * (_CapsuleCollider.height - _CapsuleCollider.radius * 2f); // Upper capsule end
            var overlappingColliders = Physics.OverlapCapsule(p1 + posDelta, p2 + posDelta, _CapsuleCollider.radius, _CollisionMask);
            
            // If so, add a depenetration component to velocity
            // TODO: Make this the average of all overlap sources
            if (overlappingColliders.Length <= 0) return;
            var totalDepen = Vector3.zero;
            foreach (var collider in overlappingColliders)
            {
                Physics.ComputePenetration(_CapsuleCollider, _CapsuleCollider.transform.position + posDelta, _CapsuleCollider.transform.rotation
                    , collider, collider.transform.position, collider.transform.rotation, out Vector3 depenDir, out float depenDist);
                totalDepen += depenDir * depenDist;
            }
            totalDepen /= overlappingColliders.Length;
            if (Vector3.Angle(totalDepen, upVec) < 75f) return; // Ignore collisions that push you up
            velocity += totalDepen / Time.deltaTime;
        }
        
        private bool PlaceOnGround(ref Vector3 position, Vector3 upVec, float probeDist)
        {
            // TODO: Fix bug where the spherecast could get interrupted by a non-relevant surface (i.e. the truck), thus causing the ground to no longer be detected
            
            upVec = upVec.normalized;
            Vector3 p2 = position + upVec * (_CapsuleCollider.height - _CapsuleCollider.radius); // Upper capsule end
            float intendedDist = _CapsuleCollider.height - _CapsuleCollider.radius * 2f;
            var groundDetected = Physics.SphereCast(p2, _CapsuleCollider.radius, -upVec, out var hitInfo, intendedDist + probeDist, _GroundMask);
            if (!groundDetected || Vector3.Angle(hitInfo.normal, upVec) > 75f) return false; // Ignore contacts which aren't ground contacts (too steep)
            position -= upVec * (hitInfo.distance - intendedDist);
            return true;
        }
    }
}
