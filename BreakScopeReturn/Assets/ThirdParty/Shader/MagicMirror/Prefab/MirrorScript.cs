using System;
using System.Collections.Generic;
using UnityEngine;

// [ExecuteInEditMode] // Make mirror live-update even when not in play mode
[DisallowMultipleComponent]
[RequireComponent(typeof(Renderer))]
public class MirrorScript : MonoBehaviour
{
    public Camera CameraLookingAtThisMirror
    {
        get => _cameraLookingAtThisMirror;
        set
        {
            _cameraLookingAtThisMirror = value;
        }
    }
    private Camera _cameraLookingAtThisMirror;

    [Header("Setting")]
    [Tooltip("Maximum number of per pixel lights that will show in the mirrored image")]
    public int MaximumPerPixelLights = 2;

    [Tooltip("Texture size for the mirror, depending on how close the player can get to the mirror, this will need to be larger")]
    public int TextureSize = 768;

    [Tooltip("Subtracted from the near plane of the mirror")]
    public float ClipPlaneOffset = 0.07f;

    [Tooltip("Far clip plane for mirro camera")]
    public float FarClipPlane = 1000.0f;

    [Tooltip("Add a flare layer to the reflection camera?")]
    public bool AddFlareLayer = false;

    [Tooltip("For quads, the normal points forward (true). For planes, the normal points up (false)")]
    public bool NormalIsForward = true;

    [Tooltip("Aspect ratio (width / height). Set to 0 to use default.")]
    public float AspectRatio = 0.0f;

    [Tooltip("Set to true if you have multiple mirrors facing each other to get an infinite effect, otherwise leave as false for a more realistic mirror effect.")]
    public bool MirrorRecursion;

    [Header("Internal Reference")]
    public Camera mirrorCamera;

    private Renderer mirrorRenderer;
    private Material mirrorMaterial;

    private RenderTexture reflectionTexture;
    private Matrix4x4 reflectionMatrix;
    private int oldReflectionTextureSize;
    private static bool renderingMirror;
    private bool willRenderMyself;

    private void Start()
    {
        mirrorRenderer = GetComponent<Renderer>();

        if (AddFlareLayer)
        {
            mirrorCamera.gameObject.AddComponent<FlareLayer>();
        }
        mirrorMaterial = mirrorRenderer.sharedMaterial;

        CreateRenderTexture();
    }

    private void CreateRenderTexture()
    {
        if (reflectionTexture == null || oldReflectionTextureSize != TextureSize)
        {
            if (reflectionTexture)
            {
                DestroyImmediate(reflectionTexture);
            }
            reflectionTexture = new RenderTexture(TextureSize, TextureSize, 16);
            reflectionTexture.filterMode = FilterMode.Bilinear;
            reflectionTexture.antiAliasing = 1;
            reflectionTexture.name = "MirrorRenderTexture_" + GetInstanceID();
            reflectionTexture.hideFlags = HideFlags.HideAndDontSave;
            reflectionTexture.autoGenerateMips = false;
            reflectionTexture.wrapMode = TextureWrapMode.Clamp;
            mirrorMaterial.SetTexture("_ReflectionMap", reflectionTexture);
            oldReflectionTextureSize = TextureSize;
        }

        if (mirrorCamera.targetTexture != reflectionTexture)
        {
            mirrorCamera.targetTexture = reflectionTexture;
        }
    }

    private void Update()
    {
        CreateRenderTexture();
        RenderMirror();
        if (willRenderMyself)
        {
            willRenderMyself = false;
        }
    }
    private void OnWillRenderObject()
    {
        willRenderMyself = true;
    }
    public void UpdateCameraProperties()
    {
        mirrorCamera.clearFlags = _cameraLookingAtThisMirror.clearFlags;
        mirrorCamera.backgroundColor = _cameraLookingAtThisMirror.backgroundColor;
        mirrorCamera.orthographic = _cameraLookingAtThisMirror.orthographic;
        mirrorCamera.orthographicSize = _cameraLookingAtThisMirror.orthographicSize;
        if (AspectRatio > 0.0f)
        {
            mirrorCamera.aspect = AspectRatio;
        }
        else
        {
            mirrorCamera.aspect = _cameraLookingAtThisMirror.aspect;
        }
        mirrorCamera.renderingPath = _cameraLookingAtThisMirror.renderingPath;
    }

    public void RenderMirror()
    {
        // bail if we don't have a camera or renderer
        if (renderingMirror || !enabled || CameraLookingAtThisMirror == null ||
            mirrorRenderer == null || mirrorMaterial == null || !mirrorRenderer.enabled)
        {
            return;
        }

        renderingMirror = true;

        int oldPixelLightCount = QualitySettings.pixelLightCount;
        if (QualitySettings.pixelLightCount != MaximumPerPixelLights)
        {
            QualitySettings.pixelLightCount = MaximumPerPixelLights;
        }

        try
        {
            UpdateCameraProperties();

            if (MirrorRecursion)
            {
                mirrorMaterial.EnableKeyword("MIRROR_RECURSION");
                mirrorCamera.ResetWorldToCameraMatrix();
                mirrorCamera.ResetProjectionMatrix();
                mirrorCamera.projectionMatrix = mirrorCamera.projectionMatrix * Matrix4x4.Scale(new Vector3(-1, 1, 1));
                mirrorCamera.cullingMask = CameraLookingAtThisMirror.cullingMask;
                GL.invertCulling = true;
                mirrorCamera.Render();
                GL.invertCulling = false;
            }
            else
            {
                mirrorMaterial.DisableKeyword("MIRROR_RECURSION");
                Vector3 pos = mirrorCamera.transform.position;
                Vector3 normal = (NormalIsForward ? mirrorCamera.transform.forward : mirrorCamera.transform.up);

                // Reflect camera around reflection plane
                float d = -Vector3.Dot(normal, pos) - ClipPlaneOffset;
                Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);
                CalculateReflectionMatrix(reflectionPlane);
                Vector3 oldpos = mirrorCamera.transform.position;
                float oldclip = mirrorCamera.farClipPlane;
                Vector3 newpos = reflectionMatrix.MultiplyPoint(oldpos);

                Matrix4x4 worldToCameraMatrix = CameraLookingAtThisMirror.worldToCameraMatrix;

                worldToCameraMatrix *= reflectionMatrix;
                mirrorCamera.worldToCameraMatrix = worldToCameraMatrix;

                // Clip out background
                Vector4 clipPlane = CameraSpacePlane(ref worldToCameraMatrix, ref pos, ref normal, 1.0f);
                mirrorCamera.projectionMatrix = CameraLookingAtThisMirror.CalculateObliqueMatrix(clipPlane);
                //TODO: With baked occlusion culling this behave wierd...
                mirrorCamera.cullingMatrix = mirrorCamera.projectionMatrix * mirrorCamera.worldToCameraMatrix;
                GL.invertCulling = true;
                mirrorCamera.transform.position = newpos;
                mirrorCamera.farClipPlane = FarClipPlane;
                mirrorCamera.cullingMask = CameraLookingAtThisMirror.cullingMask;
                mirrorCamera.Render();
                mirrorCamera.transform.position = oldpos;
                mirrorCamera.farClipPlane = oldclip;
                GL.invertCulling = false;
            }
        }
        catch(Exception e)
        {
            print(e);
        }
        finally
        {
            renderingMirror = false;
            if (QualitySettings.pixelLightCount != oldPixelLightCount)
            {
                QualitySettings.pixelLightCount = oldPixelLightCount;
            }
        }
    }

    // Cleanup all the objects we possibly have created
    private void OnDisable()
    {
        if (reflectionTexture)
        {
            DestroyImmediate(reflectionTexture);
            reflectionTexture = null;
        }
    }

    private Vector4 CameraSpacePlane(ref Matrix4x4 worldToCameraMatrix, ref Vector3 pos, ref Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * ClipPlaneOffset;
        Vector3 cpos = worldToCameraMatrix.MultiplyPoint(offsetPos);
        Vector3 cnormal = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    private void CalculateReflectionMatrix(Vector4 plane)
    {
        // Calculates reflection matrix around the given plane

        reflectionMatrix.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMatrix.m01 = (-2F * plane[0] * plane[1]);
        reflectionMatrix.m02 = (-2F * plane[0] * plane[2]);
        reflectionMatrix.m03 = (-2F * plane[3] * plane[0]);

        reflectionMatrix.m10 = (-2F * plane[1] * plane[0]);
        reflectionMatrix.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMatrix.m12 = (-2F * plane[1] * plane[2]);
        reflectionMatrix.m13 = (-2F * plane[3] * plane[1]);

        reflectionMatrix.m20 = (-2F * plane[2] * plane[0]);
        reflectionMatrix.m21 = (-2F * plane[2] * plane[1]);
        reflectionMatrix.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMatrix.m23 = (-2F * plane[3] * plane[2]);

        reflectionMatrix.m30 = 0F;
        reflectionMatrix.m31 = 0F;
        reflectionMatrix.m32 = 0F;
        reflectionMatrix.m33 = 1F;
    }

    private static void CalculateObliqueMatrix(ref Matrix4x4 projection, ref Vector4 clipPlane)
    {
        Vector4 q = projection.inverse * new Vector4
        (
            Sign(clipPlane.x),
            Sign(clipPlane.y),
            1.0f,
            1.0f
        );
        Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));
        // third row = clip plane - fourth row
        projection[2] = c.x - projection[3];
        projection[6] = c.y - projection[7];
        projection[10] = c.z - projection[11];
        projection[14] = c.w - projection[15];
    }

    private static float Sign(float a)
    {
        if (a > 0.0f) return 1.0f;
        if (a < 0.0f) return -1.0f;
        return 0.0f;
    }
}