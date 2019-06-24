using System;
using System.Collections;
using UnityEngine;

namespace PlayerHappiness
{
    public interface ICoroutineExecutor : IDisposable
    {
        Coroutine StartCoroutine(IEnumerator enumerator);
    }

    /// <summary>
    /// A helper class for running coroutines from non <see cref="T:UnityEngine.MonoBehaviour" /> classes.
    /// </summary>
    class CoroutineExecutor : ICoroutineExecutor
    {
        internal class CoroutineExecutorMonoBehaviour : MonoBehaviour { }

        private GameObject m_GameObject;
        private CoroutineExecutorMonoBehaviour m_Component;

        public CoroutineExecutor()
        {
            var existingCoroutineExecutor = GameObject.FindObjectOfType<CoroutineExecutorMonoBehaviour>();
            if (existingCoroutineExecutor != null)
            {
                m_Component = existingCoroutineExecutor;
                m_GameObject = existingCoroutineExecutor.gameObject;
            }
            else
            {
                m_GameObject = new GameObject("UnityEngine_RCS_CoroutineExecutorHiddenGameObject")
                {
                    hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector
                };
                m_Component = m_GameObject.AddComponent<CoroutineExecutorMonoBehaviour>();
            }

            GameObject.DontDestroyOnLoad(m_GameObject);
        }

        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return m_Component.StartCoroutine(enumerator);
        }

        public void Dispose()
        {
            GameObject.DestroyImmediate(m_GameObject);
            m_GameObject = null;
            m_Component = null;
        }
    }
}
