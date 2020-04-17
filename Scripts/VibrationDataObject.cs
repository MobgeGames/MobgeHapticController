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
                // wait vibrate wait vibrate wait vibrate...
                var amplitudes = new List<int>();
                var durations = new List<long>();
                for (var time = 0.0f; time <= 1.001f; time += _sampleInterval) {
                    var normalizedAmp = _amplitudeCurve.Evaluate(time);
                    if (normalizedAmp < _threshold) {
                        // waits
                        amplitudes.Add( (int) (255 * normalizedAmp));
                        durations.Add( (long) (_sampleInterval * 1000));

                        // vibrates
                        amplitudes.Add(0);
                        durations.Add( 0);
                    } else {
                        // waits
                        amplitudes.Add(0);
                        durations.Add( 0);
                        // vibrates
                        amplitudes.Add( (int) (255 * normalizedAmp));
                        durations.Add( (long) (_sampleInterval * 1000));
                    }
                }

                for (int i = 0; i < amplitudes.Count; i ++) {
                    Debug.Log("ampl: " + amplitudes[i] + "dura: " + durations[i]);
                }
                
                _cachedVibrationPattern = new HapticPattern(durations.ToArray(), amplitudes.ToArray(), -1);
                _isPatternCached = true;
            }
            CustomHaptic(_cachedVibrationPattern);
        }

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
    }
}