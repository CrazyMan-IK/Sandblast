using Sandblast.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sandblast
{
    [DefaultExecutionOrder(-9999)]
    public class FirstAwakeInitializer : MonoBehaviour
    {
        private static FirstAwakeInitializer _instance = null;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (var loadable in FindObjectsOfType<MonoBehaviour>().OfType<ISceneLoadable>())
            {
                loadable.SceneLoaded();
            }
        }
    }
}
