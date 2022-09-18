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

        private TextMeshProUGUI currentGritText;
        private TextMeshProUGUI maxGritText;
        public GritComponent source { get; set; }
        
        public void Update()
        {
            this.UpdateGritGauge(Time.deltaTime);
        }

        private void Awake()
        {
            var _text = this.gameObject.GetComponentInChildren<Transform>().Find("Text");
            var _currentGrit = _text.GetComponentInChildren<Transform>().Find("CurrentGrit");
            var _currentText = _currentGrit.GetComponent<TextMeshProUGUI>();
            var _maxGrit = _text.GetComponentInChildren<Transform>().Find("MaxGrit");
            var _maxText = _maxGrit.GetComponent<TextMeshProUGUI>();
            var _gritBar = this.gameObject.GetComponentInChildren<Transform>().Find("Grit");
            gritBar = _gritBar.GetComponent<Image>();
            var _gritBarBG = this.gameObject.GetComponentInChildren<Transform>().Find("Background");
            gritBarBG = _gritBarBG.GetComponent<Image>();
            if (_currentText) currentGritText = _currentText;
            if (_maxText) maxGritText = _maxText;
        }

        private void Start()
        {
            this.UpdateGritGauge(0f);
        }

        private void UpdateGritGauge(float deltaTime)
        {
            if (this.source && gritBar)
            {
                currentGritText.text = ((int)this.source.GetCurrentGrit()).ToString();
                maxGritText.text = ((int)this.source.GetMaxGrit()).ToString();
                gritBar.fillAmount = this.source.GetCurrentGrit() / this.source.GetMaxGrit();
            }
        }
    }
}