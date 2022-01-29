using Coffee.UISoftMask;
using DG.Tweening;
using Sandblast.Models;
using Sandblast.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Sandblast.Interfaces;
using Sandblast.Tutorials;
using System;

namespace Sandblast
{
    [DefaultExecutionOrder(-3)]
    public class Level : MonoBehaviour, ISceneLoadable
    {
        private const float _Duration = 0.4f;
        private const string _TotalPassedLevelsKey = "_passedLevels";

        [SerializeField] private Image _fadeImage = null;
        [SerializeField] private List<LevelInformation> _levels = new List<LevelInformation>();
        [SerializeField] private Transform _instrumentsParent = null;

        [Space]

        [SerializeField] private Material _targetMaterial = null;
        [SerializeField] private MeshFilter _target = null;
        [SerializeField] private RandomRotation _targetPivot = null;
        [SerializeField] private RectTransform _targetPreview = null;
        [SerializeField] private TextMeshProUGUI _completeLevelText = null;
        [SerializeField] private SoftMask _mask = null;

        [Space]

        [SerializeField] private InputPanel _input = null;
        [SerializeField] private FilledColor _filledColor = null;
        [SerializeField] private PaintablesHolder _paintablesHolder = null;
        [SerializeField] private MarkingIslandsTexture _markingIslands = null;
        [SerializeField] private InstrumentSelector _selector = null;

        [Space]

        [SerializeField] private Button _toggleButton = null;
        [SerializeField] private RectTransform _tutorialPreview = null;
        [SerializeField] private RectTransform _arrows = null;
        [SerializeField] private RectTransform _brushArrow = null;

        private readonly Dictionary<int, ITutorial> _tutorials = new Dictionary<int, ITutorial>();
        private LevelInformation _level = null;
        private Image _maskImage = null;
        private int _levelNum = 0;
        private bool _inited = false;
        private DateTime _startTime = DateTime.Today;

        public int TotalLevelsPassed
        {
            get => PlayerPrefs.GetInt(_TotalPassedLevelsKey, 0);
            set => PlayerPrefs.SetInt(_TotalPassedLevelsKey, value);
        }

        private void Awake()
        {
            _maskImage = _mask.GetComponent<Image>();
        }

        private void OnEnable()
        {
            _selector.InstrumentChanged += OnInstrumentChanged;
            _selector.FullCompleted += OnCompleted;
        }

        private void OnDisable()
        {
            _selector.InstrumentChanged -= OnInstrumentChanged;
            _selector.FullCompleted -= OnCompleted;
        }

        public void SceneLoaded()
        {
            StartCoroutine(Init(TotalLevelsPassed % _levels.Count));
        }

        public IEnumerator Init(int levelNum)
        {
            if (_inited)
            {
                yield break;
            }
            _inited = true;

            _levelNum = levelNum;
            _level = _levels[levelNum];

            _target.mesh = _level.Mesh;
            _target.transform.localPosition = _level.Offset;
            _target.transform.localScale = _level.Scale;

            _targetMaterial.SetTexture(Constants.Specular, null);

            _markingIslands.Init(_level.BaseTexture);

            var instruments = new List<Instrument>();
            Brush lastBrush = null;
            for (int i = 0; i < _level.Instruments.Count; i++)
            {
                var instrumentInfo = _level.Instruments[i];
                var uvMask = instrumentInfo.UVMask;

                if (instrumentInfo.Instrument is Brush)
                {
                    if (lastBrush == null)
                    {
                        var instrument = Instantiate(instrumentInfo.Instrument, _instrumentsParent);
                        instrument.Init(i == 0, _input, _level.BaseTexture, uvMask, _markingIslands, _target, _filledColor, _paintablesHolder, instrumentInfo.TargetColor, instrumentInfo.MaxFill);

                        if (!_tutorials.ContainsKey(i) || _tutorials[i] == null)
                        {
                            _tutorials[i] = GetTutorial(instrument);
                        }
                        instruments.Add(instrument);
                        lastBrush = instrument as Brush;
                    }

                    lastBrush.AddStage(instrumentInfo.TargetColor, instrumentInfo.UVMask);
                }
                else
                {
                    if (lastBrush != null)
                    {
                        lastBrush.Init();
                    }

                    var instrument = Instantiate(instrumentInfo.Instrument, _instrumentsParent);
                    instrument.Init(i == 0, _input, _level.BaseTexture, uvMask, _markingIslands, _target, _filledColor, _paintablesHolder, instrumentInfo.TargetColor, instrumentInfo.MaxFill);

                    if (!_tutorials.ContainsKey(i) || _tutorials[i] == null)
                    {
                        _tutorials[i] = GetTutorial(instrument);
                    }
                    instruments.Add(instrument);
                    lastBrush = null;
                }
            }

            if (lastBrush != null)
            {
                lastBrush.Init();
            }

            for (int i = 0; i < 10; i++)
            {
                yield return null;
            }

            _selector.Init(instruments);

            TryStartTutorial(0);

            if (_target.TryGetComponent(out MeshCollider collider))
            {
                collider.sharedMesh = _level.Mesh;
            }

            _targetPivot.transform.DORotate(_level.Rotation, 1.0f);
            Camera.main.transform.DOLocalMove(Vector3.forward * -5, 1.0f).From(Vector3.forward * -7);

            _startTime = DateTime.Now;
            Amplitude.Instance.logEvent("level_start", new Dictionary<string, object>() { { "level", TotalLevelsPassed } });
        }

        public void LoadNextLevel()
        {
            var nextLevelNum = _levelNum + 1;
            if (nextLevelNum >= _levels.Count)
            {
                nextLevelNum = 0;
            }

            Amplitude.Instance.logEvent("level_complete", new Dictionary<string, object>() { { "level", TotalLevelsPassed }, { "time_spent", (int)Math.Floor((DateTime.Now - _startTime).TotalSeconds) } });
            TotalLevelsPassed++;
            StartCoroutine(ReloadLevel(nextLevelNum));
        }

        private IEnumerator ReloadLevel(int levelNum)
        {
            _fadeImage.raycastTarget = true;
            _fadeImage.maskable = true;
            yield return _fadeImage.DOFade(1, 0.5f).SetEase(Ease.OutQuad).WaitForCompletion();

            Camera.main.gameObject.SetActive(false);

            var currentScene = SceneManager.GetActiveScene();
            foreach (var root in currentScene.GetRootGameObjects().Where(x => x.transform != transform))
            {
                root.SetActive(false);
            }

            yield return null;

            var targetScene = SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, new LoadSceneParameters(LoadSceneMode.Additive));
            while (!targetScene.isLoaded)
            {
                yield return null;
            }

            var rootGOs = targetScene.GetRootGameObjects();
            var level = rootGOs.Select(x => x.GetComponent<Level>()).First(x => x != null);
            level.StartCoroutine(level.Init(levelNum));

            foreach (var loadable in rootGOs.Select(x => x.GetComponentInChildren<ISceneLoadable>()).Where(x => x != null))
            {
                loadable.SceneLoaded();
            }

            yield return _fadeImage.DOFade(0, 0.5f).SetEase(Ease.InQuad).WaitForCompletion();
            _fadeImage.raycastTarget = false;
            _fadeImage.maskable = false;

            SceneManager.SetActiveScene(targetScene);
            yield return SceneManager.UnloadSceneAsync(currentScene);
        }

        private void OnCompleted()
        {
            _mask.gameObject.SetActive(true);

            var targetPivotTransform = _targetPivot.transform;
            targetPivotTransform.DOMove(Vector3.Scale(Camera.main.ScreenToWorldPoint(_targetPreview.position + Vector3.forward * 5.0f), new Vector3(1, 1, 0)) + Vector3.Scale(targetPivotTransform.position, Vector3.forward), _Duration);
            targetPivotTransform.DOScale(0.5f, _Duration);
            _mask.rectTransform.DOSizeDelta(Vector2.one * 540, _Duration);

            _completeLevelText.text = _completeLevelText.text.Replace("${}", (TotalLevelsPassed + 1).ToString());

            if (_maskImage != null)
            {
                _maskImage.raycastTarget = true;
            }
            _targetPivot.enabled = true;
        }

        private void TryStartTutorial(int index)
        {
            if (!_tutorials.ContainsKey(index) || _tutorials[index] == null)
            {
                _tutorialPreview.gameObject.SetActive(false);
                _arrows.gameObject.SetActive(false);
                return;
            }

            StartCoroutine(_tutorials[index].StartTutorial());
        }

        private ITutorial GetTutorial(Instrument instrument)
        {
            if (PlayerPrefs.GetInt($"{instrument.GetType().Name}-tutorial", 0) != 0)
            {
                return null;
            }

            ITutorial tutorial = instrument switch
            {
                SandBlaster sandBlaster => new SandBlasterTutorial(sandBlaster, _toggleButton, _tutorialPreview, _arrows),
                PaintCan paintCan => new PaintCanTutorial(paintCan, _toggleButton, _tutorialPreview, _arrows),
                Polish polish => new PolishTutorial(polish, _tutorialPreview, _arrows),
                Foam foam => new SandBlasterTutorial(foam, _toggleButton, _tutorialPreview, _arrows),
                Sponge sponge => new PolishTutorial(sponge, _tutorialPreview, _arrows),
                Brush brush => new BrushTutorial(brush, _tutorialPreview, _arrows, _brushArrow),
                _ => null,
            };

            return tutorial;
        }

        private void OnInstrumentChanged(int index)
        {
            TryStartTutorial(index);
        }
    }
}
