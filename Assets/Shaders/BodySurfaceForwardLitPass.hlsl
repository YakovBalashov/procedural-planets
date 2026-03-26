#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#include "SurfaceNoise.hlsl"
#include "Normal.hlsl"


#define NORMAL_METHOD_ANALYTIC 0
#define NORMAL_METHOD_SCREENSPACE 1

struct Attributes
{
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
};

struct TessellationControlPoint
{
    float3 positionWS : INTERNALTESSPOS;
    float4 positionCS : SV_POSITION;
    float3 normalWS : NORMAL;
};

struct TessellationFactors
{
    float edges[3] : SV_TessFactor;
    float inside : SV_InsideTessFactor;
};

struct Interpolators
{
    float3 normalWS : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float4 positionCS : SV_POSITION;
};

struct EdgePoints
{
    float3 vertex0PositionWS;
    float3 vertex1PositionWS;
    float4 vertex0PositionCS;
    float4 vertex1PositionCS;
    float3 vertex0NormalWS;
    float3 vertex1NormalWS;
};


EdgePoints MakeEdgePoints(float3 v0WS, float3 v1WS, float4 v0CS, float4 v1CS, float3 v0NormalWS = float3(0, 0, 0),
                          float3 v1NormalWS = float3(0, 0, 0))
{
    EdgePoints e;
    e.vertex0PositionWS = v0WS;
    e.vertex1PositionWS = v1WS;
    e.vertex0PositionCS = v0CS;
    e.vertex1PositionCS = v1CS;
    e.vertex0NormalWS = v0NormalWS;
    e.vertex1NormalWS = v1NormalWS;
    return e;
}

CBUFFER_START(UnityPerMaterial)
    float _TessellationFactor;
    float4 _BaseColor;
    float _Metallic;
    float _Smoothness;
    float _Occlusion;
    float _NormalCalculationMethod;
CBUFFER_END

float _NormalSampleDistance;
float _SilhouetteTessellationScale;
float _SilhouetteThreshold;
float _CameraTessellationScale;
float3 _PlanetCenter;
float _PlanetRadius;

TessellationControlPoint Vertex(Attributes input)
{
    TessellationControlPoint output;

    VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionOS);
    VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

    output.positionWS = posnInputs.positionWS;
    output.positionCS = posnInputs.positionCS;
    output.normalWS = normalInputs.normalWS;
    return output;
}

[domain("tri")]
[outputcontrolpoints(3)]
[outputtopology("triangle_cw")]
[patchconstantfunc("PatchConstantFunction")]
[partitioning("integer")]
TessellationControlPoint Hull(InputPatch<TessellationControlPoint, 3> patch,
                              uint vertexId : SV_OutputControlPointID)
{
    return patch[vertexId];
}

#define BARYCENTRIC_INTERPOLATE(field) \
    patch[0].field * barycentricCoordinates.x + \
    patch[1].field * barycentricCoordinates.y + \
    patch[2].field * barycentricCoordinates.z

float CalculateTessellationFactor(EdgePoints edgePoints)
{
    return _TessellationFactor;
}

TessellationFactors PatchConstantFunction(InputPatch<TessellationControlPoint, 3> patch)
{
    TessellationFactors output;

    output.edges[0] = CalculateTessellationFactor(MakeEdgePoints(
        patch[0].positionWS, patch[1].positionWS,
        patch[0].positionCS, patch[1].positionCS,
        patch[0].normalWS, patch[1].normalWS));
    output.edges[1] = CalculateTessellationFactor(MakeEdgePoints(
        patch[1].positionWS, patch[2].positionWS,
        patch[1].positionCS, patch[2].positionCS,
        patch[1].normalWS, patch[2].normalWS));
    output.edges[2] = CalculateTessellationFactor(MakeEdgePoints(
        patch[2].positionWS, patch[0].positionWS,
        patch[2].positionCS, patch[0].positionCS,
        patch[2].normalWS, patch[0].normalWS));


    output.inside = (output.edges[0] + output.edges[1] + output.edges[2]) / 3.0;
    return output;
}

float3 ApplyDisplacement(float3 positionWS, in fnl_state noiseState)
{
    float3 relativePosition = positionWS - _PlanetCenter;
    float3 directionFromCenter = normalize(relativePosition);

    float elevation = 0.0;

    for (int i = 0; i < _NoiseLayerCount; i++)
    {
        elevation += EvaluateNoise(directionFromCenter, _NoiseSettings[i], noiseState);
    }

    return _PlanetCenter + directionFromCenter * _PlanetRadius * (1.0 + elevation);
}

float3 CalculateNormalWS(float3 positionWS, in fnl_state noiseState)
{
    Normal normalInfo = (Normal)0;
    normalInfo.Initialize(positionWS, _PlanetCenter, _NormalSampleDistance);
    float3 finalNormal = normalInfo.GetNormal(_NoiseSettings, _NoiseLayerCount, _PlanetRadius, noiseState);
    return finalNormal;
}

[domain("tri")]
Interpolators Domain(TessellationFactors factors,
                     const OutputPatch<TessellationControlPoint, 3> patch,
                     float3 barycentricCoordinates : SV_DomainLocation)
{
    Interpolators output;

    float3 positionWS = BARYCENTRIC_INTERPOLATE(positionWS);
    float3 normalWS = BARYCENTRIC_INTERPOLATE(normalWS);
    
    fnl_state noiseState = fnlCreateState();
    noiseState.noise_type = FNL_NOISE_VALUE_CUBIC;
    noiseState.octaves = 1;
    
    
    output.positionWS = ApplyDisplacement(positionWS, noiseState);

    if (_NormalCalculationMethod == NORMAL_METHOD_ANALYTIC)
    {
        normalWS = CalculateNormalWS(positionWS, noiseState);
    }
    output.normalWS = normalWS;
    
    output.positionCS = TransformWorldToHClip(output.positionWS);

    return output;
}

float4 Fragment(Interpolators input) : SV_Target
{

    float3 normalWS = normalize(input.normalWS);

    if (_NormalCalculationMethod == NORMAL_METHOD_SCREENSPACE)
    {
        float3 dpdx = ddx(input.positionWS);
        float3 dpdy = ddy(input.positionWS);

        normalWS = normalize(cross(dpdy, dpdx));
    }
    
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
