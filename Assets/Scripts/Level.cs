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

namespace Sandblast
{
    [DefaultExecutionOrder(-3)]
    public class Level : MonoBehaviour, ISceneLoadable
    {
        private const float _Duration = 0.4f;
        private const string _TotalPassedLevelsKey = "_passedLevels";

        [SerializeField] private Image _fadeImage = null;
        [SerializeField] private List<LevelInformation> _levels = new List<LevelInformation>();

        [Space]

        [SerializeField] private MeshFilter _target = null;
        [SerializeField] private RandomRotation _targetPivot = null;
        [SerializeField] private RectTransform _targetPreview = null;
        [SerializeField] private TextMeshProUGUI _completeLevelText = null;
        [SerializeField] private SoftMask _mask = null;

        [Space]

        [SerializeField] private SandBlaster _sandblast = null;
        [SerializeField] private Brush _brush = null;
        [SerializeField] private Polish _polish = null;
        
        [Space]

        [SerializeField] private MarkingIslandsTexture _markingIslands = null;
        [SerializeField] private InstrumentSelector _selector = null;

        private LevelInformation _level = null;
        private Image _maskImage = null;
        private int _levelNum = 0;
        private bool _inited = false;

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
            _selector.FullCompleted += OnCompleted;
        }

        private void OnDisable()
        {
            _selector.FullCompleted -= OnCompleted;
        }

        public void SceneLoaded()
        {
            Init(0);
        }

        public void Init(int levelNum)
        {
            if (_inited)
            {
                return;
            }
            _inited = true;

            _levelNum = levelNum;
            _level = _levels[levelNum];

            _target.mesh = _level.Mesh;
            _target.transform.localPosition = _level.Offset;
            _target.transform.localScale = _level.Scale;

            _markingIslands.Init();
            _sandblast.Init();
            if (_level.AvailableInstrumentsCount > 1)
            {
                _brush.Init(_level.TargetColor);
            }
            if (_level.AvailableInstrumentsCount > 2)
            {
                _polish.Init(_level.TargetColor);
            }

            _selector.Init(_level.AvailableInstrumentsCount);

            if (_target.TryGetComponent(out MeshCollider collider))
            {
                collider.sharedMesh = _level.Mesh;
            }
        }

        public void LoadNextLevel()
        {
            var nextLevelNum = _levelNum + 1;
            if (nextLevelNum >= _levels.Count)
            {
                nextLevelNum = 0;
            }

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
            level.Init(levelNum);

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
    }
}
