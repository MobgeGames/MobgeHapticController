using System;
using System.Collections.Generic;
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
        [SerializeField] private float _threshold = 0.1f;
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

        public float Threshold {
            get => _threshold;
            set { 
                _isPatternCached = false;
                _threshold = value;
            } 
        }

        public void Vibrate() {
            if (!_isPatternCached) {
                _cachedVibrationPattern = Parse.AnimationCurve(_amplitudeCurve, _sampleInterval, _threshold);
                _isPatternCached = true;
            }
            CustomHaptic(_cachedVibrationPattern);
        }

#if UNITY_EDITOR
        public string ToUrlParameter {
            get {
                var amps = new StringBuilder("");
                for (var time = 0.0f; time <= 1.001f; time += _sampleInterval) {
                    var value = _amplitudeCurve.Evaluate(time);
                    if(value > _threshold)
                        amps.Append(value + ",");
                    else {
                        amps.Append(0 + ",");
                    }
                }
                
                // Removing unnecessary "," at the end of the amplitude parameter
                amps.Remove(amps.Length - 1, 1);
                return "t=" + _sampleInterval + "&" + "a=" + amps;
            }
        }
#endif
    }
}