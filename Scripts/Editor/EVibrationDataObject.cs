using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Mobge;
using UnityEditor;
using UnityEngine;

namespace HapticFeedback {
    [CustomEditor(typeof(VibrationDataObject))]
    // AKA VibrationClient
    public class EVibrationDataObject : Editor {
        private VibrationDataObject _vibration;
        private string _helpString;

        private readonly GUIContent _sampleIntervalGuiContent = new GUIContent("Sample Interval", "At which interval the amplitude curve will be rasterized for?");

        private readonly GUIContent _thresholdGuiContent = new GUIContent("Threshold", "At which threshold the amplitude height(Y coordinate) will be considered good for vibration?");
        
        private static ushort Port {
            get => (ushort) EditorPrefs.GetInt("Mobge.HapticFeedback::Port");
            set => EditorPrefs.SetInt("Mobge.HapticFeedback::Port", value);
        }

        private static string IpAddress {
            get => EditorPrefs.GetString("Mobge.HapticFeedback::IpAddress");
            set => EditorPrefs.SetString("Mobge.HapticFeedback::IpAddress", value);
        }

        private void OnEnable() {
            _vibration = (VibrationDataObject) target;
        }

        public override void OnInspectorGUI() {
            DrawConfigurations();
            DrawActionButtons();
        }

        #region Editor code
        private void DrawActionButtons() {
            if (GUILayout.Button("Update threshold")) {
                UpdateThresholdRequest();
            }
            if (GUILayout.Button("Send vibration")) {
                SendVibrationRequest();
            }
        }
        private void DrawConfigurations() {
            using (new EditorGUILayout.VerticalScope("box")) {
                AmplitudeField();
                using (InspectorExtensions.EditorColors.BackgroundColorScope(
                    InspectorExtensions.EditorColors.PastelBlue)) {
                    using (new EditorGUILayout.VerticalScope("box")) {
                        SampleIntervalField();
                        ThresholdField();
                    }
                }
                using (InspectorExtensions.EditorColors.BackgroundColorScope(
                    InspectorExtensions.EditorColors.PastelOliveGreen)) {
                    using (new EditorGUILayout.VerticalScope("box")) {
                        IpField();
                        PortField();
                    }
                }
                HelpBox();
            }
        }
        private void SampleIntervalField() {
            _vibration.data.SampleInterval =
                EditorGUILayout.FloatField(_sampleIntervalGuiContent, _vibration.data.SampleInterval);
        }
        private void ThresholdField() {
            _vibration.data.Threshold = EditorGUILayout.FloatField(_thresholdGuiContent, _vibration.data.Threshold);
        }
        private void AmplitudeField() {
            _vibration.data.AmplitudeCurve = EditorGUILayout.CurveField("Amplitude", _vibration.data.AmplitudeCurve);
        }
        private void HelpBox() {
            if (!string.IsNullOrEmpty(_helpString)) {
                EditorGUILayout.HelpBox(_helpString, MessageType.Warning);
            }
        }
        private void IpField() {
            EditorGUI.BeginChangeCheck();
            IpAddress = EditorGUILayout.DelayedTextField("IP Address", IpAddress);
            if (EditorGUI.EndChangeCheck()) {
                _helpString = "";
                ParseIp();
            }
        }
        private void PortField() {
            Port = (ushort) EditorGUILayout.IntField("Port", Port);
        }
        private void ParseIp() {
            string[] splitIp = IpAddress.Split('.');
            var ipSegments = new byte[splitIp.Length];
            if (ipSegments.Length != 4) {
                _helpString = "IP address should be something like: XXX.YYY.ZZZ.AAA";
            }
            else {
                for (var i = 0; i < splitIp.Length; i++) {
                    if (!byte.TryParse(splitIp[i], out ipSegments[i])) {
                        _helpString = "IP segments should be between 0-255 [inclusive] " + i+1 + ". segment is wrong.";
                    }
                }
            }
        }
        #endregion

        #region Network code
        private async Task<string> GetRequestAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using(HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using(Stream stream = response.GetResponseStream())
            using(StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private void UpdateThresholdRequest() {
            var uri = new StringBuilder("http://");
            uri.Append(IpAddress);
            uri.Append(":");
            uri.Append(Port);
            uri.Append("/update_threshold?t=");
            uri.Append(_vibration.data.Threshold);
            // Debug.Log(uri);
            // Send it and forget about it, therefore not waiting for response.
            GetRequestAsync(uri.ToString());
        }

        private void SendVibrationRequest() {
            var uri = new StringBuilder("http://");
            uri.Append(IpAddress);
            uri.Append(":");
            uri.Append(Port);
            uri.Append("/vibrate?");
            uri.Append(_vibration.data.ToUrlParameter);
            Debug.Log(uri);
            // Send it and forget about it, therefore not waiting for response.
            GetRequestAsync(uri.ToString());
        }
        #endregion
    }
}