using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Sandblast.UI
{
    [DefaultExecutionOrder(1)]
    public class InstrumentSelector : MonoBehaviour
    {
        public event Action<int> InstrumentChanged = null;
        public event Action FullCompleted = null;

        [SerializeField] private Sprite _fullCompletedSprite = null;
        [SerializeField] private ProgressBar _progress = null;
        [SerializeField] private OrbitalMovement _movement = null;
        [SerializeField] private Image _animationOverlay = null;
        [SerializeField] private Image _animationIcon = null;
        [SerializeField] private Button _toggle = null;

        private List<Instrument> _instruments = null;

        private int _currentIndex = -1;
        private int _completedCount = 0;
        private bool _subscribed = false;
        //private Vector3 _startTogglePosition = Vector3.zero;

        private void Start()
        {
            CommonInit();

            //_startTogglePosition = _toggle.transform.position;
        }

        private void Update()
        {
            if (_currentIndex < 0)
            {
                return;
            }

            if (_toggle.transform is RectTransform rectTransform)
            {
                var instrument = _instruments[_currentIndex];

                if (instrument.IsAlwaysActive())
                {
                    rectTransform.localScale = Vector3.zero;
                    return;
                }

                rectTransform.localScale = Vector3.one;

                rectTransform.position = Vector3.Scale(Camera.main.WorldToScreenPoint(instrument.transform.position), Vector3.right + Vector3.up);
                rectTransform.anchoredPosition += instrument.Offset.position;
                rectTransform.sizeDelta = instrument.Offset.size;
            }
        }

        private IEnumerator AsyncStart()
        {
            yield return new WaitForEndOfFrame();

            //Select(0);
        }

        private void OnEnable()
        {
            if (_instruments == null || _subscribed)
            {
                return;
            }

            foreach (var instrument in _instruments)
            {
                instrument.Completed += OnInstrumentCompleted;
            }
            _toggle.onClick.AddListener(OnInstrumentToggle);

            _subscribed = true;
        }

        private void OnDisable()
        {
            foreach (var instrument in _instruments)
            {
                instrument.Completed -= OnInstrumentCompleted;
            }
            _toggle.onClick.RemoveListener(OnInstrumentToggle);

            _subscribed = false;
        }

        public void Init(IEnumerable<Instrument> instruments)
        {
            if (instruments == null)
            {
                throw new ArgumentNullException(nameof(instruments));
            }
            if (instruments.Count() < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(instruments));
            }

            _instruments = instruments.ToList();

            _progress.Init(_instruments);

            OnEnable();

            CommonInit();
        }

        public void Select(int index)
        {
            if (index < 0 || index >= _instruments.Count || (_currentIndex >= 0 && _currentIndex > index))
            {
                return;
            }

            var instrument = _instruments[index];
            //var isEnabled = instrument.gameObject.activeSelf && _currentIndex >= 0;

            for (int i = 0; i < _instruments.Count; i++)
            {
                _instruments[i].Hide();
                if (i != index)
                {
                    _instruments[i].Disable();
                }
            }

            //_toggle.transform.DOLocalMoveY(isEnabled ? 0 : 100, 0.2f);
            /*if (!isEnabled)
            {*/
            instrument.Show();
            if (instrument.IsAlwaysActive())
            {
                instrument.Enable();
            }
            
            _movement.enabled = !(instrument.IsNeedDisablingRotation() && instrument.enabled);
            //}
            _currentIndex = index;
        }

        private void CommonInit()
        {
            if (_currentIndex == -1 && _instruments != null)
            {
                Select(0);

                StartCoroutine(AsyncStart());
            }
        }

        private void OnInstrumentCompleted()
        {
            _instruments[_completedCount].Disable();

            _completedCount++;

            if (_completedCount >= _instruments.Count)
            {
                _instruments[_completedCount - 1].transform.DOLocalMove(Vector3.down * 5, 0.6f).SetRelative();
                FullCompleted?.Invoke();
                return;
            }

            InstrumentChanged?.Invoke(_completedCount);

            _progress.SetCurrentInstrument(_completedCount);

            RunIconAnimation(0.6f);
        }

        private void OnInstrumentToggle()
        {
            var instrument = _instruments[_completedCount];

            if (instrument.IsAlwaysActive())
            {
                return;
            }

            if (instrument.enabled)
            {
                instrument.Disable();
            }
            else
            {
                instrument.Enable();
            }

            _movement.enabled = !(instrument.IsNeedDisablingRotation() && instrument.enabled);

            //Select(_completedCount);
        }

        private void RunIconAnimation(float delay)
        {
            var animation = DOTween.Sequence().OnComplete(() =>
            {
                _animationOverlay.gameObject.SetActive(false);
            });

            var prevStage = _progress.GetStage(_completedCount - 1);
            var nextStage = _progress.GetStage(_completedCount);

            _animationIcon.sprite = prevStage.sprite;
            _animationOverlay.gameObject.SetActive(true);
            _animationIcon.rectTransform.position = prevStage.rectTransform.position;

            animation.Append(_animationIcon.rectTransform.DOAnchorPos(Vector2.zero, delay).SetEase(Ease.InOutQuart));
            animation.Join(_animationIcon.rectTransform.DOScale(4, delay));
            animation.Join(_animationOverlay.DOFade(0.5f, delay));
            animation.Join(_instruments[_completedCount - 1].transform.DOLocalMove(Vector3.down * 5, delay).SetRelative());

            animation.Append(_animationIcon.rectTransform.DORotate(Vector3.up * 180, delay / 2, RotateMode.LocalAxisAdd).SetEase(Ease.InCubic).OnComplete(() =>
            {
                _animationIcon.sprite = nextStage.sprite;
                _instruments[_completedCount].transform.position += Vector3.down * 5;
                Select(_completedCount);
            }));
            animation.Append(_animationIcon.rectTransform.DORotate(Vector3.up * 180, delay / 2, RotateMode.LocalAxisAdd).SetEase(Ease.OutCubic));

            animation.Append(_animationIcon.rectTransform.DOMove(nextStage.rectTransform.position, delay).SetEase(Ease.InOutQuart));
            animation.Join(_animationIcon.rectTransform.DOScale(1, delay));
            animation.Join(_animationOverlay.DOFade(0, delay));
            animation.Join(_instruments[_completedCount].transform.DOLocalMove(Vector3.up * 5, delay).SetRelative());
        }
    }
}
