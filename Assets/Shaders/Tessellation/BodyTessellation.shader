Shader "Custom/BodySurface"
{
    Properties
    {
        [Header(Tessellation Settings)]
        [TessellationFactor] _TessellationFactor("Tessellation Factor", Int) = 1
        
        
        [Header(Surface Settings)]
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [Enum(Analytical, 0, ScreenSpace, 1)] _NormalCalculationMethod("Normal Calculation Method", Float) = 1
        _NormalSampleDistance("Normal Sample Distance", Float) = 0.01
        _Metallic("Metallic", Range(0, 1)) = 0.0
        _Smoothness("Smoothness", Range(0, 1)) = 0.1
        _Occlusion("Occlusion", Range(0, 1)) = 1.0
    }

    /*SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "ForwardLit"
            Tags {"LightMode" = "UniversalForward"}
            HLSLPROGRAM
            #pragma target 5.0

            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            

            #pragma shader_feature_local _TESSELLATION_FACTOR_CONSTANT _TESSELLATION_FACTOR_CAMERA _TESSELLATION_FACTOR_SCREEN _TESSELLATION_FACTOR_SPHERE_EDGE
            
            #pragma vertex Vertex
            #pragma hull Hull
            #pragma domain Domain
            #pragma fragment Fragment

            #include "BodyTessellationForwardLitPass.hlsl"
            ENDHLSL
        }
    }*/
}