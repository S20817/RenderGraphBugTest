using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
[RequireComponent(typeof(Camera), typeof(UniversalAdditionalCameraData))]
public class BugTest1 : MonoBehaviour
{
    [SerializeField]
    private RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

    [SerializeField]
    private bool usingCopyPassStep1 = false;

    [SerializeField]
    private bool usingCopyPassStep2 = false;

    private Camera _camera;
    private UniversalAdditionalCameraData _cameraData;
    private BugTestRenderPass1 _renderPass = new();

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
        if (_camera != camera)
        {
            return;
        }

        _renderPass.renderPassEvent = renderPassEvent;
        _renderPass.Setup(usingCopyPassStep1, usingCopyPassStep2);
        _cameraData.scriptableRenderer.EnqueuePass(_renderPass);
    }
}
