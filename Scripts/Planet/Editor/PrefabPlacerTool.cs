using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Rendering;
using Utilities;
using Utilities.Noise;

namespace Planet.Editor
{
    [EditorTool("Prefab Placer Tool")]
    public class PrefabPlacerTool : EditorTool
    {
        // References
        private PlanetDescription _planet;
        private GameObject[] _selectedPrefabs;
        private GameObject _targetPrefab;
        private GameObject _spawnCandidate;
        private List<Vector3> _brushPoints;
        
        // Setting values
        private bool _forceStatic = true;
        private bool _alignToSurfaceNormal;
        private float _scaleMult = 1f;
        private bool _randomizeScale;
        private Vector2 _randomizedScaleRange = new Vector2(.5f, 2f);
        private float _yOffset;
        private bool _randomizeYOffset;
        private Vector2 _randomizedYRange = new Vector2(-.1f, .1f);
        private float _yRotation;
        private bool _randomizeRot;
        private Transform _intendedParent;
        private bool _brushMode;
        private float _brushRadius = 5f;
        private float _brushSpacing = .5f;
        private bool _randomizePrefab;
        
        // Add hotkey
        [MenuItem("Tools/PrefabPlacer _u", validate = false)]
        public static void ActivateTool() => ToolManager.SetActiveTool(typeof(PrefabPlacerTool));
        
        // Track active selection
        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            OnSelectionChanged();
        }
        private void OnDisable() => Selection.selectionChanged -= OnSelectionChanged;
        
        private void OnSelectionChanged()
        {
            ClearSpawnCandidate();
            
            // Update the target prefab based on user selection
            var selection = Selection.gameObjects;
            bool validTarget = selection.Length > 0 && selection.All(PrefabUtility.IsPartOfPrefabAsset);
            if (validTarget)
            {
                _selectedPrefabs = selection;
                _targetPrefab = _selectedPrefabs[0];
            }
            else
            {
                _selectedPrefabs = null;
                _targetPrefab = null;
            }
        }
        
        public override void OnActivated()
        {
            //SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Entering Prefab Placer Mode"), .1f);
            _planet = FindObjectOfType<PlanetDescription>();
            if (_planet == null) Debug.LogError("Prefab Placer Tool cannot find any instances of PlanetDescription.cs");
        }
        
        public override void OnWillBeDeactivated()
        {
            //SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Exiting Prefab Placer Mode"), .1f);
            _planet = null;
            ClearSpawnCandidate();
        }
        
        public override void OnToolGUI(EditorWindow window)
        {
            if (window is not SceneView) return;
            var current = Event.current;

            // Override unity's default click handling (i.e. selecting scene objects)
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            // Show settings window
            Handles.BeginGUI();
            GUILayout.FlexibleSpace();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.VerticalScope(new GUIStyle(GUI.skin.box){ padding = new RectOffset(10, 10, 5, 10) }))
                {
                    // Label shows selected prefab name
                    var plural = _selectedPrefabs?.Length > 1;
                    var prefabName = _selectedPrefabs != null ? _selectedPrefabs.Length > 1 ? $"Multiple ({_selectedPrefabs.Length})" : _targetPrefab.name : "None";
                    GUILayout.Label($"Spawning Prefab{(plural ? "s" : "")}:\n<b>{prefabName}</b>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.UpperCenter });
                    
                    // Horizontal line
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    
                    // Settings
                    _forceStatic = EditorGUILayout.Toggle("Force Static", _forceStatic);
                    _alignToSurfaceNormal = EditorGUILayout.Toggle("Align to Surface Normal", _alignToSurfaceNormal);
                    _scaleMult = EditorGUILayout.FloatField("Scale Multiplier" + (current.control ? " (Alt+Scroll)" : ""), _scaleMult);
                    EditorGUI.indentLevel++;
                    _randomizeScale = EditorGUILayout.Toggle("Randomize Scale", _randomizeScale);
                    if (_randomizeScale)
                    {
                        _randomizedScaleRange = EditorGUILayout.Vector2Field("Scale Multiplier Bounds", _randomizedScaleRange);
                    }
                    EditorGUI.indentLevel--;
                    _yOffset = EditorGUILayout.FloatField("Y Offset" + (current.control ? " (Shift+Scroll)" : ""), _yOffset);
                    EditorGUI.indentLevel++;
                    _randomizeYOffset = EditorGUILayout.Toggle("Randomize Y Offset", _randomizeYOffset);
                    if (_randomizeYOffset)
                    {
                        _randomizedYRange = EditorGUILayout.Vector2Field("Y Offset Bounds", _randomizedYRange);
                    }
                    EditorGUI.indentLevel--;
                    _yRotation = EditorGUILayout.Slider("Y Rotation" + (current.control ? " (Scroll Wheel)" : ""), _yRotation, 0f, 360f);
                    EditorGUI.indentLevel++;
                    _randomizeRot = EditorGUILayout.Toggle("Randomize Rotation", _randomizeRot);
                    EditorGUI.indentLevel--;
                    _intendedParent = EditorGUILayout.ObjectField(new GUIContent("Intended Parent"), _intendedParent, typeof(Transform), allowSceneObjects: true) as Transform;
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    _brushMode = EditorGUILayout.Toggle("Brush Mode", _brushMode, new GUIStyle(GUI.skin.toggle) { fontStyle = FontStyle.Bold });
                    EditorGUI.indentLevel++;
                    if (_brushMode) // Brush-mode, i.e. placing multiple object instances in a radius
                    {
                        _brushRadius = EditorGUILayout.Slider("Brush Radius", _brushRadius, 1f, 20f);
                        _brushSpacing = EditorGUILayout.Slider("Brush Spacing", _brushSpacing, .1f, 5f);
                    }
                    else if (_selectedPrefabs?.Length > 1) // Non-brush-mode multi-prefab-select
                    {
                        _randomizePrefab = EditorGUILayout.Toggle("Randomize Prefab", _randomizePrefab);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.PrefixLabel("Active Prefab: ");
                            if (EditorGUILayout.DropdownButton(new GUIContent(_targetPrefab.name), FocusType.Passive))
                            {
                                GenericMenu menu = new GenericMenu();
                                foreach (var prefab in _selectedPrefabs)
                                {
                                    var localPrefab = prefab;
                                    menu.AddItem(new GUIContent(prefab.name), prefab == _targetPrefab, () =>
                                    {
                                        ClearSpawnCandidate();
                                        _targetPrefab = localPrefab;
                                    });
                                }
                                menu.ShowAsContext();
                            }
                        }
                    }
                    EditorGUI.indentLevel--;
                    
                    // Reset button
                    if (GUILayout.Button("Reset Settings", GUILayout.ExpandWidth(false)))
                    {
                        _forceStatic = true;
                        _alignToSurfaceNormal = false;
                        _randomizeScale = false;
                        _randomizedScaleRange = new Vector2(.5f, 2f);
                        _scaleMult = 1f;
                        _yOffset = 0f;
                        _randomizeYOffset = false; 
                        _randomizedYRange = new Vector2(-.1f, .1f);
                        _yRotation = 0f;
                        _randomizeRot = false;
                        _intendedParent = null;
                        _brushMode = false;
                        _brushRadius = 5f;
                        _brushSpacing = .5f;
                        _randomizePrefab = false;
                    }
                }
            }
            Handles.EndGUI();
            
            // Check for ctrl input change
            bool validSpawn = false;
            if (current.control)
            {
                if (_targetPrefab != null)
                {
                    // Accept scroll rotation inputs
                    if (current.type == EventType.ScrollWheel)
                    {
                        current.Use();
                        if (current.shift)
                        {
                            _yOffset += current.delta.y * .025f;
                        }
                        else if (current.alt)
                        {
                            _scaleMult += _scaleMult * current.delta.y * .01f;
                        }
                        else
                        {
                            _yRotation += current.delta.y * 4f;
                            _yRotation = Mathf.Repeat(_yRotation, 360f);
                        }
                    }
                    
                    // Raycast into the scene to try placing an object
                    var mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    var sceneCamera = SceneView.lastActiveSceneView.camera;
                    if (Physics.Raycast(mouseRay, out var hit, sceneCamera.farClipPlane, LayerMask.GetMask("Planet Surface")))
                    {
                        if (_brushMode)
                        {
                            validSpawn = true;
                            
                            // Force redraw for gizmos
                             if (current.type == EventType.MouseMove) SceneView.currentDrawingSceneView.Repaint();
                             
                             // Show brush radius
                             _planet.Gizmos_DrawDisc(hit.point, _brushRadius, new Color(1, 1, 0, .75f), usePositionCenter: true);
                             
                             // Calculate brush points
                             Determinism.Begin(Determinism.GetSeed(hit.point.GetHashCode()));
                             var points = PoissonDiscSampling.Sampling(new Vector2(-_brushRadius, -_brushRadius), new Vector2(_brushRadius, _brushRadius), _brushSpacing);
                             Determinism.End(out var unused);
                             var center = hit.point;
                             var axis = hit.point - _planet.Origin;
                             var orientation = Quaternion.FromToRotation(Vector3.up, axis);
                             var cameraAxis = SceneView.lastActiveSceneView.camera.transform.forward;
                             _brushPoints = points.Where(v => v.magnitude <= _brushRadius).Select(v => center + orientation * new Vector3(v.x, 0, v.y)).ToList();
                             
                             // Show brush points
                             Handles.color = new Color(1f, 1f, 1f, .5f);
                             Handles.zTest = CompareFunction.Always;
                             foreach (var point in _brushPoints) Handles.DrawSolidDisc(point, cameraAxis, HandleUtility.GetHandleSize(point) * .012f);
                        }
                        else
                        {
                            validSpawn = true;
                            var spawnCandidate = GetSpawnCandidate();
                            PlaceInstance(spawnCandidate, _targetPrefab, hit, _yOffset, _yRotation, _scaleMult);
                        }
                    }
                }
                else
                {
                    SceneView.lastActiveSceneView.ShowNotification(new GUIContent("No Prefab Selected"), .1f);
                }
            }
            if (!validSpawn)
            {
                ClearSpawnCandidate();
            }
            
            // Check for left mouse click
            if (validSpawn && current.type == EventType.MouseDown && current.button == 0)
            {
                current.Use();

                if (_brushMode)
                {
                    // Group undo for entire brush placement
                    var actionLabel = $"Spawned {_brushPoints.Count} instances with brush";
                    SceneView.lastActiveSceneView.ShowNotification(new GUIContent(actionLabel), .1f);
                    Undo.SetCurrentGroupName(actionLabel);
                    int group = Undo.GetCurrentGroup();
                    var surfaceMask = LayerMask.GetMask("Planet Surface");

                    foreach (var brushPoint in _brushPoints)
                    {
                        // Spawn prefab instance
                        var prefab = _randomizePrefab ? _selectedPrefabs[Random.Range(0, _selectedPrefabs.Length)] : _targetPrefab;
                        var newInstance = PrefabUtility.InstantiatePrefab(prefab, _intendedParent) as GameObject;
                        Undo.RegisterCreatedObjectUndo(newInstance, $"Spawn");
                        Undo.RegisterCompleteObjectUndo(newInstance, "Place");
                        
                        // Place instance on surface with appropriate randomization
                        var hit = _planet.ProjectOnSurface(brushPoint, surfaceMask);
                        var yOffset = _randomizeYOffset ? Random.Range(_randomizedYRange.x, _randomizedYRange.y) : _yOffset;
                        var yRotation = _randomizeRot ? Random.Range(0f, 360f) : _yRotation;
                        var scaleMult = _randomizeScale ? Random.Range(_randomizedScaleRange.x, _randomizedScaleRange.y) : _scaleMult;
                        PlaceInstance(newInstance, prefab, hit, yOffset, yRotation, scaleMult);
                        
                        if (_forceStatic) newInstance.isStatic = true;
                    }
                    
                    Undo.CollapseUndoOperations(group);
                }
                else
                {
                    // Undoably spawn a permanent instance based on the spawn candidate's position and orientation
                    var newInstance = PrefabUtility.InstantiatePrefab(_targetPrefab, _intendedParent) as GameObject;
                    SceneView.lastActiveSceneView.ShowNotification(new GUIContent($"Spawned {newInstance.name}"), .1f);
                    Undo.RegisterCreatedObjectUndo(newInstance, $"Spawned {newInstance.name}");
                    Undo.RegisterCompleteObjectUndo(newInstance, "Position and Rotate Instance");
                    var candidateTransform = _spawnCandidate.transform;
                    newInstance.transform.position = candidateTransform.position;
                    newInstance.transform.rotation = candidateTransform.rotation;
                    newInstance.transform.localScale = candidateTransform.localScale;
                    if (_forceStatic) newInstance.isStatic = true;
                    
                    // If randomizing values, do so after spawning so the user can see the results in the next spawn candidate
                    if (_randomizePrefab)
                    {
                        ClearSpawnCandidate();
                        _targetPrefab = _selectedPrefabs[Random.Range(0, _selectedPrefabs.Length)];
                    }
                    if (_randomizeRot) _yRotation = Random.Range(0f, 360f);
                    if (_randomizeYOffset) _yOffset = Random.Range(_randomizedYRange.x, _randomizedYRange.y);
                    if (_randomizeScale) _scaleMult = Random.Range(_randomizedScaleRange.x, _randomizedScaleRange.y);
                    
                }
            }
        }

        private void PlaceInstance(GameObject instance, GameObject prefab, RaycastHit hit, float yOffset, float yRotation, float scaleMult)
        {
            // Orient instance
            Vector3 prefabUp = Quaternion.Inverse(prefab.transform.rotation) * Vector3.up;
            Vector3 intendedUp;
            if (_alignToSurfaceNormal)
            {
                intendedUp = hit.normal;
            }
            else
            {
                intendedUp = (hit.point - _planet.Origin).normalized;
                foreach (var flatZone in _planet.FlatZones)
                    intendedUp = flatZone.Evaluate(hit.point, intendedUp); // Use non-spherical up if in a "flat zone"
            }
            instance.transform.rotation = Quaternion.AngleAxis(yRotation, intendedUp) * Quaternion.FromToRotation(prefabUp, intendedUp);
            
            // Scale instance
            instance.transform.localScale = prefab.transform.localScale * scaleMult;
            
            // Position instance
            instance.transform.position = hit.point + intendedUp.normalized * yOffset;
        }
        
        private GameObject GetSpawnCandidate()
        {
            if (_spawnCandidate == null) _spawnCandidate = PrefabUtility.InstantiatePrefab(_targetPrefab, _intendedParent) as GameObject;
            else _spawnCandidate.transform.parent = _intendedParent;
            return _spawnCandidate;
        }
        
        private void ClearSpawnCandidate()
        {
            if (_spawnCandidate != null) DestroyImmediate(_spawnCandidate);
            _spawnCandidate = null;
        }
    }
}