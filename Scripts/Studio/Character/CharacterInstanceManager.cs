using ArcToonSampleAssets.Scripts.Studio.Animation;
using ArcToonSampleAssets.Scripts.Studio.Config;
using UnityEngine;

namespace ArcToonSampleAssets.Scripts.Studio.Character
{
    // Owns the currently spawned character: instantiates its prefab, takes over its
    // Animator with a Playables-based animation player, and exposes state/action/rotation
    // controls. Switching characters tears down the previous instance and its graph.
    public class CharacterInstanceManager
    {
        private readonly Transform spawnRoot;

        private GameObject instance;
        private CharacterAnimationPlayer animationPlayer;

        public CharacterProfile CurrentProfile { get; private set; }
        public int CurrentStateIndex { get; private set; }

        // Character root transform, used as the camera focus pivot. Null when nothing spawned.
        public Transform FocusRoot => instance != null ? instance.transform : null;

        public float FocusHeight => CurrentProfile != null ? CurrentProfile.focusHeight : 1f;

        public CharacterInstanceManager(Transform spawnRoot)
        {
            this.spawnRoot = spawnRoot;
        }

        // Spawns the given profile's prefab and applies its first state (pose + idle).
        public void SwitchTo(CharacterProfile profile)
        {
            if (profile == null || profile.prefab == null) return;

            Cleanup();

            CurrentProfile = profile;
            instance = Object.Instantiate(profile.prefab, spawnRoot);

            var animator = instance.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                // Hand animation control entirely to the Playables graph.
                animator.runtimeAnimatorController = null;
                animator.applyRootMotion = false;
                animationPlayer = new CharacterAnimationPlayer(animator);
            }

            SetState(0);
        }

        // Switches behavioral state: re-poses the character to this state's spawn-relative
        // transform, then starts looping that state's idle.
        public void SetState(int index)
        {
            if (CurrentProfile == null || animationPlayer == null) return;

            var state = CurrentProfile.GetState(index);
            if (state == null) return;

            CurrentStateIndex = index;

            if (instance != null)
            {
                instance.transform.localPosition = state.localPosition;
                instance.transform.localRotation = Quaternion.Euler(state.localRotationEuler);
            }

            animationPlayer.PlayIdle(state.idle);
        }

        // Plays a one-shot action from the current state; it returns to idle when finished.
        public void PlayAction(int actionIndex)
        {
            if (animationPlayer == null) return;

            var state = CurrentProfile?.GetState(CurrentStateIndex);
            var clip = state?.GetAction(actionIndex);
            if (clip != null) animationPlayer.PlayAction(clip);
        }

        // Rotates the character in place around the world up axis.
        public void RotateCharacter(float deltaYaw)
        {
            if (instance == null) return;
            instance.transform.Rotate(0f, deltaYaw, 0f, Space.World);
        }

        public void SetPaused(bool paused)
        {
            animationPlayer?.SetSpeed(paused ? 0f : 1f);
        }

        public bool IsPaused => animationPlayer != null && animationPlayer.Speed <= 0f;

        // Clips currently on the mixer, for the animation panel header. CurrentAction is null
        // when no one-shot is playing, so callers fall back to CurrentIdle.
        public AnimationClip CurrentAction => animationPlayer?.CurrentAction;
        public AnimationClip CurrentIdle => animationPlayer?.CurrentIdle;

        public void Tick()
        {
            animationPlayer?.Tick();
        }

        public void Cleanup()
        {
            animationPlayer?.Dispose();
            animationPlayer = null;

            if (instance != null)
            {
                Object.Destroy(instance);
                instance = null;
            }

            CurrentProfile = null;
            CurrentStateIndex = 0;
        }
    }
}
