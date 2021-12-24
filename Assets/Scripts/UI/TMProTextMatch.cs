using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Sandblast
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public abstract class TMProTextMatch : MonoBehaviour
    {
        [SerializeField] private string _text = "";
        private TextMeshProUGUI _textField = null;

        private void Awake()
        {
            _textField = GetComponent<TextMeshProUGUI>();
        }

        public void SetValue(string value)
        {
            _textField.text = _text.Replace("${}", value);
        }
    }
}
