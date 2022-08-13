using SettMod.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SettMod.UI
{
    public class GritGauge : MonoBehaviour
    {
        private Image gritBar;
        private Image gritBarBG;

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
            var _gritBarBG = this.gameObject.GetComponentInChildren<Transform>().Find("Background");
            gritBarBG = _gritBarBG.GetComponent<Image>();
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
                    gritBarBG.sprite = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("cp_tensiontex_base00_Eff");
                }
                else if (gritBar.fillAmount < 1)
                {
                    gritBarBG.sprite = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("cp_tensiontex_base00_S3");
                }
            }
        }
    }
}