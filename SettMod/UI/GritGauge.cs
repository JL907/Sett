using RoR2.UI;
using SettMod.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SettMod.UI
{
    public class GritGauge : MonoBehaviour
    {
        public static readonly Color gritColor = new Color(0.96f, 0.74f, 0.22f, 1f);
        public static readonly Color trailColor = new Color(1f, 0.93f, 0.75f, 0.85f);
        public const float trailDelay = 0.4f;
        public const float trailSpeed = 0.8f;
        public const float spacing = 4f;
        public const float horizontalOffset = -3f;

        private Image fill;
        private Image trail;
        private TextMeshProUGUI currentText;
        private TextMeshProUGUI maxText;
        private RectTransform rect;
        private RectTransform templateContainer;
        private RectTransform container;
        private SpriteAsNumberManager numbers;
        private float trailValue;
        private float trailTimer;
        private int lastCurrent = -1;
        private int lastMax = -1;

        public GritComponent source { get; set; }

        public static GritGauge Create(HUD hud)
        {
            if (!hud || !hud.healthBar) return null;

            HealthBar template = hud.healthBar;
            Transform parent = template.transform.parent;

            Sprite sprite = null;
            Image.Type imageType = Image.Type.Sliced;
            if (template.style)
            {
                sprite = template.style.trailingOverHealthBarStyle.sprite;
                imageType = template.style.trailingOverHealthBarStyle.imageType;
            }

            GameObject holder = new GameObject("SettGritGaugeHolder");
            holder.SetActive(false);

            GameObject clone = Object.Instantiate(template.gameObject, holder.transform, false);
            clone.name = "SettGritBar";

            HealthBar cloneBar = clone.GetComponent<HealthBar>();
            RectTransform container = cloneBar && cloneBar.barContainer ? cloneBar.barContainer : clone.GetComponent<RectTransform>();
            TextMeshProUGUI current = cloneBar ? cloneBar.currentHealthText : null;
            TextMeshProUGUI max = cloneBar ? cloneBar.fullHealthText : null;
            SpriteAsNumberManager numbers = clone.GetComponentInChildren<SpriteAsNumberManager>(true);
            if (cloneBar)
            {
                foreach (string fieldName in new string[] { "criticallyHurtImage", "deadImage", "eliteBackdropRectTransform" })
                {
                    System.Reflection.FieldInfo field = typeof(HealthBar).GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    Component component = field != null ? field.GetValue(cloneBar) as Component : null;
                    if (component) component.gameObject.SetActive(false);
                }
                Object.DestroyImmediate(cloneBar);
            }

            foreach (Transform child in clone.GetComponentsInChildren<Transform>(true))
            {
                if (child == clone.transform) continue;
                if (child.name == "SharedSufferingRoot") child.gameObject.SetActive(false);
            }

            if (container)
            {
                foreach (Transform child in container) child.gameObject.SetActive(false);
            }

            if (numbers)
            {
                numbers.gameObject.SetActive(true);
                foreach (TextMeshProUGUI text in new TextMeshProUGUI[] { current, max })
                {
                    if (text) text.gameObject.SetActive(false);
                }
                foreach (TextMeshProUGUI text in clone.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    if (text.name == "Slash") text.gameObject.SetActive(false);
                }
            }
            else
            {
                foreach (TextMeshProUGUI text in new TextMeshProUGUI[] { current, max })
                {
                    if (!text) continue;
                    text.gameObject.SetActive(true);
                    text.enabled = true;
                    text.alpha = 1f;
                }
            }

            RectTransform cloneRect = clone.GetComponent<RectTransform>();
            clone.transform.SetParent(template.transform, false);
            LayoutElement layoutElement = clone.GetComponent<LayoutElement>();
            if (!layoutElement) layoutElement = clone.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
            if (cloneRect)
            {
                cloneRect.anchorMin = Vector2.zero;
                cloneRect.anchorMax = Vector2.one;
                cloneRect.pivot = new Vector2(0.5f, 0.5f);
                cloneRect.offsetMin = Vector2.zero;
                cloneRect.offsetMax = Vector2.zero;
                cloneRect.localRotation = Quaternion.identity;
                cloneRect.localScale = Vector3.one;
            }
            Object.Destroy(holder);

            GritGauge gauge = clone.AddComponent<GritGauge>();
            gauge.trail = GritGauge.CreateBar(container, "GritTrail", sprite, imageType, GritGauge.trailColor);
            gauge.fill = GritGauge.CreateBar(container, "GritFill", sprite, imageType, GritGauge.gritColor);
            gauge.currentText = current;
            gauge.maxText = max;
            gauge.rect = cloneRect;
            gauge.templateContainer = template.barContainer;
            gauge.container = container;
            gauge.numbers = numbers;

            clone.SetActive(false);
            return gauge;
        }

        private static Image CreateBar(RectTransform container, string name, Sprite sprite, Image.Type imageType, Color color)
        {
            GameObject bar = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = bar.GetComponent<RectTransform>();
            rect.SetParent(container, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0f, 0.5f);

            Image image = bar.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = (int)Image.OriginHorizontal.Left;
            image.fillAmount = 0f;
            image.color = color;
            image.raycastTarget = false;

            return image;
        }

        private static void SetFill(Image bar, float fraction)
        {
            if (!bar) return;
            fraction = Mathf.Clamp01(fraction);
            bar.fillAmount = fraction;
            bar.enabled = fraction > 0.001f;
        }

        private void LateUpdate()
        {
            if (!this.rect) return;
            float height = this.templateContainer ? this.templateContainer.rect.height : 0f;
            if (height <= 0f) height = 24f;
            this.rect.anchoredPosition = new Vector2(GritGauge.horizontalOffset, -(height + GritGauge.spacing));

            if (this.templateContainer && this.container && this.container != this.rect)
            {
                this.container.anchorMin = this.templateContainer.anchorMin;
                this.container.anchorMax = this.templateContainer.anchorMax;
                this.container.pivot = this.templateContainer.pivot;
                this.container.anchoredPosition = this.templateContainer.anchoredPosition;
                this.container.sizeDelta = this.templateContainer.sizeDelta;
            }
        }

        private void Update()
        {
            if (!this.source) return;

            float current = this.source.GetCurrentGrit();
            float max = this.source.GetMaxGrit();
            float fraction = max > 0f ? Mathf.Clamp01(current / max) : 0f;

            if (fraction >= this.trailValue)
            {
                this.trailValue = fraction;
                this.trailTimer = GritGauge.trailDelay;
            }
            else if (this.trailTimer > 0f)
            {
                this.trailTimer -= Time.deltaTime;
            }
            else
            {
                this.trailValue = Mathf.MoveTowards(this.trailValue, fraction, GritGauge.trailSpeed * Time.deltaTime);
            }

            GritGauge.SetFill(this.fill, fraction);
            GritGauge.SetFill(this.trail, this.trailValue);

            int currentInt = (int)current;
            int maxInt = (int)max;
            if (this.numbers)
            {
                if (currentInt != this.lastCurrent || maxInt != this.lastMax)
                {
                    this.lastCurrent = currentInt;
                    this.lastMax = maxInt;
                    this.numbers.SetHitPointValues(currentInt, maxInt);
                }
                return;
            }
            if (this.currentText && currentInt != this.lastCurrent)
            {
                this.lastCurrent = currentInt;
                this.currentText.text = currentInt.ToString();
            }
            if (this.maxText && maxInt != this.lastMax)
            {
                this.lastMax = maxInt;
                this.maxText.text = maxInt.ToString();
            }
        }
    }
}
