Shader "Custom/PlanetSurface"
{
    Properties
    {
        [Header(Tessellation Settings)]
        [TessellationFactor] _TessellationFactor("Tessellation Factor", Int) = 1
        [KeywordEnum(CONSTANT, CAMERA, SPHERE_EDGE)] _TESSELLATION_FACTOR("Tessellation mode", Float) = 0
        _DynamicTessellationScale("Dynamic Tessellation Scale", Float) = 1.0
        _SilhouetteThreshold("Silhouette Threshold", Float) = 0.1
        _SilhouetteTessellationScale("Silhouette Tessellation Scale", Float) = 2.0
        _CameraTessellationScale("Camera Tessellation Scale", Float) = 2.0
        
        
        [Header(Planet Noise Settings)]
        [Header(Color and Texture Settings)]
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white"
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma target 5.0

            #pragma shader_feature_local _TESSELLATION_FACTOR_CONSTANT _TESSELLATION_FACTOR_CAMERA _TESSELLATION_FACTOR_SCREEN _TESSELLATION_FACTOR_SPHERE_EDGE
            
            #pragma vertex Vertex
            #pragma hull Hull
            #pragma domain Domain
            #pragma fragment Fragment

            #include "PlanetSurfaceForwardLitPass.hlsl"
            ENDHLSL
        }
    }
}