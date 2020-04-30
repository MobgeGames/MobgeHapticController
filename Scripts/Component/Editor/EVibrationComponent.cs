using Mobge.BrigRex;
using UnityEditor;
using UnityEngine;

namespace HapticFeedback.Component {
    [CustomEditor(typeof(VibrationComponent))]
    public class EVibrationComponent : EComponentDefinition {
        public override EditableElement CreateEditorElement(BaseComponent dataObject) {
            return new Editor(dataObject as VibrationComponent.Data, this);
        }
        public class Editor : EditableElement<VibrationComponent.Data> {
            private bool _editMode;
            public Editor(VibrationComponent.Data component, EComponentDefinition editor) : base(component, editor) {
            }
            // In this method, implement the logic you would normally implement under OnInspectorGUI() 
            public override void DrawGUILayout() {
                // Inspector: Boiler plate for exclusive editing of the element
                base.DrawGUILayout();
                //_editMode = ExclusiveEditField("edit on scene");
            }
            // In this method, implement the logic you would normally implement under OnSceneGUI() 
            public override bool SceneGUI(in SceneParams @params) {
                // Scene: Boiler plate for exclusive editing of the element
                bool enabled = @params.selected /* && _editMode */ ;
                bool edited = false;

                // Logic here. Explicit edit is on, do what you need
                // On Change set edited variable to true to save the data and update the visual

                return enabled && edited;
            }
        }
    }
}
