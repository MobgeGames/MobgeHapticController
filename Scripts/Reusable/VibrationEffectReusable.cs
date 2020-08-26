using System;
using System.Collections;
using System.Collections.Generic;
using Mobge;
using UnityEngine;

namespace HapticFeedback {
    public class VibrationEffectReusable : AReusableItem {
        public VibrationDataObject vibrationData;
        public bool loop;
        public bool defaultToRegularVibration;
        
        private float _vibratingUntil = 0f;
        private float _duration = 0f;
        private bool _looping = false;
        
        protected override void OnPlay() {
            Debug.Log("onplay");
            vibrationData.data.Vibrate(defaultToRegularVibration);
            if (loop) {
                var curve = vibrationData.data.AmplitudeCurve;
                _duration = curve.keys[curve.length - 1].time;
                _vibratingUntil = Time.unscaledTime + _duration;
                _looping = true;
            }
        }
        private void Update() {
            if (_looping) {
                if (_vibratingUntil <= Time.unscaledTime) {
                    vibrationData.data.Vibrate(defaultToRegularVibration);
                    _vibratingUntil = Time.unscaledTime + _duration;
                }
            }
        }
        public override bool IsActive => (_vibratingUntil <= Time.unscaledTime) && !_looping;
        public override void StopImmediately() {
            _looping = false;
            Manager.CustomHaptic(new Manager.HapticPattern(new []{0l,20l}, new []{0,0}, -1));
        }
        public override void Stop() {
            _looping = false;
        }
    }
}