#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "PlanetNoise.hlsl"

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

float _TessellationFactor;
float _SilhouetteTessellationScale;
float _SilhouetteThreshold;
float _CameraTessellationScale;
float3 _BaseColor;
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
    // float length = distance(edgePoints.vertex0PositionWS, edgePoints.vertex1PositionWS);
    // float distanceToCamera = distance(GetCameraPositionWS(), 
    //                                   (edgePoints.vertex0PositionWS + edgePoints.vertex1PositionWS) * 0.5);
    // return round(length * _DynamicTessellationScale / (distanceToCamera * distanceToCamera));
    #if defined(_TESSELLATION_FACTOR_CONSTANT)
    return _TessellationFactor;
    #elif defined(_TESSELLATION_FACTOR_CAMERA)
    float length = distance(edgePoints.vertex0PositionWS, edgePoints.vertex1PositionWS);
    float distanceToCamera = distance(GetCameraPositionWS(), 
                                      (edgePoints.vertex0PositionWS + edgePoints.vertex1PositionWS) * 0.5);
    return round(length * _CameraTessellationScale / (distanceToCamera * distanceToCamera));
    /*#elif defined(_TESSELLATION_FACTOR_SCREEN)
    return distance(edgePoints.vertex0PositionCS.xyz / edgePoints.vertex0PositionCS.w,
                    edgePoints.vertex1PositionCS.xyz / edgePoints.vertex1PositionCS.w) * _ScreenParams.y *
        _DynamicTessellationScale;*/
    #elif defined(_TESSELLATION_FACTOR_SPHERE_EDGE)
    float3 toCam0 = normalize(_WorldSpaceCameraPos - edgePoints.vertex0PositionWS);
    float3 toCam1 = normalize(_WorldSpaceCameraPos - edgePoints.vertex1PositionWS);

    float d0 = dot(edgePoints.vertex0NormalWS, toCam0);
    float d1 = dot(edgePoints.vertex1NormalWS, toCam1);

    float silhouetteDistance = min(abs(d0), abs(d1));

    return (silhouetteDistance < _SilhouetteThreshold) ? _SilhouetteTessellationScale : 1;
    #endif
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

[domain("tri")]
Interpolators Domain(TessellationFactors factors,
                     const OutputPatch<TessellationControlPoint, 3> patch,
                     float3 barycentricCoordinates : SV_DomainLocation)
{
    Interpolators output;

    float3 positionWS = BARYCENTRIC_INTERPOLATE(positionWS);
    float3 normalWS = BARYCENTRIC_INTERPOLATE(normalWS);

    float3 relativePosition = positionWS - _PlanetCenter;
    float3 directionFromCenter = normalize(relativePosition);

    float elevation = 0.0;

    for (int i = 0; i < _NoiseLayerCount; i++)
    {
        elevation += EvaluateNoise(directionFromCenter, _NoiseSettings[i]);
    }
    
    float3 displacedPositionWS = _PlanetCenter + directionFromCenter * _PlanetRadius * (1.0 + elevation);

    output.positionWS = displacedPositionWS;
    output.normalWS = normalize(normalWS);
    output.positionCS = TransformWorldToHClip(displacedPositionWS);

    return output;
}

float4 Fragment(Interpolators input) : SV_Target
{
    float3 n = normalize(input.normalWS);
    float3 lightDir = normalize(float3(0.5, 1.0, 0.5));
    float NdotL = saturate(dot(n, lightDir));

    float3 color = _BaseColor * NdotL + _BaseColor * 0.1;

    return float4(color, 1.0);
}
