#pragma once
#include "FastNoiseLite.hlsl"

struct NoiseSettings
{
    float strength;
    float baseRoughness;
    float persistence;
    float lacunarity;
    float minimumValue;
    int octaves;
    float3 offset;
    int fnlSeed;
    int fnlNoiseType;
};

int _NoiseLayerCount;
StructuredBuffer<NoiseSettings> _NoiseSettings;

float EvaluateSingleNoise(float3 position, NoiseSettings settings)
{
    float noiseValue = 0.0;

    fnl_state noiseState = fnlCreateState();
    noiseState.seed = settings.fnlSeed;
    noiseState.noise_type = settings.fnlNoiseType;

    float frequency = settings.baseRoughness * 100.0;
    float amplitude = 1.0;
    
    for (int i = 0; i < settings.octaves; i++)
    {
        float3 samplePoint = position * frequency + settings.offset;
        float v = fnlGetNoise3D(noiseState, samplePoint.x, samplePoint.y, samplePoint.z);
        noiseValue += (v + 1) * 0.5 * amplitude;

        frequency *= settings.lacunarity;
        amplitude *= settings.persistence;
    }

    noiseValue = max(0, noiseValue - settings.minimumValue);

    return noiseValue * settings.strength;
}

float EvaluateMultilayeredNoise(float3 position)
{
    float noiseValue = 0.0;

    for (int i = 0; i < _NoiseLayerCount; i++)
    {
        NoiseSettings settings = _NoiseSettings[i];
        noiseValue += EvaluateSingleNoise(position, settings);
    }

    return noiseValue;
}
