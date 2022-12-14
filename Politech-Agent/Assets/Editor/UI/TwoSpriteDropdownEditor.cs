using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TwoSpriteDropdown))]
public class TwoSpriteDropdownEditor : UnityEditor.UI.DropdownEditor
{
    public override void OnInspectorGUI()
    {
        TwoSpriteDropdown component = (TwoSpriteDropdown)target;

        base.OnInspectorGUI();

        component.normal = (Sprite)EditorGUILayout.ObjectField("Normal", component.normal, typeof(Sprite), true);
        component.opened = (Sprite)EditorGUILayout.ObjectField("Opened", component.opened, typeof(Sprite), true);
    }
}
