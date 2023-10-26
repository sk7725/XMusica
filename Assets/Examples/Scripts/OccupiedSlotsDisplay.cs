using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XMusica {
    public class OccupiedSlotsDisplay : MonoBehaviour {
        public InstrumentSource source;
        private Image[] images; 

        void Start() {
            images = new Image[source.SourceCount];
            Image template = transform.GetChild(0).GetComponent<Image>();
            images[0] = template;
            template.gameObject.name = "Source 0";


            for (int i = 1; i < source.SourceCount; i++) {
                images[i] = Instantiate(template, transform);
                images[i].gameObject.name = $"Source {i}";
            }
        }

        void Update() {
            for(int i = 0; i < source.SourceCount; i++) {
                images[i].color = source.CurrentSourceNote[i] == 0 ? Color.white :
                    source.CurrentSourcePressed[i] ? Color.red : Color.blue;
            }
        }
    }
}
