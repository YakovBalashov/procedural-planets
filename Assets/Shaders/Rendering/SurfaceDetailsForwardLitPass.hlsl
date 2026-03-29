#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct Attributes
{
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
};

struct Interpolators
{
    float3 normalWS : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float4 positionCS : SV_POSITION;
};


CBUFFER_START(UnityPerMaterial)
    float4 _BaseColor;
    float _Metallic;
    float _Smoothness;
    float _Occlusion;
CBUFFER_END


Interpolators Vertex(Attributes input)
{
    Interpolators output;

    VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionOS);
    VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

    output.positionWS = posnInputs.positionWS;
    output.positionCS = posnInputs.positionCS;
    output.normalWS = normalInputs.normalWS;
    return output;
}

float4 Fragment(Interpolators input) : SV_Target
{

    float3 normalWS = normalize(input.normalWS);
    
    InputData lightingInput = (InputData)0;
    lightingInput.positionWS = input.positionWS;
    lightingInput.normalWS = normalWS;

    lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);

    lightingInput.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
    lightingInput.fogCoord = ComputeFogFactor(input.positionWS);
    lightingInput.vertexLighting = VertexLighting(input.positionWS, input.normalWS);
    lightingInput.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

    lightingInput.shadowMask = half4(1, 1, 1, 1);

    lightingInput.bakedGI = SampleSH(lightingInput.normalWS);

    SurfaceData surface = (SurfaceData)0;
    surface.albedo = _BaseColor.rgb;
    surface.alpha = _BaseColor.a;
    surface.metallic = _Metallic;
    surface.smoothness = _Smoothness;
    surface.occlusion = _Occlusion;

    return UniversalFragmentPBR(lightingInput, surface);
}
