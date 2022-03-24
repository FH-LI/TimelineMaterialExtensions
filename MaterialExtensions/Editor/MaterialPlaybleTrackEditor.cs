using UnityEngine;
using UnityEditor;
using UnityEditor.Timeline;

[CustomEditor(typeof(MaterialPlayableTrack))]
public class MaterialPlaybleTrackEditor : Editor
{
    SerializedProperty _controls;
    static class Labels
    {
        public static readonly GUIContent Remove = new GUIContent("Remove");
    }

    void OnEnable()
    {
        _controls = serializedObject.FindProperty("template.controls");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        // Draw all the controls using the "header with a toggle" style.
        for (var i = 0; i < _controls.arraySize; i++)
        {
            CoreEditorUtils.DrawSplitter();

            var title = "Control Element " + (i + 1);
            var control = _controls.GetArrayElementAtIndex(i);
            var enabled = control.FindPropertyRelative("enabled");

            var toggle = CoreEditorUtils.DrawHeaderToggle(title, control, enabled, pos => OnContextClick(pos, i));

            if (!toggle) continue;

            using (new EditorGUI.DisabledScope(!enabled.boolValue))
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(control);
                serializedObject.ApplyModifiedProperties();
            }
        }

        // We have to manually refresh the timeline.
        if (EditorGUI.EndChangeCheck())
            TimelineEditor.Refresh(RefreshReason.ContentsModified);

        if (_controls.arraySize > 0) CoreEditorUtils.DrawSplitter();
        EditorGUILayout.Space();

        // "Add" button
        if (GUILayout.Button("Add Control Element")) AppendDefaultMidiControl();
    }

    void OnContextClick(Vector2 pos, int index)
    {
        var menu = new GenericMenu();

        menu.AddSeparator(string.Empty);
        menu.AddItem(Labels.Remove, false, () => OnRemoveControl(index));

        menu.DropDown(new Rect(pos, Vector2.zero));
    }

    void OnRemoveControl(int index)
    {
        serializedObject.Update();
        _controls.DeleteArrayElementAtIndex(index);
        serializedObject.ApplyModifiedProperties();
        TimelineEditor.Refresh(RefreshReason.ContentsModified);
    }
    void AppendDefaultMidiControl()
    {
        // Expand the array via SerializedProperty.
        var index = _controls.arraySize;
        _controls.InsertArrayElementAtIndex(index);

        var prop = _controls.GetArrayElementAtIndex(index);
        prop.isExpanded = true;

        serializedObject.ApplyModifiedProperties();

        // Set a new control instance.
        var track = (MaterialPlayableTrack)target;
        var controls = track.template.controls;
        Undo.RecordObject(track, "Add Material Control");
        controls[controls.Length - 1] = new MaterialControl();
    }
}
