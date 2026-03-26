struct Normal
{
    float3 sampleXPlus;
    float3 sampleXMinus;
    float3 sampleYPlus;
    float3 sampleYMinus;

    float3 planetCenter;
    

    void Initialize(float3 positionWS, float3 _PlanetCenter, float _NormalSampleDistance)
    {
        planetCenter = _PlanetCenter;

        float3 relativePosition = positionWS - _PlanetCenter;
        float3 directionFromCenter = normalize(relativePosition);

        float3 randomVector = (abs(directionFromCenter.y) < 0.999) ? float3(0, 1, 0) : float3(1, 0, 0);
    
        float3 tangentXPlus = normalize(cross(directionFromCenter, randomVector));
        float3 tangentYPlus = normalize(cross(directionFromCenter, tangentXPlus));

        sampleXPlus = normalize(positionWS + tangentXPlus * _NormalSampleDistance - _PlanetCenter);
        sampleXMinus = normalize(positionWS - tangentXPlus * _NormalSampleDistance - _PlanetCenter);
        sampleYPlus = normalize(positionWS + tangentYPlus * _NormalSampleDistance - _PlanetCenter);
        sampleYMinus = normalize(positionWS - tangentYPlus * _NormalSampleDistance - _PlanetCenter);
    }

    float3 GetNormal(StructuredBuffer<NoiseSettings> noiseSettings, int noiseLayerCount, float planetRadius, in fnl_state noiseState)
    {
        int sampleNumber = 4;

        float3 samples[4] = { sampleXPlus, sampleXMinus, sampleYPlus, sampleYMinus };
        float elevations[4] = { 0, 0, 0, 0 };
        float3 displacedPositions[4];

        
        for (int layerIndex = 0; layerIndex < noiseLayerCount; layerIndex++)
        {
            for (int sampleIndex = 0; sampleIndex < 4; sampleIndex++)
            {
                elevations[sampleIndex] += EvaluateNoise(samples[sampleIndex], noiseSettings[layerIndex], noiseState);
            }
        }

        for (int s = 0; s < 4; s++)
        {
            displacedPositions[s] = planetCenter + samples[s] * planetRadius * (1.0 + elevations[s]);
        }

        float3 tangentXPlus = displacedPositions[0] - displacedPositions[1];
        float3 tangentYPlus = displacedPositions[2] - displacedPositions[3];

        float3 finalNormal = normalize(cross(tangentXPlus, tangentYPlus));

        return finalNormal;
    }
};