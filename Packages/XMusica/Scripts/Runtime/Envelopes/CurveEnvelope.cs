using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XMusica.Envelope {
    [CreateAssetMenu(fileName = "New Curve Envelope", menuName = "XMusica/Envelopes/Curve Envelope", order = 226)]
    public class CurveEnvelope : VirtualEnvelope {
        [EnvelopeCurve(false)] public AnimationCurve pressCurve;
        [Range(0f, 3f)] public float pressCurveDuration = 0.5f;
        [EnvelopeCurve(true)] public AnimationCurve releaseCurve;
        [Range(0f, 5f)] public float releaseCurveDuration = 1f;

        public override float DownAmplitude(float timePassed) {
            if(timePassed < pressCurveDuration) {
                return pressCurve.Evaluate(timePassed / pressCurveDuration);
            }
            return pressCurve.Evaluate(1);
        }

        public override float UpAmplitude(float timePassed) {
            if (timePassed < releaseCurveDuration) {
                return releaseCurve.Evaluate(timePassed / releaseCurveDuration) * pressCurve.Evaluate(1);
            }
            return -1;
        }
    }
}
