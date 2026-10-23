using UnityEngine;

namespace SettMod.SkillStates
{
    public class SettHandEnergy : MonoBehaviour
    {
        public const float sizeScale = 1f;
        public const float coreSize = 1.1f;
        public const float gatherRadius = 2.2f;

        private Transform hand;
        private ParticleSystem core;
        private ParticleSystem gather;
        private ParticleSystem burst;
        private float charge;
        private float fade = 1f;
        private bool released;
        private bool stopping;

        public static SettHandEnergy Create(Transform hand)
        {
            if (!hand) return null;

            GameObject root = new GameObject("SettHandEnergy");
            root.transform.position = hand.position;
            SettHandEnergy energy = root.AddComponent<SettHandEnergy>();
            energy.hand = hand;
            energy.Setup();
            return energy;
        }

        public void SetCharge(float value)
        {
            this.charge = Mathf.Clamp01(value);
        }

        public void Release(bool fullGrit)
        {
            if (this.released) return;
            this.released = true;
            if (this.burst) this.burst.Emit(fullGrit ? 70 : 45);
        }

        public void Stop()
        {
            if (this.stopping) return;
            this.stopping = true;
            if (this.core) this.core.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (this.gather) this.gather.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Destroy(this.gameObject, 0.6f);
        }

        private void Setup()
        {
            Material material = SettGoldAura.GetMaterial();

            this.core = this.CreateSystem("Core", material, ParticleSystemRenderMode.Billboard, 120);
            ParticleSystem.MainModule coreMain = this.core.main;
            coreMain.startLifetime = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
            coreMain.startSpeed = 0f;
            coreMain.startSize = 0f;
            coreMain.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            coreMain.startColor = new ParticleSystem.MinMaxGradient(Color.white, new Color(1f, 0.8f, 0.3f));
            ParticleSystem.ShapeModule coreShape = this.core.shape;
            coreShape.enabled = false;

            this.gather = this.CreateSystem("Gather", material, ParticleSystemRenderMode.Stretch, 300);
            float gatherLifetime = 0.22f;
            float radius = SettHandEnergy.gatherRadius * SettHandEnergy.sizeScale;
            ParticleSystem.MainModule gatherMain = this.gather.main;
            gatherMain.startLifetime = gatherLifetime;
            gatherMain.startSpeed = -radius / gatherLifetime;
            gatherMain.startSize = new ParticleSystem.MinMaxCurve(0.08f * SettHandEnergy.sizeScale, 0.16f * SettHandEnergy.sizeScale);
            gatherMain.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.95f, 0.7f), new Color(1f, 0.7f, 0.2f));
            ParticleSystem.ShapeModule gatherShape = this.gather.shape;
            gatherShape.enabled = true;
            gatherShape.shapeType = ParticleSystemShapeType.Sphere;
            gatherShape.radius = radius;
            gatherShape.radiusThickness = 0f;
            ParticleSystemRenderer gatherRenderer = this.gather.GetComponent<ParticleSystemRenderer>();
            gatherRenderer.lengthScale = 3f;
            gatherRenderer.velocityScale = 0.04f;

            this.burst = this.CreateSystem("Burst", material, ParticleSystemRenderMode.Stretch, 200);
            ParticleSystem.MainModule burstMain = this.burst.main;
            burstMain.loop = false;
            burstMain.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            burstMain.startSpeed = new ParticleSystem.MinMaxCurve(12f * SettHandEnergy.sizeScale, 28f * SettHandEnergy.sizeScale);
            burstMain.startSize = new ParticleSystem.MinMaxCurve(0.1f * SettHandEnergy.sizeScale, 0.25f * SettHandEnergy.sizeScale);
            burstMain.startColor = new ParticleSystem.MinMaxGradient(Color.white, new Color(1f, 0.72f, 0.18f));
            burstMain.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.ShapeModule burstShape = this.burst.shape;
            burstShape.enabled = true;
            burstShape.shapeType = ParticleSystemShapeType.Sphere;
            burstShape.radius = 0.3f * SettHandEnergy.sizeScale;
            ParticleSystemRenderer burstRenderer = this.burst.GetComponent<ParticleSystemRenderer>();
            burstRenderer.lengthScale = 2.5f;
            burstRenderer.velocityScale = 0.03f;

            this.core.Play();
            this.gather.Play();
        }

        private ParticleSystem CreateSystem(string name, Material material, ParticleSystemRenderMode mode, int maxParticles)
        {
            GameObject systemObject = new GameObject(name);
            systemObject.transform.SetParent(this.transform, false);

            ParticleSystem system = systemObject.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = system.main;
            main.loop = true;
            main.playOnAwake = false;
            main.duration = 1f;
            main.maxParticles = maxParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Local;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 0f;

            ParticleSystem.ColorOverLifetimeModule color = system.colorOverLifetime;
            color.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(1f, 0.75f, 0.2f), 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.2f),
                    new GradientAlphaKey(0f, 1f)
                });
            color.color = new ParticleSystem.MinMaxGradient(gradient);

            ParticleSystemRenderer renderer = systemObject.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = mode;
            renderer.material = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            return system;
        }

        private void LateUpdate()
        {
            if (this.hand) this.transform.position = this.hand.position;

            if (this.stopping) return;

            if (this.released) this.fade = Mathf.Max(0f, this.fade - Time.deltaTime * 4f);

            float level = this.charge * this.fade;
            float pulse = 1f + 0.12f * Mathf.Sin(Time.time * 30f);

            if (this.core)
            {
                ParticleSystem.MainModule main = this.core.main;
                float size = SettHandEnergy.coreSize * SettHandEnergy.sizeScale * (0.25f + 0.75f * level) * pulse;
                main.startSize = new ParticleSystem.MinMaxCurve(size * 0.8f, size * 1.2f);
                ParticleSystem.EmissionModule emission = this.core.emission;
                emission.rateOverTime = level > 0.01f ? 90f : 0f;
            }

            if (this.gather)
            {
                ParticleSystem.EmissionModule emission = this.gather.emission;
                emission.rateOverTime = this.released ? 0f : 40f + 260f * this.charge;
            }
        }
    }
}
