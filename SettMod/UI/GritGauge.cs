using RoR2;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore;
using SettMod.Modules;

namespace SettMod.UI
{
    public class GritGauge : MonoBehaviour
    {
        public GritComponent source { get; set; }


        private void Awake()
        {
            gritText = this.gameObject.GetComponentInChildren<Text>();
        }

        private void Start()
        {
            this.UpdateGritGauge(0f);
        }

        public void Update()
        {
            this.UpdateGritGauge(Time.deltaTime);
        }

        private void UpdateGritGauge(float deltaTime)
        {
            if(this.source && gritText)
            {
                string text = "Grit: " + ((int)this.source.GetCurrentGrit()).ToString() + " / " + ((int)this.source.GetMaxGrit()).ToString();
                gritText.text = text;
            }
        }

        private Text gritText;
    }
}
