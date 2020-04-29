using System;
using System.Collections.Generic;
using Mobge;
using Mobge.BrigRex;
using UnityEngine;

namespace HapticFeedback.Component
{
    public class VibrationComponent : ComponentDefinition<VibrationComponent.Data>
    {
        [Serializable]
        public class Data : BaseComponent
        {
            [SerializeField] private VibrationDataObject vibrationDataObject;
            [SerializeField] [HideInInspector] private LogicConnections _connections;
            public override void Start(in InitArgs initData) {
#if UNITY_EDITOR
                if(vibrationDataObject == null) 
                    Debug.LogError("Vibration data is missing. Did you forget to put it into the component?");
#endif
            }

            public override LogicConnections Connections {
                get => _connections;
                set => _connections = value;
            }
            
            public override object HandleInput(ILogicComponent sender, int index, object input)
            {
                switch (index) {
                    case 0:
                        vibrationDataObject.data.Vibrate();
                        break;
                }
                return null;
            }
            
#if UNITY_EDITOR
            public override void EditorInputs(List<LogicSlot> slots)
            {
                slots.Add(new LogicSlot("Vibrate", 0));
            }
#endif
        }
    }
}