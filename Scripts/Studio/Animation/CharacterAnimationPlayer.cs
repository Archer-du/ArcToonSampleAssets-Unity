using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace ArcToonSampleAssets.Scripts.Studio.Animation
{
    // Drives a character's animation through a PlayableGraph, replacing the per-character
    // AnimatorController state machine. A two-input mixer blends a continuously looping
    // idle (input 0) against an optional one-shot action (input 1). Playing an action
    // crossfades to it, lets it run once, then crossfades back to the idle automatically.
    //
    // The blend weight is derived purely from the action clip's own timeline, so there is
    // no explicit phase state machine to keep in sync:
    //   weight = clamp01( min(t / crossfade, (length - t) / crossfade) )
    // which fades in over the first `crossfade` seconds and out over the last.
    public class CharacterAnimationPlayer : IDisposable
    {
        private const int IdleInput = 0;
        private const int ActionInput = 1;

        private PlayableGraph graph;
        private AnimationMixerPlayable mixer;

        private AnimationClipPlayable idlePlayable;
        private AnimationClipPlayable actionPlayable;

        private float idleLength;
        private AnimationClip idleClip;
        private AnimationClip actionClip;
        private float actionLength;

        private float crossfade;
        private float speed = 1f;

        public bool IsPlayingAction => actionClip != null;
        public float Speed => speed;

        // The clips currently governing the mixer, for UI readouts. CurrentAction is null
        // whenever no one-shot is playing (before one starts, or after it returns to idle).
        public AnimationClip CurrentAction => actionClip;
        public AnimationClip CurrentIdle => idleClip;

        public CharacterAnimationPlayer(Animator animator, float crossfadeDuration = 0.2f)
        {
            crossfade = Mathf.Max(0.01f, crossfadeDuration);

            graph = PlayableGraph.Create($"StudioAnim_{animator.name}");
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            var output = AnimationPlayableOutput.Create(graph, "Animation", animator);
            mixer = AnimationMixerPlayable.Create(graph, 2);
            output.SetSourcePlayable(mixer);

            mixer.SetInputWeight(IdleInput, 1f);
            mixer.SetInputWeight(ActionInput, 0f);

            graph.Play();
        }

        // Sets the looping idle. Any in-progress action is dropped.
        public void PlayIdle(AnimationClip idle)
        {
            ClearAction();

            if (idlePlayable.IsValid())
            {
                graph.Disconnect(mixer, IdleInput);
                graph.DestroyPlayable(idlePlayable);
            }

            idleClip = idle;
            if (idle == null)
            {
                idleLength = 0f;
                return;
            }

            idlePlayable = AnimationClipPlayable.Create(graph, idle);
            idlePlayable.SetSpeed(speed);
            // Never let the playable finish on its own; looping is handled manually in Tick
            // so it is independent of the clip's import loop-time setting.
            idlePlayable.SetDuration(double.MaxValue);
            graph.Connect(idlePlayable, 0, mixer, IdleInput);
            mixer.SetInputWeight(IdleInput, 1f);
            idleLength = Mathf.Max(0.01f, idle.length);
        }

        // Plays a clip once, then automatically returns to the current idle.
        public void PlayAction(AnimationClip clip)
        {
            if (clip == null) return;

            ClearAction();

            actionClip = clip;
            actionLength = Mathf.Max(0.01f, clip.length);
            actionPlayable = AnimationClipPlayable.Create(graph, clip);
            actionPlayable.SetSpeed(speed);
            actionPlayable.SetTime(0d);
            // Keep advancing past the clip length so Tick can detect completion reliably.
            actionPlayable.SetDuration(double.MaxValue);
            graph.Connect(actionPlayable, 0, mixer, ActionInput);
            mixer.SetInputWeight(ActionInput, 0f);
        }

        public void SetSpeed(float value)
        {
            speed = Mathf.Max(0f, value);
            if (idlePlayable.IsValid()) idlePlayable.SetSpeed(speed);
            if (actionPlayable.IsValid()) actionPlayable.SetSpeed(speed);
        }

        // Advances loop wrapping and action blend. Call once per frame.
        public void Tick()
        {
            if (!graph.IsValid()) return;

            // Keep the idle looping regardless of the clip's import loop-time setting.
            if (idlePlayable.IsValid() && idleLength > 0f)
            {
                double t = idlePlayable.GetTime();
                if (t >= idleLength) idlePlayable.SetTime(t % idleLength);
            }

            if (actionClip == null)
            {
                mixer.SetInputWeight(IdleInput, 1f);
                mixer.SetInputWeight(ActionInput, 0f);
                return;
            }

            float at = (float)actionPlayable.GetTime();
            if (at >= actionLength)
            {
                ClearAction();
                mixer.SetInputWeight(IdleInput, 1f);
                mixer.SetInputWeight(ActionInput, 0f);
                return;
            }

            float weight = Mathf.Clamp01(Mathf.Min(at / crossfade, (actionLength - at) / crossfade));
            mixer.SetInputWeight(ActionInput, weight);
            mixer.SetInputWeight(IdleInput, 1f - weight);
        }

        public void Dispose()
        {
            if (graph.IsValid()) graph.Destroy();
        }

        private void ClearAction()
        {
            if (actionPlayable.IsValid())
            {
                graph.Disconnect(mixer, ActionInput);
                graph.DestroyPlayable(actionPlayable);
            }
            actionClip = null;
            actionLength = 0f;
        }
    }
}
