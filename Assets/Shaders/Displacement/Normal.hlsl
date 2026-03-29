#pragma once

#include "VertexDisplacement.hlsl"

float NormalSampleDistance;

struct Normal
{
    float3 sampleXPlus;
    float3 sampleXMinus;
    float3 sampleYPlus;
    float3 sampleYMinus;

    
    void Initialize(float3 position)
    {

        float3 directionFromCenter = normalize(position);

        float3 randomVector = (abs(directionFromCenter.y) < 0.999) ? float3(0, 1, 0) : float3(1, 0, 0);
    
        float3 tangentXPlus = normalize(cross(directionFromCenter, randomVector));
        float3 tangentYPlus = normalize(cross(directionFromCenter, tangentXPlus));

        sampleXPlus = normalize(position + tangentXPlus * NormalSampleDistance);
        sampleXMinus = normalize(position - tangentXPlus * NormalSampleDistance);
        sampleYPlus = normalize(position + tangentYPlus * NormalSampleDistance);
        sampleYMinus = normalize(position - tangentYPlus * NormalSampleDistance);
    }

    float3 GetNormal()
    {
        float3 samples[4] = { sampleXPlus, sampleXMinus, sampleYPlus, sampleYMinus };
        float3 displacedPositions[4];
        
        for (int s = 0; s < 4; s++)
        {
            displacedPositions[s] = ApplyVertexDisplacement(samples[s]);
        }

        float3 tangentXPlus = displacedPositions[0] - displacedPositions[1];
        float3 tangentYPlus = displacedPositions[2] - displacedPositions[3];

        float3 finalNormal = normalize(cross(tangentXPlus, tangentYPlus));

        return finalNormal;
    }
};