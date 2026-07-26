using System;
using UnityEngine;

namespace ArcToonSampleAssets.Scripts.Studio.Config
{
    // A single camera viewpoint expressed in the orbit rig's own parameter space.
    // Values are stored raw; the rig clamps them into its configured bounds on apply.
    [Serializable]
    public class CameraPreset
    {
        public string name = "Preset";

        [Tooltip("Horizontal orbit angle around the character, in degrees.")]
        public float yaw;

        [Tooltip("Vertical orbit angle, in degrees. Positive looks down at the character.")]
        public float pitch = 10f;

        [Tooltip("Distance from the focus pivot to the camera.")]
        public float radius = 4f;

        [Tooltip("Vertical offset of the focus pivot above the character root.")]
        public float height = 1f;

        [Range(1f, 179f)]
        public float fov = 30f;

        public CameraPreset() { }

        public CameraPreset(string name, float yaw, float pitch, float radius, float height, float fov)
        {
            this.name = name;
            this.yaw = yaw;
            this.pitch = pitch;
            this.radius = radius;
            this.height = height;
            this.fov = fov;
        }

        public CameraPreset Clone()
        {
            return new CameraPreset(name, yaw, pitch, radius, height, fov);
        }
    }
}
