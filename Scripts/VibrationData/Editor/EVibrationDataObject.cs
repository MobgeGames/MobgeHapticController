using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoDiscovery;
using Mobge;
using Mobge.Threading;
using UnityEditor;
using UnityEngine;
using static Mobge.InspectorExtensions.EditorColors;

namespace HapticFeedback {
    [CustomEditor(typeof(VibrationDataObject))]
    // AKA VibrationClient
    public class EVibrationDataObject : Editor {
        private VibrationDataObject _vibration;
        private string _helpString;

        private Client _client;
        private List<IPEndPoint> _activeVibrationServers;

        private static string IpAddress {
            get => EditorPrefs.GetString("Mobge.HapticFeedback::IpAddress");
            set => EditorPrefs.SetString("Mobge.HapticFeedback::IpAddress", value);
        }

        private static ushort Port {
            get => (ushort) EditorPrefs.GetInt("Mobge.HapticFeedback::Port");
            set => EditorPrefs.SetInt("Mobge.HapticFeedback::Port", value);
        }

        private bool _isAutoDiscoveryOn;

        private bool IsAutoDiscoveryOn {
            get => _isAutoDiscoveryOn;
            set {
                if (value)
                    _client.Start();
                else
                    _client.Stop();
                _isAutoDiscoveryOn = value;
            }
        }

        private int _selectedServerIndex;

        private int SelectedServerIndex {
            get => _selectedServerIndex;
            set {
                _selectedServerIndex = value;
                if (_activeVibrationServers.Count <= _selectedServerIndex) return;
                IpAddress = _activeVibrationServers[_selectedServerIndex].Address.ToString();
                Port = (ushort) _activeVibrationServers[_selectedServerIndex].Port;
            }
        }

        private GUIContent _guiManualServerConfig;
        private GUIContent _guiAutomaticServerConfig;

        private GUIContent GUIServerConfig {
            get {
                // auto discovery
                if (_isAutoDiscoveryOn) {
                    if (_guiAutomaticServerConfig == null) {
                        _guiAutomaticServerConfig = new GUIContent("Automatic Server Discovery",
                            EditorGUIUtility.IconContent("Profiler.NetworkMessages@2x").image as Texture2D,
                            "Click to switch to manual server settings.");
                    }

                    return _guiAutomaticServerConfig;
                }

                // manual discovery
                if (_guiManualServerConfig == null) {
                    _guiManualServerConfig = new GUIContent("Manual Server Setting",
                        EditorGUIUtility.IconContent("Profiler.NetworkOperations@2x").image as Texture2D,
                        "Click to switch to automatic server discovery.");
                }

                return _guiManualServerConfig;
            }
        }

        private void OnEnable() {
            _vibration = (VibrationDataObject) target;
            if (_vibration.data.AmplitudeCurve == null) {
                _vibration.data.AmplitudeCurve = new AnimationCurve();
            }
            _isAutoDiscoveryOn = true;
            _activeVibrationServers = new List<IPEndPoint>();
            _client = new Client("VibeServer");
            // Event is raised on separate thread
            _client.ServersUpdated += servers => ThreadSystem.DoOnNewThread(() => {
                _activeVibrationServers.Clear();
                foreach (var server in servers) {
                    _activeVibrationServers.Add(server.Address);
                    // Debug.Log("Vibration server discovered : " + server.Address);
                }
            });
            IsAutoDiscoveryOn = true;
        }

        public override void OnInspectorGUI() {
            using (new EditorGUILayout.VerticalScope("box")) {
                AmplitudeField();
                using (BackgroundColorScope(PastelBlue)) {
                    using (new EditorGUILayout.VerticalScope("box")) {
                        SampleIntervalField();
                    }
                }

                using (BackgroundColorScope(PastelOliveGreen)) {
                    using (new EditorGUILayout.VerticalScope("box")) {
                        if (IsAutoDiscoveryOn) {
                            AutoDiscoveryField();
                        }
                        else {
                            IpField();
                            PortField();
                        }

                        DiscoveryConfigurationToggle();
                    }
                }

                HelpBox();
                SendVibrationButton();
            }
        }

        private void OnDestroy() {
            _client.Dispose();
        }

        private void OnDisable() {
            _client.Stop();
        }

        #region Editor code
        private void SendVibrationButton() {
            if (GUILayout.Button("Send vibration")) {
                SendVibrationRequest();
            }
        }

        private void DiscoveryConfigurationToggle() {
            if (GUILayout.Button(GUIServerConfig)) {
                IsAutoDiscoveryOn = !IsAutoDiscoveryOn;
            }
        }

        private void SampleIntervalField() {
            var max = 0.1f;
            var min = 0.025f;
            string label = "";
            var percentage = Mathf.InverseLerp(max, min, _vibration.data.SampleInterval);
            if (percentage >= 0 && percentage <= 0.25f) {
                label = "best size";
            } else if (percentage > 0.25f && percentage <= 0.5f) {
                label = "better size";
            } else if (percentage > 0.5f && percentage <= 0.75f) {
                label = "better quality";
            } else if (percentage > 0.75f && percentage <= 1f) {
                label = "best quality";
            }
            EditorGUI.BeginChangeCheck();
            _vibration.data.SampleInterval = EditorGUILayout.Slider(label, _vibration.data.SampleInterval, min, max);
            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(_vibration);
            }
        }

        private void AmplitudeField() {
            EditorGUI.BeginChangeCheck();
            _vibration.data.AmplitudeCurve = EditorGUILayout.CurveField("Amplitude", _vibration.data.AmplitudeCurve);
            if (EditorGUI.EndChangeCheck()) {
                AmplitudeSanityCheck();
                EditorUtility.SetDirty(_vibration);
            }
        }

        private void AmplitudeSanityCheck() {
            _helpString = "";
            var curve = _vibration.data.AmplitudeCurve;
            var maxTime = curve.keys[curve.length - 1].time;
            for (var time = 0.0f; time <= maxTime; time += 0.1f) {
                var y = curve.Evaluate(time);
                if (y > 1) {
                    _helpString = "Amplitude should be between 0 - 1 on the Y coordinate system";
                }
            }
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
                IpSanityCheck();
            }
        }

        private void PortField() {
            Port = (ushort) EditorGUILayout.IntField("Port", Port);
        }

        private void AutoDiscoveryField() {
            SelectedServerIndex =
                EditorLayoutDrawer.Popup("Connected server", _activeVibrationServers, SelectedServerIndex);
        }

        private void IpSanityCheck() {
            _helpString = "";
            string[] splitIp = IpAddress.Split('.');
            var ipSegments = new byte[splitIp.Length];
            if (ipSegments.Length != 4) {
                _helpString = "IP address should be something like: XXX.YYY.ZZZ.AAA";
            }
            else {
                for (var i = 0; i < splitIp.Length; i++) {
                    if (!byte.TryParse(splitIp[i], out ipSegments[i])) {
                        i++;
                        _helpString = "IP segments should be between 0-255 [inclusive]\n" + i + ". segment is wrong.";
                    }
                }
            }
        }
        #endregion

        #region Network code
        private async Task<string> GetRequestAsync(string uri) {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
                return await reader.ReadToEndAsync();
            }
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