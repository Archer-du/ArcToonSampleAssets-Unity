namespace ArcToonSampleAssets.Scripts.Studio.Modes
{
    // Base class for an operation mode. Each mode owns an independent set of inputs and its
    // own IMGUI panel. New modes (scene editing, character movement, ...) extend this and
    // are registered on the CharacterStudioController.
    public abstract class StudioMode
    {
        protected StudioContext Context;

        public void Bind(StudioContext context) => Context = context;

        // Short label shown on mode-switch controls.
        public abstract string DisplayName { get; }

        // Called when this mode becomes active / inactive.
        public virtual void OnEnter() { }
        public virtual void OnExit() { }

        // Per-frame input and logic while active.
        public virtual void Tick(float deltaTime) { }

        // This mode's IMGUI panel.
        public abstract void OnGUI();
    }
}
