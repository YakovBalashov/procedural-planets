using System.Collections.Generic;
using ProceduralPlanets.Noise;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    public static class CraterGenerator
    {
        public static List<CraterParameters> GenerateCraters(CraterGenerationSettings settings, int seed)
        {
            Random.InitState(0);
            int number = Random.Range((int)settings.NumberRange.x, (int)settings.NumberRange.y);

            var craters = new List<CraterParameters>(number);

            for (var i = 0; i < number; i++)
            {
                var newCrater = new CraterParameters
                {
                    Position = Random.onUnitSphere,
                    Radius = Random.Range(settings.RadiusRange.x, settings.RadiusRange.y),
                    Depth = Random.Range(settings.DepthRange.x, settings.DepthRange.y),
                    RimWidth = Random.Range(settings.RimWidthRange.x, settings.RimWidthRange.y),
                    RimSteepness = Random.Range(settings.RimSteepnessRange.x, settings.RimSteepnessRange.y),
                    Strength = Random.Range(settings.StrengthRange.x, settings.StrengthRange.y)
                };
                craters.Add(newCrater);
            }

            return craters;
        }
    }
}