using RoR2;
using UnityEngine;

namespace SettMod.SkillStates
{
    public class SettGroundIndicator : MonoBehaviour
    {
        public const float rotationSpeed = 25f;
        public const float groundOffset = 0.3f;
        public const float ringSeparation = 0.1f;
        public const float brightness = 0.5f;

        public const int rings = 16;
        public const int segments = 48;
        public const float rebuildInterval = 0.05f;
        public const float probeHeight = 8f;
        public const float probeDepth = 30f;

        private static Texture2D markerTexture;
        private static Texture2D closingTexture;

        private Mesh markerMesh;
        private Mesh closingMesh;
        private Material markerMaterial;
        private Material closingMaterial;
        private Vector3[] baseVertices;
        private Vector3[] liftedVertices;
        private Vector2[] flatPositions;
        private Vector2[] markerUVs;
        private Vector2[] closingUVs;
        private float radius;
        private float closingFraction = 1f;
        private float intensity = 1f;
        private float spin;
        private float rebuildTimer;
        private Vector3 lastCenter;
        private bool hasGeometry;

        public static SettGroundIndicator Create(float radius)
        {
            GameObject root = new GameObject("SettGroundIndicator");
            SettGroundIndicator indicator = root.AddComponent<SettGroundIndicator>();
            indicator.radius = radius;
            indicator.Setup();
            return indicator;
        }

        public void SetGround(Vector3 point, Vector3 normal)
        {
            this.transform.rotation = Quaternion.identity;
            if (!this.hasGeometry || (point - this.lastCenter).sqrMagnitude > 0.0625f || this.rebuildTimer <= 0f)
            {
                this.transform.position = point;
                this.lastCenter = point;
                this.rebuildTimer = SettGroundIndicator.rebuildInterval;
                this.RebuildGeometry();
            }
        }

        public void SetClosing(float fraction)
        {
            this.closingFraction = Mathf.Clamp01(fraction);
        }

        public void SetIntensity(float value)
        {
            this.intensity = Mathf.Max(0f, value);
        }

        private void Setup()
        {
            SettGroundIndicator.EnsureTextures();
            Material baseMaterial = SettGoldAura.GetMaterial();

            this.markerMaterial = SettGroundIndicator.CreateMaterial(baseMaterial, SettGroundIndicator.markerTexture, 3000);
            this.closingMaterial = SettGroundIndicator.CreateMaterial(baseMaterial, SettGroundIndicator.closingTexture, 3001);

            int vertexCount = 1 + SettGroundIndicator.rings * SettGroundIndicator.segments;
            this.baseVertices = new Vector3[vertexCount];
            this.liftedVertices = new Vector3[vertexCount];
            this.flatPositions = new Vector2[vertexCount];
            this.markerUVs = new Vector2[vertexCount];
            this.closingUVs = new Vector2[vertexCount];

            Color[] colors = new Color[vertexCount];
            this.flatPositions[0] = Vector2.zero;
            colors[0] = Color.white;
            for (int ring = 1; ring <= SettGroundIndicator.rings; ring++)
            {
                float ringRadius = this.radius * ring / SettGroundIndicator.rings;
                for (int segment = 0; segment < SettGroundIndicator.segments; segment++)
                {
                    float angle = segment * Mathf.PI * 2f / SettGroundIndicator.segments;
                    int index = SettGroundIndicator.VertexIndex(ring, segment);
                    this.flatPositions[index] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ringRadius;
                    colors[index] = Color.white;
                }
            }

            int[] triangles = SettGroundIndicator.BuildTriangles();
            this.markerMesh = SettGroundIndicator.CreateMesh("SettGroundIndicatorMarker", this.baseVertices, colors, triangles);
            this.closingMesh = SettGroundIndicator.CreateMesh("SettGroundIndicatorClosing", this.liftedVertices, colors, triangles);

            this.CreateLayer("Marker", this.markerMesh, this.markerMaterial);
            this.CreateLayer("Closing", this.closingMesh, this.closingMaterial);
        }

        private static int VertexIndex(int ring, int segment)
        {
            return 1 + (ring - 1) * SettGroundIndicator.segments + segment;
        }

        private static int[] BuildTriangles()
        {
            int segmentsCount = SettGroundIndicator.segments;
            int[] triangles = new int[(segmentsCount + (SettGroundIndicator.rings - 1) * segmentsCount * 2) * 3];
            int t = 0;
            for (int segment = 0; segment < segmentsCount; segment++)
            {
                int next = (segment + 1) % segmentsCount;
                triangles[t++] = 0;
                triangles[t++] = SettGroundIndicator.VertexIndex(1, next);
                triangles[t++] = SettGroundIndicator.VertexIndex(1, segment);
            }
            for (int ring = 1; ring < SettGroundIndicator.rings; ring++)
            {
                for (int segment = 0; segment < segmentsCount; segment++)
                {
                    int next = (segment + 1) % segmentsCount;
                    int a = SettGroundIndicator.VertexIndex(ring, segment);
                    int b = SettGroundIndicator.VertexIndex(ring, next);
                    int c = SettGroundIndicator.VertexIndex(ring + 1, segment);
                    int d = SettGroundIndicator.VertexIndex(ring + 1, next);
                    triangles[t++] = a;
                    triangles[t++] = b;
                    triangles[t++] = c;
                    triangles[t++] = b;
                    triangles[t++] = d;
                    triangles[t++] = c;
                }
            }
            return triangles;
        }

        private static Mesh CreateMesh(string name, Vector3[] vertices, Color[] colors, int[] triangles)
        {
            Mesh mesh = new Mesh();
            mesh.name = name;
            mesh.MarkDynamic();
            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.uv = new Vector2[vertices.Length];
            mesh.triangles = triangles;
            return mesh;
        }

        private void CreateLayer(string name, Mesh mesh, Material material)
        {
            GameObject layer = new GameObject(name);
            layer.transform.SetParent(this.transform, false);
            layer.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = layer.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = material.renderQueue - 3000;
        }

        private void RebuildGeometry()
        {
            Vector3 center = this.transform.position;
            for (int i = 0; i < this.flatPositions.Length; i++)
            {
                Vector2 flat = this.flatPositions[i];
                Vector3 origin = new Vector3(center.x + flat.x, center.y + SettGroundIndicator.probeHeight, center.z + flat.y);
                RaycastHit hit;
                float height = Physics.Raycast(origin, Vector3.down, out hit, SettGroundIndicator.probeDepth, LayerIndex.world.mask) ? hit.point.y - center.y : 0f;
                this.baseVertices[i] = new Vector3(flat.x, height + SettGroundIndicator.groundOffset, flat.y);
                this.liftedVertices[i] = new Vector3(flat.x, height + SettGroundIndicator.groundOffset + SettGroundIndicator.ringSeparation, flat.y);
            }

            this.markerMesh.vertices = this.baseVertices;
            this.markerMesh.RecalculateBounds();
            this.closingMesh.vertices = this.liftedVertices;
            this.closingMesh.RecalculateBounds();
            this.hasGeometry = true;
        }

        private void UpdateUVs()
        {
            float markerAngle = this.spin * Mathf.Deg2Rad;
            float closingAngle = -this.spin * 2f * Mathf.Deg2Rad;
            float markerScale = 1f / (this.radius * 2f);
            float closingScale = 1f / (this.radius * 2f * Mathf.Max(0.01f, this.closingFraction));
            float markerCos = Mathf.Cos(markerAngle), markerSin = Mathf.Sin(markerAngle);
            float closingCos = Mathf.Cos(closingAngle), closingSin = Mathf.Sin(closingAngle);

            for (int i = 0; i < this.flatPositions.Length; i++)
            {
                Vector2 p = this.flatPositions[i];
                this.markerUVs[i] = new Vector2(0.5f + (p.x * markerCos - p.y * markerSin) * markerScale, 0.5f + (p.x * markerSin + p.y * markerCos) * markerScale);
                this.closingUVs[i] = new Vector2(0.5f + (p.x * closingCos - p.y * closingSin) * closingScale, 0.5f + (p.x * closingSin + p.y * closingCos) * closingScale);
            }

            this.markerMesh.uv = this.markerUVs;
            this.closingMesh.uv = this.closingUVs;
        }

        private void Update()
        {
            this.rebuildTimer -= Time.deltaTime;
            this.spin = (this.spin + SettGroundIndicator.rotationSpeed * Time.deltaTime) % 360f;
            if (this.hasGeometry) this.UpdateUVs();

            float pulse = 0.85f + 0.15f * Mathf.Sin(Time.time * 10f);
            SettGroundIndicator.Tint(this.markerMaterial, 0.8f * this.intensity * pulse * SettGroundIndicator.brightness);
            SettGroundIndicator.Tint(this.closingMaterial, (0.6f + 0.4f * (1f - this.closingFraction)) * this.intensity * SettGroundIndicator.brightness);
        }

        private void OnDestroy()
        {
            if (this.markerMaterial) Destroy(this.markerMaterial);
            if (this.closingMaterial) Destroy(this.closingMaterial);
            if (this.markerMesh) Destroy(this.markerMesh);
            if (this.closingMesh) Destroy(this.closingMesh);
        }

        private static Material CreateMaterial(Material baseMaterial, Texture2D texture, int renderQueue)
        {
            Material material = new Material(baseMaterial);
            material.renderQueue = renderQueue;
            if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
            if (material.HasProperty("_Boost")) material.SetFloat("_Boost", 1f);
            if (material.HasProperty("_AlphaBoost")) material.SetFloat("_AlphaBoost", 1f);
            if (material.HasProperty("_Cull")) material.SetFloat("_Cull", 0f);
            material.mainTexture = texture;
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
            if (material.HasProperty("_Cloud1Tex")) material.SetTexture("_Cloud1Tex", Texture2D.whiteTexture);
            if (material.HasProperty("_Cloud2Tex")) material.SetTexture("_Cloud2Tex", Texture2D.whiteTexture);
            if (material.HasProperty("_InvFade")) material.SetFloat("_InvFade", 30f);
            return material;
        }

        private static void Tint(Material material, float alpha)
        {
            if (!material) return;
            Color color = new Color(1f, 0.8f, 0.35f, Mathf.Clamp01(alpha));
            if (material.HasProperty("_TintColor")) material.SetColor("_TintColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }

        private static void EnsureTextures()
        {
            if (!SettGroundIndicator.markerTexture) SettGroundIndicator.markerTexture = SettGroundIndicator.BuildTexture(true);
            if (!SettGroundIndicator.closingTexture) SettGroundIndicator.closingTexture = SettGroundIndicator.BuildTexture(false);
        }

        private static float Band(float r, float inner, float outer, float softness)
        {
            float rise = Mathf.Clamp01((r - inner) / softness);
            float fall = Mathf.Clamp01((outer - r) / softness);
            return Mathf.Min(rise, fall);
        }

        private static Texture2D BuildTexture(bool marker)
        {
            const int size = 256;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f;
                    float dy = (y + 0.5f) / size * 2f - 1f;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha;

                    if (marker)
                    {
                        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg + 180f;
                        float tick = Mathf.Abs(Mathf.Repeat(angle, 30f) - 15f) > 12.5f ? 1f : 0f;

                        float edge = SettGroundIndicator.Band(r, 0.93f, 0.995f, 0.015f);
                        float inner = SettGroundIndicator.Band(r, 0.86f, 0.88f, 0.01f) * 0.6f;
                        float ticks = SettGroundIndicator.Band(r, 0.78f, 0.9f, 0.01f) * tick * 0.8f;
                        float center = SettGroundIndicator.Band(r, 0.3f, 0.33f, 0.01f) * 0.7f;
                        float dot = Mathf.Clamp01((0.06f - r) / 0.02f) * 0.8f;
                        float fill = r < 0.93f ? Mathf.Lerp(0.04f, 0.2f, r / 0.93f) : 0f;

                        alpha = Mathf.Max(edge, Mathf.Max(inner, Mathf.Max(ticks, Mathf.Max(center, Mathf.Max(dot, fill)))));
                    }
                    else
                    {
                        alpha = SettGroundIndicator.Band(r, 0.9f, 0.99f, 0.03f);
                    }

                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return texture;
        }
    }
}
