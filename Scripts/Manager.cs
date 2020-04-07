﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
#if UNITY_IOS
	using UnityEngine.iOS;
#endif

namespace HapticFeedback {
    public enum HapticTypes { Selection, Success, Warning, Failure, LightImpact, MediumImpact, HeavyImpact, None }

    /// <summary>
    /// This class allows developers to call vibration + haptic feedback on Android and iOS (generically or individually)
    /// Haptic patterns are very similar to the iOS guidelines : 
    /// https://developer.apple.com/ios/human-interface-guidelines/user-interaction/feedback
    /// on iOS haptics are as they are presented by Apple
    /// on Android haptics are merely replicates of iOS ones
    /// 
    /// Here's a brief overview of the patterns on Android (iOS ones are already natively called) :
    /// Combinations:
    ///  +selection : light
    ///  +success   : light / heavy
    ///  +warning   : heavy / medium
    ///  +failure   : medium / medium / heavy / light
    /// Primitives:
    ///  +light 
    ///  +medium 
    ///  +heavy  
    /// </summary>
    public static class Manager {
        #region Interface

        public static long LightDuration = 20;
        public static long MediumDuration = 40;
        public static long HeavyDuration = 80;
        public static int LightAmplitude = 40;
        public static int MediumAmplitude = 120;
        public static int HeavyAmplitude = 255;
        private static int _sdkVersion = -1;
        private static long[] _lightimpactPattern = {0, LightDuration};
        private static int[] _lightimpactPatternAmplitude = {0, LightAmplitude};
        private static long[] _mediumimpactPattern = {0, MediumDuration};
        private static int[] _mediumimpactPatternAmplitude = {0, MediumAmplitude};
        private static long[] _HeavyimpactPattern = {0, HeavyDuration};
        private static int[] _HeavyimpactPatternAmplitude = {0, HeavyAmplitude};
        private static long[] _successPattern = {0, LightDuration, LightDuration, HeavyDuration};
        private static int[] _successPatternAmplitude = {0, LightAmplitude, 0, HeavyAmplitude};
        private static long[] _warningPattern = {0, HeavyDuration, LightDuration, MediumDuration};
        private static int[] _warningPatternAmplitude = {0, HeavyAmplitude, 0, MediumAmplitude};

        private static long[] _failurePattern = {
            0, MediumDuration, LightDuration, MediumDuration, LightDuration, HeavyDuration, LightDuration, LightDuration
        };
        private static int[] _failurePatternAmplitude = {0, MediumAmplitude, 0, MediumAmplitude, 0, HeavyAmplitude, 0, LightAmplitude};

        /// <summary>
        /// Returns true if the current platform is Android, false otherwise.
        /// </summary>
        public static bool IsAndroid {
            get {
#if UNITY_ANDROID && !UNITY_EDITOR
			return true;
#else
            return false;
#endif
            }
        }

        /// <summary>
        /// Returns true if the current platform is iOS, false otherwise
        /// </summary>
        /// <returns><c>true</c>, if O was ied, <c>false</c> otherwise.</returns>
        public static bool IsiOS {
            get {
#if UNITY_IOS && !UNITY_EDITOR
			return true;
#else
            return false;
#endif
            }
        }

        /// <summary>
        /// Triggers a simple vibration
        /// </summary>
        public static void Vibrate() {
            if (IsAndroid)
                AndroidVibrate(MediumDuration);
            else if (IsiOS) 
                iOSTriggerHaptics(HapticTypes.MediumImpact);
        }

        /// <summary>
        /// Triggers a haptic feedback of the specified type
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="defaultToRegularVibrate">Should it default to regular vibrate if appropriate api is no found</param>
        public static void Haptic(HapticTypes type, bool defaultToRegularVibrate = false) {
            if (IsAndroid) {
                switch (type) {
                    case HapticTypes.None:
                        // do nothing
                        break;
                    case HapticTypes.Selection:
                        AndroidVibrate(LightDuration, LightAmplitude);
                        break;
                    case HapticTypes.Success:
                        AndroidVibrate(_successPattern, _successPatternAmplitude, -1);
                        break;
                    case HapticTypes.Warning:
                        AndroidVibrate(_warningPattern, _warningPatternAmplitude, -1);
                        break;
                    case HapticTypes.Failure:
                        AndroidVibrate(_failurePattern, _failurePatternAmplitude, -1);
                        break;
                    case HapticTypes.LightImpact:
                        AndroidVibrate(_lightimpactPattern, _lightimpactPatternAmplitude, -1);
                        break;
                    case HapticTypes.MediumImpact:
                        AndroidVibrate(_mediumimpactPattern, _mediumimpactPatternAmplitude, -1);
                        break;
                    case HapticTypes.HeavyImpact:
                        AndroidVibrate(_HeavyimpactPattern, _HeavyimpactPatternAmplitude, -1);
                        break;
                }
            }
            else if (IsiOS) {
                iOSTriggerHaptics(type, defaultToRegularVibrate);
            }
        }
        #endregion

        #region Android specific code
        // Android Vibration reference can be found at :
        // https://developer.android.com/reference/android/os/Vibrator.html
        // And there starting v26, with support for amplitude :
        // https://developer.android.com/reference/android/os/VibrationEffect.html

#if UNITY_ANDROID && !UNITY_EDITOR
			private static AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			private static AndroidJavaObject CurrentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			private static AndroidJavaObject AndroidVibrator = CurrentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
			private static AndroidJavaClass VibrationEffectClass;
			private static AndroidJavaObject VibrationEffect;
			private static int DefaultAmplitude;
            private static IntPtr AndroidVibrateMethodRawClass = AndroidJNIHelper.GetMethodID(AndroidVibrator.GetRawClass(), "vibrate", "(J)V", false);
            private static jvalue[] AndroidVibrateMethodRawClassParameters = new jvalue[1];
#else
        private static AndroidJavaClass UnityPlayer;
        private static AndroidJavaObject CurrentActivity;
        private static AndroidJavaObject AndroidVibrator = null;
        private static AndroidJavaClass VibrationEffectClass = null;
        private static AndroidJavaObject VibrationEffect;
        private static int DefaultAmplitude;
        private static IntPtr AndroidVibrateMethodRawClass = IntPtr.Zero;
        private static jvalue[] AndroidVibrateMethodRawClassParameters = null;
#endif

        /// <summary>
        /// Requests a default vibration on Android, for the specified duration, in milliseconds
        /// </summary>
        /// <param name="milliseconds">Milliseconds.</param>
        public static void AndroidVibrate(long milliseconds) {
            if (!IsAndroid)
                return;
            AndroidVibrateMethodRawClassParameters[0].j = milliseconds;
            AndroidJNI.CallVoidMethod(AndroidVibrator.GetRawObject(), AndroidVibrateMethodRawClass,
                AndroidVibrateMethodRawClassParameters);
        }

        /// <summary>
        /// Requests a vibration of the specified amplitude and duration. If amplitude is not supported by the device's SDK, a default vibration will be requested
        /// </summary>
        /// <param name="milliseconds">Milliseconds.</param>
        /// <param name="amplitude">Amplitude.</param>
        public static void AndroidVibrate(long milliseconds, int amplitude) {
            if (!IsAndroid)
                return;
            // amplitude is only supported after API26
            if (AndroidSDKVersion() < 26) {
                AndroidVibrate(milliseconds);
            } else {
                VibrationEffectClassInitialization();
                VibrationEffect =
                    VibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot",
                        new object[] {milliseconds, amplitude});
                AndroidVibrator.Call("vibrate", VibrationEffect);
            }
        }

        // Requests a vibration on Android for the specified pattern and optional repeat
        // Straight out of the Android documentation :
        // Pass in an array of ints that are the durations for which to turn on or off the vibrator in milliseconds. 
        // The first value indicates the number of milliseconds to wait before turning the vibrator on. 
        // The next value indicates the number of milliseconds for which to keep the vibrator on before turning it off. 
        // Subsequent values alternate between durations in milliseconds to turn the vibrator off or to turn the vibrator on.
        // repeat:  the index into pattern at which to repeat, or -1 if you don't want to repeat.
        public static void AndroidVibrate(long[] pattern, int repeat) {
            if (!IsAndroid)
                return;
            if (AndroidSDKVersion() < 26) {
                AndroidVibrator.Call("vibrate", pattern, repeat);
            } else {
                VibrationEffectClassInitialization();
                VibrationEffect = VibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform",
                        new object[] {pattern, repeat});
                AndroidVibrator.Call("vibrate", VibrationEffect);
            }
        }

        /// <summary>
        /// Requests a vibration on Android for the specified pattern, amplitude and optional repeat
        /// </summary>
        /// <param name="pattern">Pattern.</param>
        /// <param name="amplitudes">Amplitudes.</param>
        /// <param name="repeat">Repeat.</param>
        public static void AndroidVibrate(long[] pattern, int[] amplitudes, int repeat) {
            if (!IsAndroid)
                return;
            if (AndroidSDKVersion() < 26) {
                AndroidVibrator.Call("vibrate", pattern, repeat);
            } else {
                VibrationEffectClassInitialization();
                VibrationEffect = VibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform",
                    new object[] {pattern, amplitudes, repeat});
                AndroidVibrator.Call("vibrate", VibrationEffect);
            }
        }

        /// <summary>
        /// Stops all Android vibrations that may be active
        /// </summary>
        public static void AndroidCancelVibrations() {
            if (!IsAndroid)
                return;
            AndroidVibrator.Call("cancel");
        }

        /// <summary>
        /// Initializes the VibrationEffectClass if needed.
        /// </summary>
        private static void VibrationEffectClassInitialization() {
            if (VibrationEffectClass == null) {
                VibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
            }
        }

        /// <summary>
        /// Returns the current Android SDK version as an int
        /// </summary>
        /// <returns>The SDK version.</returns>
        public static int AndroidSDKVersion() {
            if (_sdkVersion != -1) 
                return _sdkVersion;
            int apiLevel = int.Parse(SystemInfo.operatingSystem.Substring(SystemInfo.operatingSystem.IndexOf("-") + 1, 3));
            _sdkVersion = apiLevel;
            return apiLevel;
        }
        #endregion

        #region IOS specific code
        // The following will only work if the iOSHapticInterface.m file is in a Plugins folder in your project.
        // It's a pretty straightforward implementation of iOS's UIFeedbackGenerator's methods.
        // You can learn more about them there : https://developer.apple.com/documentation/uikit/uifeedbackgenerator

#if UNITY_IOS && !UNITY_EDITOR
			[DllImport ("__Internal")]
			private static extern void InstantiateFeedbackGenerators();
			[DllImport ("__Internal")]
			private static extern void ReleaseFeedbackGenerators();
			[DllImport ("__Internal")]
			private static extern void SelectionHaptic();
			[DllImport ("__Internal")]
			private static extern void SuccessHaptic();
			[DllImport ("__Internal")]
			private static extern void WarningHaptic();
			[DllImport ("__Internal")]
			private static extern void FailureHaptic();
			[DllImport ("__Internal")]
			private static extern void LightImpactHaptic();
			[DllImport ("__Internal")]
			private static extern void MediumImpactHaptic();
			[DllImport ("__Internal")]
			private static extern void HeavyImpactHaptic();
#else
        private static void InstantiateFeedbackGenerators() {
        }

        private static void ReleaseFeedbackGenerators() {
        }

        private static void SelectionHaptic() {
        }

        private static void SuccessHaptic() {
        }

        private static void WarningHaptic() {
        }

        private static void FailureHaptic() {
        }

        private static void LightImpactHaptic() {
        }

        private static void MediumImpactHaptic() {
        }

        private static void HeavyImpactHaptic() {
        }
#endif
        private static bool _iOsHapticsInitialized;

        /// <summary>
        /// Call this method to initialize the haptics. If you forget to do it, the first time iOSTriggerHaptics is called
        /// it will be initialized. It's better if you do it though.
        /// </summary>
        public static void iOSInitializeHaptics() {
            if (!IsiOS)
                return;
            InstantiateFeedbackGenerators();
            _iOsHapticsInitialized = true;
        }

        /// <summary>
        /// Releases the feedback generators, usually you'll want to call this at OnDisable(); or anytime you know you won't need 
        /// vibrations anymore.
        /// </summary>
        public static void iOSReleaseHaptics() {
            if (!IsiOS)
                return;
            ReleaseFeedbackGenerators();
        }

        /// <summary>
        /// This methods tests the current device generation against a list of devices that don't support haptics,
        /// and returns true if haptics are supported, false otherwise.
        /// </summary>
        /// <returns><c>true</c>, if supported was hapticsed, <c>false</c> otherwise.</returns>
        public static bool HapticsSupported() {
            bool hapticsSupported = false;
#if UNITY_IOS
			DeviceGeneration generation = Device.generation;
			if ((generation == DeviceGeneration.iPhone3G)
			|| (generation == DeviceGeneration.iPhone3GS)
			|| (generation == DeviceGeneration.iPodTouch1Gen)
			|| (generation == DeviceGeneration.iPodTouch2Gen)
			|| (generation == DeviceGeneration.iPodTouch3Gen)
			|| (generation == DeviceGeneration.iPodTouch4Gen)
			|| (generation == DeviceGeneration.iPhone4)
			|| (generation == DeviceGeneration.iPhone4S)
			|| (generation == DeviceGeneration.iPhone5)
			|| (generation == DeviceGeneration.iPhone5C)
			|| (generation == DeviceGeneration.iPhone5S)
			|| (generation == DeviceGeneration.iPhone6)
			|| (generation == DeviceGeneration.iPhone6Plus)
			|| (generation == DeviceGeneration.iPhone6S)
			|| (generation == DeviceGeneration.iPhone6SPlus)
            || (generation == DeviceGeneration.iPhoneSE1Gen)
            || (generation == DeviceGeneration.iPad1Gen)
            || (generation == DeviceGeneration.iPad2Gen)
            || (generation == DeviceGeneration.iPad3Gen)
            || (generation == DeviceGeneration.iPad4Gen)
            || (generation == DeviceGeneration.iPad5Gen)
            || (generation == DeviceGeneration.iPadAir1)
            || (generation == DeviceGeneration.iPadAir2)
            || (generation == DeviceGeneration.iPadMini1Gen)
            || (generation == DeviceGeneration.iPadMini2Gen)
            || (generation == DeviceGeneration.iPadMini3Gen)
            || (generation == DeviceGeneration.iPadMini4Gen)
            || (generation == DeviceGeneration.iPadPro10Inch1Gen)
            || (generation == DeviceGeneration.iPadPro10Inch2Gen)
            || (generation == DeviceGeneration.iPadPro11Inch)
            || (generation == DeviceGeneration.iPadPro1Gen)
            || (generation == DeviceGeneration.iPadPro2Gen)
            || (generation == DeviceGeneration.iPadPro3Gen)
            || (generation == DeviceGeneration.iPadUnknown)
            || (generation == DeviceGeneration.iPodTouch1Gen)
            || (generation == DeviceGeneration.iPodTouch2Gen)
            || (generation == DeviceGeneration.iPodTouch3Gen)
            || (generation == DeviceGeneration.iPodTouch4Gen)
            || (generation == DeviceGeneration.iPodTouch5Gen)
            || (generation == DeviceGeneration.iPodTouch6Gen)
			|| (generation == DeviceGeneration.iPhone6SPlus))
			{
			    hapticsSupported = false;
			}
			else
			{
			    hapticsSupported = true;
			}
#endif
            return hapticsSupported;
        }

        /// <summary>
        /// iOS only : triggers a haptic feedback of the specified type
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="defaultToRegularVibrate"></param>
        public static void iOSTriggerHaptics(HapticTypes type, bool defaultToRegularVibrate = false) {
            if (!IsiOS)
                return;
            if (!_iOsHapticsInitialized) 
                iOSInitializeHaptics();

            // this will trigger a standard vibration on all the iOS devices that don't support haptic feedback
            if (HapticsSupported()) {
                switch (type) {
                    case HapticTypes.Selection:
                        SelectionHaptic(); break;
                    case HapticTypes.Success:
                        SuccessHaptic(); break;
                    case HapticTypes.Warning:
                        WarningHaptic(); break;
                    case HapticTypes.Failure:
                        FailureHaptic(); break;
                    case HapticTypes.LightImpact:
                        LightImpactHaptic(); break;
                    case HapticTypes.MediumImpact:
                        MediumImpactHaptic(); break;
                    case HapticTypes.HeavyImpact:
                        HeavyImpactHaptic(); break;
                }
            }
            else if (defaultToRegularVibrate) {
#if UNITY_IOS
					Handheld.Vibrate();
#endif
            }
        }

        /// <summary>
        /// Returns a string containing iOS SDK informations
        /// </summary>
        /// <returns>The OSSDK version.</returns>
        public static string iOSSDKVersion() {
#if UNITY_IOS && !UNITY_EDITOR
			return Device.systemVersion;
#else
            return null;
#endif
        }
        #endregion
    }
}