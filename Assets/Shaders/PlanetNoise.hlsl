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
};

int _NoiseLayerCount;
StructuredBuffer<NoiseSettings> _NoiseSettings;

float EvaluateNoise(float3 position, NoiseSettings settings)
{
    fnl_state noiseState = fnlCreateState();
    noiseState.noise_type = FNL_NOISE_OPENSIMPLEX2;
    noiseState.octaves = 1;
    
    float noiseValue = 0.0;

    for (int i = 0; i < settings.octaves; i++)
    {
        float frequency = settings.baseRoughness * 100.0 * pow(settings.lacunarity, i);
        float amplitude = pow(settings.persistence, i);

        float3 samplePoint = position * frequency + settings.offset;
        float v = fnlGetNoise3D(noiseState, samplePoint.x, samplePoint.y, samplePoint.z);
        noiseValue += (v + 1) * 0.5 * amplitude;
    }

    noiseValue = max(0, noiseValue - settings.minimumValue);

    return noiseValue * settings.strength;
}
