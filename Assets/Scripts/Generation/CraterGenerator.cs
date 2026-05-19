using System.Collections.Generic;
using ProceduralPlanets.Noise;
using UnityEngine;
using ProceduralPlanets.Extensions;

namespace ProceduralPlanets.Generation
{
    public static class CraterGenerator
    {
        public static List<CraterParameters> GenerateCraters(CraterGenerationSettings settings, int seed)
        {
            var random = new System.Random(seed);
            int number = random.Range((int)settings.NumberRange.x, (int)settings.NumberRange.y);

            var craters = new List<CraterParameters>(number);

            for (var i = 0; i < number; i++)
            {
                var newCrater = new CraterParameters
                {
                    Position = random.OnUnitSphere(),
                    Radius = random.Range(settings.RadiusRange.x, settings.RadiusRange.y),
                    Depth = random.Range(settings.DepthRange.x, settings.DepthRange.y),
                    RimWidth = random.Range(settings.RimWidthRange.x, settings.RimWidthRange.y),
                    RimSteepness = random.Range(settings.RimSteepnessRange.x, settings.RimSteepnessRange.y),
                    Strength = random.Range(settings.StrengthRange.x, settings.StrengthRange.y)
                };
                craters.Add(newCrater);
            }

            return craters;
        }
    }
}