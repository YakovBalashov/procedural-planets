using System.Collections;
using ProceduralPlanets.Generation;
using TMPro;
using UnityEngine;

namespace ProceduralPlanets.UI
{
    public class MessageText : MonoBehaviour
    {
        public static Color ErrorColor => Color.lightCoral;
        public static Color SuccessColor => Color.lightGreen;
        public static Color InfoColor => Color.lightBlue;
        
        private TextMeshProUGUI _text;
        private bool _isMessageDisplaying;
        
        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            SystemGenerator.MessageText = this;
        }
        
        public void SetMessage(string message, float durationInSeconds, Color color)
        {
            if (_isMessageDisplaying) StopAllCoroutines();
            _text.text = message;
            _text.color = color;
            StartCoroutine(EraseMessageAfterDelay(durationInSeconds));
        }
        
        private IEnumerator EraseMessageAfterDelay(float delay)
        {
            _isMessageDisplaying = true;
            yield return new WaitForSeconds(delay);
            _isMessageDisplaying = false;   
            _text.text = string.Empty;
        }
    }
}
