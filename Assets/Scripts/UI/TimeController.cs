using System;
using ProceduralPlanets.Generation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProceduralPlanets.UI
{
    public class TimeController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timeScaleText;
        [SerializeField] private Button speedUpButton;
        [SerializeField] private Button slowDownButton;

        private const string TimeScaleTextFormat = "Time Scale:\n{0}x";

        public void SpeedUpTime()
        {
            SystemGenerator.SetTimeExponent(SystemGenerator.CurrentTimeExponent + 1);
            UpdateTimeScalePanel();
        }

        public void SlowDownTime()
        {
            SystemGenerator.SetTimeExponent(SystemGenerator.CurrentTimeExponent - 1);
            UpdateTimeScalePanel();
        }

        private void UpdateTimeScalePanel()
        {
            timeScaleText.text = string.Format(TimeScaleTextFormat, SystemGenerator.TimeMultiplier);
            speedUpButton.interactable = SystemGenerator.CurrentTimeExponent + 1 <= SystemGenerator.MaxTimeExponent;
            slowDownButton.interactable = SystemGenerator.CurrentTimeExponent - 1 >= SystemGenerator.MinTimeExponent;
        }
    }
}