namespace ProceduralPlanets
{
    public class MinMax
    {
        public float Min { get; private set; } = float.MaxValue;
        public float Max { get; private set; } = float.MinValue;

        public void Evaluate(float value)
        {
            if (value < Min) Min = value;

            if (value > Max) Max = value;
        }
    }
}
