using ArcToonSampleAssets.Scripts.Studio.Config;
using UnityEngine;

namespace ArcToonSampleAssets.Scripts.Studio.Camera
{
    // A self-contained orbit camera. It owns the live spherical parameters (yaw, pitch,
    // radius, height, fov), drives a target UnityEngine.Camera every frame, and blends
    // smoothly toward preset viewpoints. Manual input writes the live values directly and
    // cancels any in-progress preset blend.
    public class OrbitCameraRig
    {
        private readonly UnityEngine.Camera camera;
        private readonly OrbitCameraSettings settings;

        // Live viewpoint, continuously applied to the camera.
        private float yaw;
        private float pitch;
        private float radius;
        private float height;
        private float fov;

        // Focus pivot in world space (character root); the rig orbits pivot + up * height.
        private Transform focus;

        // Preset blend state.
        private bool blending;
        private float blendElapsed;
        private CameraPreset blendFrom;
        private CameraPreset blendTo;

        public OrbitCameraRig(UnityEngine.Camera camera, OrbitCameraSettings settings)
        {
            this.camera = camera;
            this.settings = settings;

            // Seed a valid viewpoint so the rig works even before any preset is applied.
            yaw = 0f;
            pitch = Mathf.Clamp(10f, settings.pitchMin, settings.pitchMax);
            radius = Mathf.Clamp(4f, settings.radiusMin, settings.radiusMax);
            height = Mathf.Clamp(1f, settings.heightMin, settings.heightMax);
            fov = Mathf.Clamp(30f, settings.fovMin, settings.fovMax);
        }

        public OrbitCameraSettings Settings => settings;

        public float Radius => radius;
        public float Height => height;
        public float Fov => fov;
        public float Yaw => yaw;
        public float Pitch => pitch;

        public void SetFocus(Transform focusTarget) => focus = focusTarget;

        // --- Manual controls (free-observe mode). Each cancels preset blending. ---

        public void RotateBy(float deltaYaw, float deltaPitch)
        {
            blending = false;
            yaw += deltaYaw;
            pitch = Mathf.Clamp(pitch + deltaPitch, settings.pitchMin, settings.pitchMax);
        }

        public void Zoom(float delta)
        {
            blending = false;
            radius = Mathf.Clamp(radius + delta, settings.radiusMin, settings.radiusMax);
        }

        public void AdjustHeight(float delta)
        {
            blending = false;
            height = Mathf.Clamp(height + delta, settings.heightMin, settings.heightMax);
        }

        public void AdjustFov(float delta)
        {
            blending = false;
            fov = Mathf.Clamp(fov + delta, settings.fovMin, settings.fovMax);
        }

        // --- Absolute setters (preset editing). Like the manual controls, each cancels any
        // in-progress preset blend and clamps to the configured bounds. ---

        public void SetYaw(float value)
        {
            blending = false;
            yaw = value;
        }

        public void SetPitch(float value)
        {
            blending = false;
            pitch = Mathf.Clamp(value, settings.pitchMin, settings.pitchMax);
        }

        public void SetRadius(float value)
        {
            blending = false;
            radius = Mathf.Clamp(value, settings.radiusMin, settings.radiusMax);
        }

        public void SetHeight(float value)
        {
            blending = false;
            height = Mathf.Clamp(value, settings.heightMin, settings.heightMax);
        }

        public void SetFov(float value)
        {
            blending = false;
            fov = Mathf.Clamp(value, settings.fovMin, settings.fovMax);
        }

        // --- Preset control (state-adjust mode). ---

        // Blends toward a preset over settings.blendDuration; snaps immediately when instant.
        public void ApplyPreset(CameraPreset preset, bool instant)
        {
            if (preset == null) return;

            var target = ClampToBounds(preset);
            if (instant || settings.blendDuration <= 0f)
            {
                yaw = target.yaw;
                pitch = target.pitch;
                radius = target.radius;
                height = target.height;
                fov = target.fov;
                blending = false;
                return;
            }

            blendFrom = CaptureCurrent();
            blendTo = target;
            blendElapsed = 0f;
            blending = true;
        }

        // Snapshot of the current live viewpoint, e.g. to seed free-observe from a preset.
        public CameraPreset CaptureCurrent()
        {
            return new CameraPreset("Current", yaw, pitch, radius, height, fov);
        }

        // Advances any preset blend. Call once per frame before Apply.
        public void Tick(float deltaTime)
        {
            if (!blending) return;

            blendElapsed += deltaTime;
            float t = settings.blendDuration <= 0f ? 1f : Mathf.Clamp01(blendElapsed / settings.blendDuration);
            float s = Mathf.SmoothStep(0f, 1f, t);

            yaw = Mathf.LerpAngle(blendFrom.yaw, blendTo.yaw, s);
            pitch = Mathf.Lerp(blendFrom.pitch, blendTo.pitch, s);
            radius = Mathf.Lerp(blendFrom.radius, blendTo.radius, s);
            height = Mathf.Lerp(blendFrom.height, blendTo.height, s);
            fov = Mathf.Lerp(blendFrom.fov, blendTo.fov, s);

            if (t >= 1f) blending = false;
        }

        // Writes the live viewpoint to the camera. Call from LateUpdate.
        public void Apply()
        {
            if (camera == null) return;

            Vector3 pivot = (focus != null ? focus.position : Vector3.zero) + Vector3.up * height;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 position = pivot - rotation * Vector3.forward * radius;

            camera.transform.SetPositionAndRotation(position, rotation);
            camera.fieldOfView = fov;
        }

        private CameraPreset ClampToBounds(CameraPreset preset)
        {
            return new CameraPreset(
                preset.name,
                preset.yaw,
                Mathf.Clamp(preset.pitch, settings.pitchMin, settings.pitchMax),
                Mathf.Clamp(preset.radius, settings.radiusMin, settings.radiusMax),
                Mathf.Clamp(preset.height, settings.heightMin, settings.heightMax),
                Mathf.Clamp(preset.fov, settings.fovMin, settings.fovMax));
        }
    }
}
