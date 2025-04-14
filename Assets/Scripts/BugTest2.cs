using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
[RequireComponent(typeof(Camera), typeof(UniversalAdditionalCameraData))]
public class BugTest2 : MonoBehaviour
{
    [SerializeField]
    private RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

    [SerializeField]
    private Material material;

    [SerializeField]
    private bool convertUsingFrameBufferFetch = false;

    [SerializeField]
    private bool revertUsingFrameBufferFetch = false;

    private Camera _camera;
    private UniversalAdditionalCameraData _cameraData;
    private BugTestRenderPass2 _renderPass = new();

    private void OnEnable()
    {
        _camera = GetComponent<Camera>();
        _cameraData = _camera.GetUniversalAdditionalCameraData();
        RenderPipelineManager.beginCameraRendering += AddRenderPass;
    }

    private void OnDisable()
    {
        _camera = null;
        _cameraData = null;
        RenderPipelineManager.beginCameraRendering -= AddRenderPass;
    }

    private void AddRenderPass(ScriptableRenderContext context, Camera camera)
    {
        if (_camera != camera || !material)
        {
            return;
        }

        _renderPass.renderPassEvent = renderPassEvent;
        _renderPass.Setup(convertUsingFrameBufferFetch, revertUsingFrameBufferFetch, material);
        _cameraData.scriptableRenderer.EnqueuePass(_renderPass);
    }
}
