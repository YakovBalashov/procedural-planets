#pragma once

#include "Assets/Shaders/Displacement/FastNoiseLite.hlsl"
#include "Assets/Shaders/Displacement/SurfaceNoise.hlsl"

#define FEATURE_MAIN_NOISE (1 << 0)
#define FEATURE_EDGE_NOISE (1 << 1)
#define FEATURE_HEIGHT_RANGE (1 << 2)
#define FEATURE_STEEPNESS (1 << 3)
#define FEATURE_POLE_ANGLE (1 << 4)
#define FEATURE_STRIPES (1 << 5)
#define FEATURE_EMISSION (1 << 6)

static const float MIN_BIOME_BLEND = 0.001;

static const float INITIAL_BIOME_BLEND = -1.0;


struct BiomeParameters
{
    uint featureMask;
    float3 color;
    float3 accentColor;
    int noiseType;
    int warpType;
    float warpAmplitude;
    float frequency;
    int octaves;
    float3 offset;
    float maskThreshold;
    float blendFactor;
    float accentThreshold;
    float accentBlendFactor;
    float2 heightRange;
    float2 steepnessRange;
    float poleAngle;
    float3 poleDirection;
    float3 stripesAxis;
    float stripesScale;
    float emissionIntensity;
};

StructuredBuffer<BiomeParameters> _Biomes;
int _BiomeCount;
float4 _BaseColor;

float _LowestVertexHeight;
float _HighestVertexHeight;

float InverseLerp(float lowerBound, float upperBound, float value)
{
    return (value - lowerBound) / (upperBound - lowerBound);
}

float2 GetBaseBlend(float3 position, BiomeParameters biome)
{
    fnl_state state = fnlCreateState();
    state.noise_type = biome.noiseType;
    state.domain_warp_type = biome.warpType;
    state.domain_warp_amp = biome.warpAmplitude;
    state.frequency = biome.frequency / _PlanetRadius;
    state.fractal_type = FNL_FRACTAL_FBM;
    state.octaves = biome.octaves;

    position += biome.offset;
    
    if (biome.warpAmplitude > 0) fnlDomainWarp3D(state, position.x, position.y, position.z);

    float noiseValue = fnlGetNoise3D(state, position.x, position.y, position.z);

    if (noiseValue < biome.maskThreshold - biome.blendFactor) return 0.0;
    float baseBlend = smoothstep(biome.maskThreshold - biome.blendFactor, biome.maskThreshold, noiseValue);
    float accentBlend = smoothstep(biome.accentThreshold - biome.accentBlendFactor, biome.accentThreshold, noiseValue);
    return float2(baseBlend, accentBlend);
}

float GetHeightBlend(float3 position, BiomeParameters biome)
{
    float2 range = biome.heightRange;

    float height = InverseLerp(_LowestVertexHeight, _HighestVertexHeight, length(position));

    float bottomFade = smoothstep(range.x - biome.blendFactor, range.x, height);
    float topFade = 1.0 - smoothstep(range.y, range.y + biome.blendFactor, height);

    return bottomFade * topFade;
}

float GetSteepnessBlend(float3 position, float3 normal, BiomeParameters biome)
{
    float3 upDirection = normalize(position);
    float steepness = 1.0 - dot(normal, upDirection);

    float2 range = biome.steepnessRange;

    float bottomFade = smoothstep(range.x - biome.blendFactor, range.x, steepness);
    float topFade = 1.0 - smoothstep(range.y, range.y + biome.blendFactor, steepness);

    return bottomFade * topFade;
}

float GetPoleAngleBlend(float3 position, BiomeParameters biome)
{
    float3 poleDirection = normalize(biome.poleDirection);
    float3 vertexDirection = normalize(position);

    float latitude = max(0, dot(vertexDirection, poleDirection));

    return smoothstep(biome.poleAngle - biome.blendFactor, biome.poleAngle, latitude);
}

float3 ScalePosition(float3 position, float3 axis, float scale)
{
    axis = normalize(axis);

    return position + axis * dot(position, axis) * (scale - 1.0);
}

float2 GetBiomeBlend(float3 position, float3 normal, BiomeParameters biome)
{
    float maxBlend = INITIAL_BIOME_BLEND;

    if ((biome.featureMask & FEATURE_HEIGHT_RANGE) != 0)
    {
        float heightBlend = GetHeightBlend(position, biome);
        if (heightBlend < MIN_BIOME_BLEND) return 0.0;
        maxBlend = max(maxBlend, heightBlend);
    }

    if ((biome.featureMask & FEATURE_STEEPNESS) != 0)
    {
        float steepnessBlend = GetSteepnessBlend(position, normal, biome);
        if (steepnessBlend < MIN_BIOME_BLEND) return 0.0;
        maxBlend = max(maxBlend, steepnessBlend);
    }

    if ((biome.featureMask & FEATURE_POLE_ANGLE) != 0)
    {
        float poleBlend = GetPoleAngleBlend(position, biome);
        if (poleBlend < MIN_BIOME_BLEND) return 0.0;
        maxBlend = max(maxBlend, poleBlend);
    }

    float3 scaledPosition = (biome.featureMask & FEATURE_STRIPES) != 0
                                ? ScalePosition(position, biome.stripesAxis, biome.stripesScale)
                                : position;
    float2 biomeBlend = (biome.featureMask & FEATURE_MAIN_NOISE) != 0 ? GetBaseBlend(scaledPosition, biome) : float2(1.0, 0.0);
    if (biomeBlend.x < MIN_BIOME_BLEND) return float2(0.0, 0.0);

    if (maxBlend == INITIAL_BIOME_BLEND) return biomeBlend;

    return float2(min(biomeBlend.x, maxBlend), biomeBlend.y);
    
}

void CalculateColor_float(float3 position, float3 normal, out float3 color, out float emissionIntensity)
{
    color = _BaseColor.rgb;
    emissionIntensity = 0.0;

    for (int i = 0; i < _BiomeCount; ++i)
    {
        float2 biomeBlend = GetBiomeBlend(position, normal, _Biomes[i]);
        float3 biomeColor = lerp(_Biomes[i].color, _Biomes[i].accentColor, biomeBlend.y);
        color = lerp(color, biomeColor, biomeBlend.x);

        float biomeEmission = (_Biomes[i].featureMask & FEATURE_EMISSION) != 0 ? _Biomes[i].emissionIntensity : 0.0;
        emissionIntensity = lerp(emissionIntensity, biomeEmission, biomeBlend.x);
    }
}
