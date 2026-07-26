using System;
using UnityEngine;

namespace ArcToonSampleAssets.Scripts.Studio.Camera
{
    // Tunable bounds and input sensitivities for the orbit camera rig. Bounds are the
    // source of truth for the min/max readouts shown in the free-observe GUI.
    [Serializable]
    public class OrbitCameraSettings
    {
        [Header("Radius (zoom)")]
        public float radiusMin = 1f;
        public float radiusMax = 12f;
        [Tooltip("World units of radius change per unit of scroll-wheel input.")]
        public float zoomSpeed = 10f;

        [Header("Height")]
        public float heightMin = 0f;
        public float heightMax = 3f;
        [Tooltip("World units of height change per second while holding Q/E.")]
        public float heightSpeed = 2f;

        [Header("Field of view")]
        public float fovMin = 10f;
        public float fovMax = 80f;
        [Tooltip("Degrees of FOV change per second while holding the arrow keys.")]
        public float fovSpeed = 30f;

        [Header("Rotation")]
        public float pitchMin = -80f;
        public float pitchMax = 85f;
        [Tooltip("Degrees of camera orbit per unit of mouse movement.")]
        public float rotateSpeed = 3f;

        [Header("Preset blending")]
        [Tooltip("Seconds to blend when switching to a preset viewpoint.")]
        public float blendDuration = 0.35f;
    }
}
