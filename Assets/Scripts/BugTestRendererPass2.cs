using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

/// <summary>
/// ActiveCameraColor -sRGBに変換-> TempTexture -> CustomUI描画 -Linearに変換-> ActiveCameraColor
/// </summary>
public class BugTestRenderPass2 : ScriptableRenderPass
{
    private enum Pass
    {
        Convert,
        Revert,
        ConvertUsingFrameBufferFetch,
        RevertUsingFrameBufferFetch,
    }

    private bool _convertUsingFrameBufferFetch;
    private bool _revertUsingFrameBufferFetch;
    private Material _material;

    public void Setup(bool usingFrameBufferFetchStep1, bool usingFrameBufferFetchStep2, Material material)
    {
        _convertUsingFrameBufferFetch = usingFrameBufferFetchStep1;
        _revertUsingFrameBufferFetch = usingFrameBufferFetchStep2;
        _material = material;
    }

    private class CustomBlitPassData
    {
        public TextureHandle Source;
        public Material Material;
        public int PassIndex;
    }
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var resourceData = frameData.Get<UniversalResourceData>();
        var activeColorTexture = resourceData.activeColorTexture;
        var tempDesc = activeColorTexture.GetDescriptor(renderGraph);
        tempDesc.name = "_TempTexture";
        var tempTexture = renderGraph.CreateTexture(tempDesc);

        var convertPass = _convertUsingFrameBufferFetch ? Pass.Convert + 2 : Pass.Convert;
        CustomBlit(renderGraph, resourceData.activeColorTexture, tempTexture, _material,
            (int)convertPass, _convertUsingFrameBufferFetch, "LinearToSRGB");

        // カスタムUI描画
        DrawCustomUI(renderGraph, frameData, tempTexture);

        var revertPass = _revertUsingFrameBufferFetch ? Pass.Revert + 2 : Pass.Revert;
        CustomBlit(renderGraph, tempTexture, resourceData.activeColorTexture, _material,
            (int)revertPass, _revertUsingFrameBufferFetch, "SRGBToLinear");
    }

    private void CustomBlit(RenderGraph renderGraph, in TextureHandle source, in TextureHandle destination,
        Material material, int passIndex, bool usingFrameBufferFetch, string name = "CustomBlit")
    {
        using (var builder = renderGraph.AddRasterRenderPass<CustomBlitPassData>(name, out var passData))
        {
            passData.Material = material;
            passData.PassIndex = passIndex;

            builder.SetRenderAttachment(destination, 0, AccessFlags.Write);
            if (usingFrameBufferFetch)
            {
                builder.SetInputAttachment(source, 0, AccessFlags.Read);
            }
            else
            {
                passData.Source = source;
                builder.UseTexture(source, AccessFlags.Read);
            }

            builder.SetRenderFunc(static (CustomBlitPassData data, RasterGraphContext ctx) =>
            {
                var scaleBias = new Vector4(1, 1, 0, 0);
                if (data.Source.IsValid())
                {
                    Blitter.BlitTexture(ctx.cmd, data.Source, scaleBias, data.Material, data.PassIndex);
                }
                else
                {
                    Blitter.BlitTexture(ctx.cmd, scaleBias, data.Material, data.PassIndex);
                }
            });
        }
    }

    private class DrawCustomUIPassData
    {
        public RendererListHandle RendererList;
    }
    private void DrawCustomUI(RenderGraph renderGraph, ContextContainer frameData, in TextureHandle renderTarget)
    {
        var cameraData = frameData.Get<UniversalCameraData>();
        var renderingData = frameData.Get<UniversalRenderingData>();
        var lightData = frameData.Get<UniversalLightData>();
        var resourceData = frameData.Get<UniversalResourceData>();

        using (var builder = renderGraph.AddRasterRenderPass<DrawCustomUIPassData>("Draw CustomUI", out var passData))
        {
            var shaderTagIds = new List<ShaderTagId> { new("SRPDefaultUnlit") };
            var sortingCriteria = SortingCriteria.CommonTransparent;
            var drawingSettings = RenderingUtils.CreateDrawingSettings(shaderTagIds, renderingData, cameraData, lightData, sortingCriteria);
            var layerMask = LayerMask.GetMask("CustomUI");
            var filteringSettings = new FilteringSettings(RenderQueueRange.transparent, layerMask);
            var rendererListParams = new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings);
            passData.RendererList = renderGraph.CreateRendererList(rendererListParams);

            builder.SetRenderAttachment(renderTarget, 0, AccessFlags.Write);
            // BugTest2 Problem
            // Depthセットしなければ、FrameBuffer Index問題が発生しない
            builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);
            builder.UseRendererList(passData.RendererList);
            builder.SetRenderFunc(static (DrawCustomUIPassData data, RasterGraphContext ctx) =>
            {
                ctx.cmd.DrawRendererList(data.RendererList);
            });
        }
    }
}
