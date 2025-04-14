using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

/// <summary>
/// ActiveCameraColor -> TempTexture -> ActiveCameraColorのコピーを行う
/// </summary>
public class BugTestRenderPass1 : ScriptableRenderPass
{
    private bool _usingCopyPassStep1;
    private bool _usingCopyPassStep2;

    public void Setup(bool usingCopyPassStep1, bool usingCopyPassStep2)
    {
        _usingCopyPassStep1 = usingCopyPassStep1;
        _usingCopyPassStep2 = usingCopyPassStep2;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var resourceData = frameData.Get<UniversalResourceData>();
        var activeColorTexture = resourceData.activeColorTexture;
        var tempDesc = activeColorTexture.GetDescriptor(renderGraph);
        tempDesc.name = "_TempTexture";
        var tempTexture = renderGraph.CreateTexture(tempDesc);

        if (_usingCopyPassStep1)
        {
            // BugTest1 Problem
            // AddCopyPassが正しく機能しない
            // Copy the active color texture to the temporary texture
            renderGraph.AddCopyPass(activeColorTexture, tempTexture);
        }
        else
        {
            // Blit the active color texture to the temporary texture
            renderGraph.AddBlitPass(activeColorTexture, tempTexture, Vector2.one, Vector2.zero);
        }

        if (_usingCopyPassStep2)
        {
            // BugTest1 Problem
            // AddCopyPassが正しく機能しない
            // Copy the temporary texture back to the active color texture
            renderGraph.AddCopyPass(tempTexture, activeColorTexture);
        }
        else
        {
            // Blit the temporary texture back to the active color texture
            renderGraph.AddBlitPass(tempTexture, activeColorTexture, Vector2.one, Vector2.zero);
        }
    }
}
