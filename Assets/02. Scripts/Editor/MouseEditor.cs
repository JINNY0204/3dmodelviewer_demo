using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MouseEventManager)), CanEditMultipleObjects]
public class MouseEditor : Editor
{
    MouseEventManager mouseEvenetManager;
    private void OnEnable()
    {
        mouseEvenetManager = target as MouseEventManager;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        mouseEvenetManager.defaultCursor = (Texture2D)EditorGUILayout.ObjectField("Add Cursor Image", mouseEvenetManager.defaultCursor, typeof(Texture2D), false);
    }
}