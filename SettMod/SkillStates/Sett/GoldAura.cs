using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SettMod.SkillStates
{
    public class SettGoldAura : MonoBehaviour
    {
        private static Material auraMaterial;
        private static Texture2D softTexture;
        private static Texture2D rampTexture;

        public const float brightness = 1f;

        private Light auraLight;
        private float intensity;
        private float bodyHeight;
        private bool stopping;

        public static SettGoldAura Create(CharacterBody body, bool fullGrit)
        {
            if (!body) return null;

            GameObject root = new GameObject("SettGoldAura");
            root.transform.SetParent(body.transform, false);
            root.transform.localPosition = body.transform.InverseTransformPoint(body.footPosition);

            SettGoldAura aura = root.AddComponent<SettGoldAura>();
            aura.Setup(body, fullGrit);
            return aura;
        }

        public void SetIntensity(float value)
        {
            this.intensity = Mathf.Clamp(value, 0f, 2.5f);
        }

        public void Stop()
        {
            if (this.stopping) return;
            this.stopping = true;

            if (this.auraLight) this.auraLight.enabled = false;

            this.transform.SetParent(null, true);
            Destroy(this.gameObject);
        }

        private void Setup(CharacterBody body, bool fullGrit)
        {
            CapsuleCollider capsule = body.GetComponent<CapsuleCollider>();
            this.bodyHeight = capsule ? capsule.height : 3f;

            GameObject lightObject = new GameObject("AuraLight");
            lightObject.transform.SetParent(this.transform, false);
            lightObject.transform.localPosition = Vector3.up * (this.bodyHeight * 0.5f);
            this.auraLight = lightObject.AddComponent<Light>();
            this.auraLight.type = LightType.Point;
            this.auraLight.color = new Color(1f, 0.78f, 0.25f);
            this.auraLight.range = this.bodyHeight * (fullGrit ? 8f : 6f);
            this.auraLight.intensity = 0f;
            this.auraLight.shadows = LightShadows.None;
        }

        private void Update()
        {
            if (this.stopping) return;

            float level = this.intensity;

            if (this.auraLight)
            {
                float flicker = 0.75f + 0.5f * Mathf.PerlinNoise(Time.time * 14f, 0.37f);
                this.auraLight.intensity = 3f * SettGoldAura.brightness * level * flicker;
            }
        }

        internal static Material GetMaterial()
        {
            if (SettGoldAura.auraMaterial) return SettGoldAura.auraMaterial;

            SettGoldAura.EnsureTextures();

            Material material = null;

            GameObject particleSource = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Treebot/SonicBoomEffect.prefab").WaitForCompletion();
            if (particleSource)
            {
                foreach (ParticleSystemRenderer sourceRenderer in particleSource.GetComponentsInChildren<ParticleSystemRenderer>(true))
                {
                    Material sourceMaterial = sourceRenderer.sharedMaterial;
                    if (sourceMaterial && sourceMaterial.shader && sourceMaterial.shader.name.Contains("Cloud"))
                    {
                        material = new Material(sourceMaterial);
                        break;
                    }
                }
            }

            if (!material)
            {
                Shader fallback = Shader.Find("Sprites/Default");
                if (!fallback) fallback = Shader.Find("Legacy Shaders/Particles/Additive");
                if (fallback) material = new Material(fallback);
            }

            if (!material)
            {
                Material template = LegacyResourcesAPI.Load<Material>("Materials/matVagrantEnergized");
                material = template ? new Material(template) : new Material(Shader.Find("Standard"));
            }

            material.mainTexture = SettGoldAura.softTexture;
            SettGoldAura.SetTexture(material, "_MainTex", SettGoldAura.softTexture);
            SettGoldAura.SetTexture(material, "_RemapTex", SettGoldAura.rampTexture);
            SettGoldAura.SetColor(material, "_TintColor", new Color(1f, 0.85f, 0.45f, 1f));
            SettGoldAura.SetColor(material, "_Color", new Color(1f, 0.85f, 0.45f, 1f));
            SettGoldAura.SetFloat(material, "_Boost", 2.5f * SettGoldAura.brightness);
            SettGoldAura.SetFloat(material, "_AlphaBoost", 1.5f * SettGoldAura.brightness);
            SettGoldAura.SetFloat(material, "_SrcBlend", 1f);
            SettGoldAura.SetFloat(material, "_DstBlend", 1f);
            SettGoldAura.SetFloat(material, "_InvFade", 0.5f);
            SettGoldAura.SetFloat(material, "_Cull", 0f);

            SettGoldAura.auraMaterial = material;
            return material;
        }

        private static void EnsureTextures()
        {
            if (!SettGoldAura.softTexture)
            {
                const int size = 64;
                Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.wrapMode = TextureWrapMode.Clamp;
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = (x + 0.5f) / size * 2f - 1f;
                        float dy = (y + 0.5f) / size * 2f - 1f;
                        float distance = Mathf.Sqrt(dx * dx + dy * dy);
                        float alpha = Mathf.Clamp01(1f - distance);
                        alpha = alpha * alpha * (3f - 2f * alpha);
                        texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                }
                texture.Apply();
                SettGoldAura.softTexture = texture;
            }

            if (!SettGoldAura.rampTexture)
            {
                const int width = 256;
                Texture2D ramp = new Texture2D(width, 4, TextureFormat.RGBA32, false);
                ramp.wrapMode = TextureWrapMode.Clamp;
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[]
                    {
                        new GradientColorKey(new Color(0f, 0f, 0f), 0f),
                        new GradientColorKey(new Color(0.9f, 0.35f, 0.02f), 0.3f),
                        new GradientColorKey(new Color(1f, 0.72f, 0.15f), 0.6f),
                        new GradientColorKey(new Color(1f, 0.95f, 0.65f), 0.85f),
                        new GradientColorKey(Color.white, 1f)
                    },
                    new GradientAlphaKey[]
                    {
                        new GradientAlphaKey(0f, 0f),
                        new GradientAlphaKey(1f, 0.3f),
                        new GradientAlphaKey(1f, 1f)
                    });
                for (int x = 0; x < width; x++)
                {
                    Color c = gradient.Evaluate(x / (width - 1f));
                    for (int y = 0; y < 4; y++) ramp.SetPixel(x, y, c);
                }
                ramp.Apply();
                SettGoldAura.rampTexture = ramp;
            }
        }

        private static void SetTexture(Material material, string property, Texture texture)
        {
            if (material.HasProperty(property)) material.SetTexture(property, texture);
        }

        private static void SetColor(Material material, string property, Color color)
        {
            if (material.HasProperty(property)) material.SetColor(property, color);
        }

        private static void SetFloat(Material material, string property, float value)
        {
            if (material.HasProperty(property)) material.SetFloat(property, value);
        }
    }
}
