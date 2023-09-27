#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor;
using System.Reflection;
using System;
using UnityEditor.SceneManagement;
using System.Drawing.Printing;
using UnityEngine.SocialPlatforms;
using Unity.VisualScripting;
using UnityEditor.ProBuilder;

namespace IzumiTools
{
    [EditorTool("Duct Snap Tool")]
    public class DuctSnapTool : EditorTool
    {
        [SerializeField] Texture2D _toolIcon;
        GUIContent _iconContent;

        private DuctSnapToolSettings _settings;

        private Transform _activeTransform;
        private List<Transform> _activeTransformChildren = new List<Transform>();
        private List<TransformLayer> _activeTransformChildrenLayers = new List<TransformLayer>();
        private SphereCollider _activeTransformCollider;
        private Renderer _activeTransformRenderer;

        private object _sceneOverlayWindow;
        private MethodInfo _showSceneViewOverlay;

        private float _previousGuiHotcontrol;

        private Color _upAxisColor = Color.green;

        private const string SETTINGS_NAME = "SurfaceAlignTool_Settings";
        private const int IGNORE_RAYCAST_LAYER = 2;

        private static bool mirrored;

        void OnEnable()
        {
            _iconContent = new GUIContent()
            {
                image = _toolIcon,
                text = "Duct Snap Tool",
                tooltip = "Duct Snap Tool"
            };

            _settings = Resources.Load(SETTINGS_NAME) as DuctSnapToolSettings;
#if UNITY_2020_2_OR_NEWER
            ToolManager.activeToolChanged += ToolManager_activeToolChanged;
#else
            UnityEditor.EditorTools.EditorTools.activeToolChanged += ToolManager_activeToolChanged;
#endif

            EnableSettingsWindow();

            _previousGuiHotcontrol = GUIUtility.hotControl;
        }

        private void OnDisable()
        {

#if UNITY_2020_2_OR_NEWER
            ToolManager.activeToolChanged -= ToolManager_activeToolChanged;
#else
            UnityEditor.EditorTools.EditorTools.activeToolChanged -= ToolManager_activeToolChanged;
#endif
        }

        private void ToolManager_activeToolChanged()
        {
#if UNITY_2020_2_OR_NEWER
            if (ToolManager.IsActiveTool(this))
#else
            if (UnityEditor.EditorTools.EditorTools.IsActiveTool(this))
#endif
            {
                _settings = Resources.Load(SETTINGS_NAME) as DuctSnapToolSettings;
                return;
            }
            if (_activeTransformCollider != null)
            {
                DestroyImmediate(_activeTransformCollider.gameObject);
                ResetLayer();
                _activeTransform = null;
            }
        }

        public override GUIContent toolbarIcon
        {
            get { return _iconContent; }
        }

        [UnityEditor.ShortcutManagement.Shortcut("Duct Snap Tool", KeyCode.D)]
        static void DuctSnapShortcut()
        {
            if (ToolManager.activeToolType == typeof(DuctSnapTool))
            {
                ToolManager.RestorePreviousTool();
            }
            else
            {
                ToolManager.SetActiveTool<DuctSnapTool>();
            }
        }

        private void SetActiveTransform(Transform transform)
        {
            if (_activeTransform != transform)
            {
                DestroyCollider();
                ResetLayer();

                _activeTransform = transform;

                if (_activeTransform == null) { return; }

                GameObject tempColliderObject = new GameObject("SurfaceAlignTool_Temp_Collider");
                tempColliderObject.transform.localScale = Vector3.one;
                tempColliderObject.transform.SetParent(_activeTransform);
                tempColliderObject.transform.localPosition = Vector3.zero;
                tempColliderObject.transform.localEulerAngles = Vector3.zero;
                tempColliderObject.hideFlags = HideFlags.HideAndDontSave;

                _activeTransformCollider = tempColliderObject.AddComponent<SphereCollider>();
                _activeTransformCollider.center = Vector3.zero;
                _activeTransformCollider.radius = _settings.SnapRadius;

                _activeTransformChildren.Clear();
                FillActiveTransformChildrenListRecursive(_activeTransform);

                _activeTransformRenderer = _activeTransform.GetComponent<Renderer>();
                if (_activeTransformRenderer != null)
                {
                    Vector3 boundsExtents = _activeTransformRenderer.bounds.extents;
                    float largestExtent = 0f;
                    if (boundsExtents.x > largestExtent) { largestExtent = boundsExtents.x; }
                    if (boundsExtents.y > largestExtent) { largestExtent = boundsExtents.y; }
                    if (boundsExtents.z > largestExtent) { largestExtent = boundsExtents.z; }

                    _activeTransformCollider.radius += largestExtent;
                }

                Physics.autoSyncTransforms = true;
            }
        }

        private bool IsActiveTool()
        {
#if UNITY_2020_2_OR_NEWER
            return ToolManager.IsActiveTool(this);
#else
            return UnityEditor.EditorTools.EditorTools.IsActiveTool(this);
#endif
        }
        public override void OnToolGUI(EditorWindow window)
        {
            //If we're not in the scene view, exit.
            if (!(window is SceneView)) { return; }

            //If we're not the active tool, exit.
            if (!IsActiveTool())
            {
                DestroyCollider();
                ResetLayer();
                return;
            }

            if (_settings == null) { return; }

            // Get selected object
            SetActiveTransform(Selection.activeTransform);
            if (!_activeTransform) { return; }
            if (!_activeTransformCollider) { return; }

            if (_showSceneViewOverlay != null && _sceneOverlayWindow != null) { _showSceneViewOverlay.Invoke(null, new object[] { _sceneOverlayWindow }); };

            KeepColliderScale();

            EditorGUI.BeginChangeCheck();

            Quaternion handleRotation = _activeTransform.rotation;
            if (Tools.pivotRotation == PivotRotation.Global)
            {
                handleRotation = Quaternion.Euler(Vector3.zero);
            }

            Vector3 targetPosition = Handles.PositionHandle(_activeTransform.position, handleRotation);
          
            Quaternion targetRotation = _activeTransform.rotation;


            Handles.color = Color.yellow;
#if UNITY_2022_1_OR_NEWER
            Handles.FreeMoveHandle(1, _activeTransform.position, .25f * HandleUtility.GetHandleSize(_activeTransform.position), Vector3.one, Handles.SphereHandleCap);
#else
            Handles.FreeMoveHandle(1, _activeTransform.position, handleRotation, .25f * HandleUtility.GetHandleSize(_activeTransform.position), Vector3.one, Handles.SphereHandleCap);
#endif

            Vector3 mouseRaycastNormal = Vector3.zero;

            // Handle mouse control (FreeMoveHandle)
            if (GUIUtility.hotControl == 1) 
            {
                if (_previousGuiHotcontrol != 1)
                {
                    SetupLayer();
                    _previousGuiHotcontrol = GUIUtility.hotControl;
                }
                targetPosition = GetCurrentMousePositionInScene(ref mouseRaycastNormal);
                targetRotation = _activeTransform.rotation;
            }
            else if (GUIUtility.hotControl != _previousGuiHotcontrol)
            {
                if (_previousGuiHotcontrol == 1)
                {
                    ResetLayer();
                }     
                _previousGuiHotcontrol = GUIUtility.hotControl;
            }

            SnapRoot snapRoot = _activeTransform.GetComponent<SnapRoot>();
            if (snapRoot != null && snapRoot.Anchors.Count > 0)
            {
                SnapAnchor myAnchor = snapRoot.Anchors[0];
                // Check snap anchors in range
                float closestDistance = _activeTransformCollider.radius;
                SnapAnchor closestSnapAnchor = null;
                foreach (var snapAnchor in GameObject.FindGameObjectsWithTag("SnapAnchor"))
                {
                    if (snapAnchor.GetComponentInParent<SnapRoot>() == snapRoot)
                        continue;
                    float distance = Vector3.Distance(snapAnchor.transform.position, targetPosition);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestSnapAnchor = snapAnchor.GetComponent<SnapAnchor>();
                    }
                }

                // Snap and align to SnapAnchor
                if (closestSnapAnchor != null)
                {
                    if (GUIUtility.hotControl == 1 || GUIUtility.hotControl != 2 && GUIUtility.hotControl != 3)
                    {
                        targetRotation = closestSnapAnchor.transform.rotation * Quaternion.Inverse(myAnchor.transform.rotation) * _activeTransform.transform.rotation * Quaternion.Euler(Vector3.up * 180);
                        if(mirrored)
                        {
                            _activeTransform.localScale = _activeTransform.localScale.Set(x: -_activeTransform.localScale.x);
                            foreach(var boxCollider in _activeTransform.GetComponentsInChildren<BoxCollider>())
                            {
                                boxCollider.size = boxCollider.size.Set(x: -boxCollider.size.x);
                            }
                        }
                        else
                        {
                            _activeTransform.localScale = Vector3.one;
                        }
                        Vector3 diff = _activeTransform.InverseTransformPoint(myAnchor.transform.position);
                        diff.x /= _activeTransform.lossyScale.x;
                        diff.y /= _activeTransform.lossyScale.y;
                        diff.z /= _activeTransform.lossyScale.z;
                        targetPosition = closestSnapAnchor.transform.position - targetRotation * diff;
                    }
                }

                // Apply transform changes
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_activeTransform, "Move " + _activeTransform.name);

                    _activeTransform.position = targetPosition;
                    _activeTransform.rotation = targetRotation;

                    window.Repaint();
                }
            }
        }

        private Vector3 GetCurrentMousePositionInScene(ref Vector3 normal)
        {
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            int layerMask = ~LayerMask.GetMask("Ignore Raycast");
            RaycastHit hit;
#if UNITY_2022_1_OR_NEWER
            if (_activeTransform.gameObject.scene.GetPhysicsScene().Raycast(mouseRay.origin, mouseRay.direction, out hit, float.MaxValue, layerMask))
#else
            if (Physics.Raycast(mouseRay, out hit, float.MaxValue, layerMask))
#endif
            {
                normal = hit.normal;
                return hit.point;
            }
            else
            {
                Plane worldBasePlane = new(Vector3.up, -_settings.WorldBaseY);
                return worldBasePlane.RaycastAndTryGetPoint(mouseRay, out Vector3 point) ? point : mouseRay.origin + mouseRay.direction * 10;
            }
        }

        private void KeepColliderScale()
        {
            if (!_activeTransform || !_activeTransformCollider) { return; }
            if (_activeTransform.localScale.x != 0 && _activeTransform.localScale.y != 0 && _activeTransform.localScale.z != 0)
            {
                _activeTransformCollider.transform.localScale = new Vector3((1f / _activeTransform.lossyScale.x), (1f / _activeTransform.lossyScale.y), (1f / _activeTransform.lossyScale.z));
            }
        }

        private void DestroyCollider()
        {
            if (_activeTransformCollider != null)
            {
                DestroyImmediate(_activeTransformCollider.gameObject);
            }
        }

        private void SetupLayer()
        {
            if (_activeTransform != null)
            {
                _activeTransformChildrenLayers.Clear();
                SetLayersRecursive(_activeTransform, IGNORE_RAYCAST_LAYER);
                _activeTransformCollider.gameObject.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private void ResetLayer()
        {
            if (_activeTransform != null)
            {
                foreach (TransformLayer transformLayer in _activeTransformChildrenLayers)
                {
                    if (transformLayer.Transform != null)
                    {
                        transformLayer.Transform.gameObject.hideFlags = HideFlags.None;
                        transformLayer.Transform.gameObject.layer = transformLayer.Layer;
                    }
                }
                if (_activeTransformCollider)
                {
                    _activeTransformCollider.gameObject.hideFlags = HideFlags.HideAndDontSave;
                    if (!Application.isPlaying)
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
        }

        private void SetLayersRecursive(Transform t, int layer)
        {
            _activeTransformChildrenLayers.Add(new TransformLayer(t, t.gameObject.layer));
            t.gameObject.hideFlags = HideFlags.DontSave;
            t.gameObject.layer = layer;

            for (int i = 0; i < t.childCount; i++)
            {
                Transform child = t.GetChild(i);
                SetLayersRecursive(child, layer);
            }
        }

        private void FillActiveTransformChildrenListRecursive(Transform t)
        {
            _activeTransformChildren.Add(t);

            for (int i = 0; i < t.childCount; i++)
            {
                Transform child = t.GetChild(i);
                FillActiveTransformChildrenListRecursive(child);
            }
        }

        private void EnableSettingsWindow()
        {
#if UNITY_2020_1_OR_NEWER
            Assembly unityEditor = Assembly.GetAssembly(typeof(SceneView));
            Type overlayWindowType = unityEditor.GetType("UnityEditor.OverlayWindow");
            Type sceneViewOverlayType = unityEditor.GetType("UnityEditor.SceneViewOverlay");
            Type windowFuncType = sceneViewOverlayType.GetNestedType("WindowFunction");
            Delegate windowFunc = Delegate.CreateDelegate(windowFuncType, this.GetType().GetMethod(nameof(DoOverlayUI), BindingFlags.Static | BindingFlags.NonPublic));
            Type windowDisplayOptionType = sceneViewOverlayType.GetNestedType("WindowDisplayOption");
            _sceneOverlayWindow = Activator.CreateInstance(overlayWindowType,
                            EditorGUIUtility.TrTextContent("Duct Snap Tool Settings", null, (Texture)null), // Title
                            windowFunc, // Draw function of the window
                            int.MaxValue, // Priority of the window
                            _settings, // Unity Obect that will be passed to the drawing function
                            Enum.Parse(windowDisplayOptionType, "OneWindowPerTarget") //SceneViewOverlay.WindowDisplayOption.OneWindowPerTarget
                        );
            _showSceneViewOverlay = sceneViewOverlayType.GetMethod("ShowWindow", BindingFlags.Static | BindingFlags.Public);
#endif
        }

#if UNITY_2020_1_OR_NEWER
        private static void DoOverlayUI(UnityEngine.Object settingsObject, SceneView sceneView)
        {
            DuctSnapToolSettings settings = (DuctSnapToolSettings)settingsObject;
            GUILayout.Space(10);
            settings.SnapRadius = EditorGUILayout.FloatField("Snap Radius:", settings.SnapRadius);
            settings.WorldBaseY = EditorGUILayout.FloatField("World Base Y:", settings.WorldBaseY);
            mirrored = EditorGUILayout.Toggle("Mirror:", mirrored);
        }
#endif
    }

    [System.Serializable]
    public class NormalPoint
    {
        public Vector3 Position;
        public Vector3 Normal;

        public NormalPoint(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }
    }

    [System.Serializable]
    public class TransformLayer
    {
        public Transform Transform;
        public int Layer;

        public TransformLayer(Transform transform, int layer)
        {
            Transform = transform;
            Layer = layer;
        }
    }
}
#endif