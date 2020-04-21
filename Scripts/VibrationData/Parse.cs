using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using static HapticFeedback.Manager;

namespace HapticFeedback {
    public static class Parse {
        /// <summary>
        /// Parses VibrationData class into actionable HapticPattern that can be used by custom pattern consumer
        /// </summary>
        /// <param name="curve">Amplitude curve</param>
        /// <param name="sampleInterval"></param>
        /// <param name="threshold">Amplitudes over threshold are considered as vibration.
        /// Under are considered as wait periods</param>
        /// <returns></returns>
        public static HapticPattern AnimationCurve(AnimationCurve curve, float sampleInterval) {
            var normalizedAmplitudes = new List<float>();
            var maxTime = curve.keys[curve.length - 1].time;
            for (var time = 0.0f; time <= maxTime; time += sampleInterval) {
                var a = curve.Evaluate(time);
                a = (float) Math.Round(a, 2);
                normalizedAmplitudes.Add(a);
            }

            var na = normalizedAmplitudes.ToArray();
            return GeneratePattern(in na, sampleInterval);
        }

        /// <summary>
        /// Parses vibration get query (sent by EVibrationObject aka VibrationClient) into actionable
        /// HapticPattern that can be used by custom pattern consumer
        /// </summary>
        /// <param name="request"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static HapticPattern VibrationQuery(HttpListenerRequest request) {
            var sampleInterval = (float) double.Parse(request.QueryString.Get("t"));
            string queryAmps = request.QueryString.Get("a");
            string[] splicedAmps = queryAmps.Split(',');
            var length = splicedAmps.Length;
            var normalizedAmplitudes = new float[length];
            for (var i = 0; i < length; i++) {
                normalizedAmplitudes[i] = (float) double.Parse(splicedAmps[i]);
            }

            return GeneratePattern(in normalizedAmplitudes, sampleInterval);
        }

        /// <summary>
        /// Amplitudes over threshold are considered as vibration. Under are considered as wait periods
        /// </summary>
        /// <param name="normalizedAmplitudes"></param>
        /// <param name="interval"></param>
        /// <param name="threshold"></param>
        /// <returns>Actionable haptic pattern</returns>
        private static HapticPattern GeneratePattern(in float[] normalizedAmplitudes, float interval) {
            var length = normalizedAmplitudes.Length;
            // Android demands amplitudes as integer array, durations as long array
            // ( however amplitudes are actually a byte array, as the maximum number for an amplitude is 255)
            var amplitudes = new int[length * 2];
            var durations = new long[length * 2];
            var j = 0;
            for (var i = 0; i < length; i++) {
                if (normalizedAmplitudes[i] <= 0) {
                    // waits
                    amplitudes[j] = (int) (255 * normalizedAmplitudes[i]);
                    durations[j] = (long) (interval * 1000);

                    // vibrates
                    amplitudes[++j] = 0;
                    durations[j] = 0;
                }
                else {
                    // waits
                    amplitudes[j] = 0;
                    durations[j] = 0;

                    // vibrates
                    amplitudes[++j] = (int) (255 * normalizedAmplitudes[i]);
                    durations[j] = (long) (interval * 1000);
                }

                j++;
            }

// #if DEVELOPMENT_BUILD || UNITY_EDITOR
//             string msg = "";
//             for (int i = 0; i < amplitudes.Length; i++)
//                 msg = msg + i + " ampl: " + amplitudes[i] + "dura: " + durations[i] + "\n";
//             Debug.Log(msg);
// #endif
            return new HapticPattern(durations, amplitudes, -1);
        }
    }
}