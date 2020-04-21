using System;
using System.Globalization;
using System.Text;
using UnityEngine;
using static HapticFeedback.Manager;

namespace HapticFeedback {
    [CreateAssetMenu(menuName = "Mobge/Vibration")]
    public class VibrationDataObject : ScriptableObject {
        public VibrationData data;
    }

    [Serializable]
    public class VibrationData {
        [SerializeField] private AnimationCurve _amplitudeCurve;
        [SerializeField] private float _sampleInterval = 0.1f;
        private HapticPattern _cachedVibrationPattern;
        private bool _isPatternCached;

        public AnimationCurve AmplitudeCurve {
            get => _amplitudeCurve;
            set {
                _isPatternCached = false;
                _amplitudeCurve = value;
            }
        }

        public float SampleInterval {
            get => _sampleInterval;
            set {
                _isPatternCached = false;
                _sampleInterval = value;
            }
        }

        public void Vibrate(bool defaultToRegularVibrate = false) {
            if (!_isPatternCached) {
                _cachedVibrationPattern = Parse.AnimationCurve(_amplitudeCurve, _sampleInterval);
                _isPatternCached = true;
            }

            CustomHaptic(_cachedVibrationPattern, defaultToRegularVibrate);
        }

#if UNITY_EDITOR
        public string ToUrlParameter {
            get {
                var maxTime = _amplitudeCurve.keys[_amplitudeCurve.length - 1].time;
                var amps = new StringBuilder("");
                for (var time = 0.0f; time <= maxTime; time += _sampleInterval) {
                    var value = _amplitudeCurve.Evaluate(time);
                    value = (float) Math.Round(value, 2);
                    var str = value.ToString(CultureInfo.InvariantCulture);
                    if (value > 0)
                        amps.Append(str + ":");
                    else {
                        amps.Append(0 + ":");
                    }
                }

                // Removing unnecessary "," at the end of the amplitude parameter
                amps.Remove(amps.Length - 1, 1);
                return "t=" + _sampleInterval.ToString(CultureInfo.InvariantCulture) + "&" + "a=" + amps;
            }
        }
#endif
    }
}