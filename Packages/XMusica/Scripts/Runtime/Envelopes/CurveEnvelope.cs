using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XMusica.Envelope {
    [CreateAssetMenu(fileName = "New Curve Envelope", menuName = "XMusica/Envelopes/Curve Envelope", order = 226)]
    public class CurveEnvelope : VirtualEnvelope {
        [EnvelopeCurve(false)] public AnimationCurve pressCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [Range(0f, 3f)] public float pressCurveDuration = 0.5f;
        [EnvelopeCurve(true)] public AnimationCurve releaseCurve = AnimationCurve.Linear(0, 1, 1, 0);
        [Range(0f, 5f)] public float releaseCurveDuration = 1f;

        public override float DownAmplitude(float timePassed) {
            if(timePassed < pressCurveDuration) {
                return pressCurve.Evaluate(timePassed / pressCurveDuration);
            }
            return pressCurve[pressCurve.length - 1].value;
        }

        public override float UpAmplitude(float timePassed) {
            if (timePassed < releaseCurveDuration) {
                return releaseCurve.Evaluate(timePassed / releaseCurveDuration) * pressCurve[pressCurve.length - 1].value;
            }
            return -1;
        }

        public float Sustain => pressCurve[pressCurve.length - 1].value;
    }
}
