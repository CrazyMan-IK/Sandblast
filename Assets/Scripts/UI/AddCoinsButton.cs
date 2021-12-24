using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Sandblast
{
    [RequireComponent(typeof(Button))]
    public class AddCoinsButton : MonoBehaviour
    {
        [SerializeField] private Level _level = null;
        [SerializeField] private Wallet _wallet = null;
        [SerializeField] private TextMeshProUGUI _text = null;
        [SerializeField] private int _count = 75;

        private Button _button = null;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _text.text = _count.ToString();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            _button.enabled = false;

            StartCoroutine(AddCoins());
        }

        private IEnumerator AddCoins()
        {
            yield return _wallet.Add(transform, _count);

            _level.LoadNextLevel();
        }
    }
}
