Shader "Hidden/ConvertColorSpace"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}

        ZTest Off
        ZWrite Off
        Cull Off
        LOD 100

        Pass    // 0
        {
            Name "Linear to sRGB"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            half4 Fragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 color = LOAD_TEXTURE2D_X(_BlitTexture, input.positionCS.xy);
                color.rgb = FastLinearToSRGB(color.rgb);

                return color;
            }
            ENDHLSL
        }

        Pass    // 1
        {
            Name "sRGB to Linear"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            half4 Fragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 color = LOAD_TEXTURE2D_X(_BlitTexture, input.positionCS.xy);
                color.rgb = FastSRGBToLinear(color.rgb);

                return color;
            }
            ENDHLSL
        }

        Pass    // 2
        {
            Name "Linear to sRGB using FrameBufferFetch"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // BugTest2 Problem
            // FrameBufferIndex問題発生時、ここのIndexを1にすれば正しくなる
            // FRAMEBUFFER_INPUT_HALF(1);
            FRAMEBUFFER_INPUT_HALF(0)

            half4 Fragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // BugTest2 Problem
                // FrameBufferIndex問題発生時、ここのIndexを1にすれば正しくなる
                // LOAD_FRAMEBUFFER_INPUT(0, input.positionCS.xy);
                half4 color = LOAD_FRAMEBUFFER_INPUT(0, input.positionCS.xy);
                color.rgb = FastLinearToSRGB(color.rgb);

                return color;
            }
            ENDHLSL
        }

        Pass    // 3
        {
            Name "sRGB to Linear using FrameBufferFetch"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            FRAMEBUFFER_INPUT_HALF(0)

            half4 Fragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 color = LOAD_FRAMEBUFFER_INPUT(0, input.positionCS.xy);
                color.rgb = FastSRGBToLinear(color.rgb);

                return color;
            }
            ENDHLSL
        }
    }
}
