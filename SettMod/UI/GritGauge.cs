using SettMod.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SettMod.UI
{
    public class GritGauge : MonoBehaviour
    {
        private Image gritBar;
        private TextMeshProUGUI gritText;
        public GritComponent source { get; set; }

        public void Update()
        {
            this.UpdateGritGauge(Time.deltaTime);
        }

        private void Awake()
        {
            gritText = this.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            var _gritBar = this.gameObject.GetComponentInChildren<Transform>().Find("Grit");
            gritBar = _gritBar.GetComponent<Image>();
        }

        private void Start()
        {
            this.UpdateGritGauge(0f);
        }

        private void UpdateGritGauge(float deltaTime)
        {
            if (this.source && gritBar)
            {
                string text = ((int)this.source.GetCurrentGrit()).ToString() + " / " + ((int)this.source.GetMaxGrit()).ToString();
                gritText.text = text;
                gritBar.fillAmount = this.source.GetCurrentGrit() / this.source.GetMaxGrit();
                if (gritBar.fillAmount >= 1)
                {
                    gritBar.color = new Color(255, 167, 0, 255);
                }
                else if (gritBar.fillAmount < 1)
                {
                    gritBar.color = new Color(255, 255, 255, 255);
                }
            }
        }
    }
}