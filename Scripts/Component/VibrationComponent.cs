using System;
using System.Collections.Generic;
using Mobge;
using Mobge.BrigRex;
using UnityEngine;

namespace HapticFeedback.Component {
    public class VibrationComponent : ComponentDefinition<VibrationComponent.Data> {
        [Serializable]
        public class Data : BaseComponent {
            [SerializeField] private VibrationDataObject vibrationDataObject;
            [SerializeField] private bool shouldDefaultToNormalVibrateIfNoHapticEngineFound;
            [SerializeField] [HideInInspector] private LogicConnections _connections;

            public override void Start(in InitArgs initData) {
#if UNITY_EDITOR
                if (vibrationDataObject == null)
                    Debug.LogError("Vibration data is missing. Did you forget to put it into the component?");
#endif
            }

            public override LogicConnections Connections {
                get => _connections;
                set => _connections = value;
            }

            public override object HandleInput(ILogicComponent sender, int index, object input) {
                switch (index) {
                    case 0:
                        vibrationDataObject.data.Vibrate(shouldDefaultToNormalVibrateIfNoHapticEngineFound);
                        break;
                    case 1:
                        Manager.Haptic(HapticTypes.LightImpact, shouldDefaultToNormalVibrateIfNoHapticEngineFound);
                        break;
                    case 2:
                        Manager.Haptic(HapticTypes.MediumImpact, shouldDefaultToNormalVibrateIfNoHapticEngineFound);
                        break;
                    case 3:
                        Manager.Haptic(HapticTypes.HeavyImpact, shouldDefaultToNormalVibrateIfNoHapticEngineFound);
                        break;
                    case 4:
                        Manager.Haptic(HapticTypes.Selection, shouldDefaultToNormalVibrateIfNoHapticEngineFound);
                        break;
                    case 5:
                        Manager.Haptic(HapticTypes.Success, shouldDefaultToNormalVibrateIfNoHapticEngineFound);
                        break;
                    case 6:
                        Manager.Haptic(HapticTypes.Warning, shouldDefaultToNormalVibrateIfNoHapticEngineFound);
                        break;
                    case 7:
                        Manager.Haptic(HapticTypes.Failure, shouldDefaultToNormalVibrateIfNoHapticEngineFound);
                        break;
                }

                return null;
            }

#if UNITY_EDITOR
            public override void EditorInputs(List<LogicSlot> slots) {
                slots.Add(new LogicSlot("Custom Vibrate", 0));
                slots.Add(new LogicSlot("Light Impact Haptic", 1));
                slots.Add(new LogicSlot("Medium Impact Haptic", 2));
                slots.Add(new LogicSlot("Heavy Impact Haptic", 3));
                slots.Add(new LogicSlot("Selection Haptic", 4));
                slots.Add(new LogicSlot("Success Haptic", 5));
                slots.Add(new LogicSlot("Warning Haptic", 6));
                slots.Add(new LogicSlot("Failure Haptic", 7));
            }
#endif
        }
    }
}