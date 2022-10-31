using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Effect
{
    public class ParticleNode : IDisposable
    {
        public GameObject gameObject;
        public string path;

        private ParticleSystem[] particlesSystem;
        private TrailRenderer[] trailRenderers;
        private Animator[] animators;

        public bool pause { get; private set; }
        bool disposed = false;

        private float during = 0;
        private float totalTime = 0;
        public bool IsPlaying { get; private set; } = false;

        private Action cb = null;
        public int nodeId;
        public SortingGroup sortingGroup;
        public ParticleNode(GameObject node, string path)
        {
            this.path = path;
            particlesSystem = node.GetComponents<ParticleSystem>();
            trailRenderers = node.GetComponents<TrailRenderer>();
            animators = node.GetComponents<Animator>();
            if (sortingGroup == null)
                sortingGroup = node.AddComponent<SortingGroup>();
        }

        public void Pause()
        {
            if (pause) return;
            if (particlesSystem != null && particlesSystem.Length > 0)
            {
                for (int i = 0; i < particlesSystem.Length; i++)
                {
                    particlesSystem[i].Pause();
                }
            }

            if (animators != null && animators.Length > 0)
            {
                for (int i = 0; i < animators.Length; i++)
                {
                    animators[i].speed = 0;
                    animators[i].enabled = false;
                }
            }
        }

        public void Resume()
        {
            if (!pause) return;
            if (particlesSystem != null && particlesSystem.Length > 0)
            {
                ParticleSystem ps = null;
                for (int i = 0; i < particlesSystem.Length; i++)
                {
                    ps = particlesSystem[i];
                    ps.Simulate(0);
                    ps.Play();
                    var main = ps.main;
                    main.simulationSpeed = 1;
                }
            }

            if (animators != null && animators.Length > 0)
            {
                for (int i = 0; i < animators.Length; i++)
                {
                    animators[i].speed = 1;
                    animators[i].enabled = true;
                }
            }
        }

        public void Play(float totalTime = -1)
        {
            this.totalTime = totalTime;
            IsPlaying = true;
            Resume();
            this.gameObject.SetActive(true);
        }

        public void Update(float deltaTime)
        {
            if (totalTime <= 0) return;
            during += deltaTime;
            if (during >= totalTime)
            {
                totalTime = 0;
                during = 0;
                cb?.Invoke();
                cb = null;
                IsPlaying = false;
            }
        }

        public void Recycle()
        {
            if (particlesSystem != null && particlesSystem.Length > 0)
            {
                for (int i = 0; i < particlesSystem.Length; i++)
                {
                    particlesSystem[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
            gameObject.SetActive(false);
            IsPlaying = false;
        }

        public void Dispose()
        {
            cb = null;
            GameObject.Destroy(gameObject);
            gameObject = null;
            particlesSystem = null;
            trailRenderers = null;
        }
    }
}