using UnityEngine;
using UnityEditor;

namespace Editor.PropertyDrawers
{
    /// <summary>
    /// Property drawer for ReadOnly attribute
    /// Makes fields visible but non-editable in Inspector
    /// </summary>
    [CustomPropertyDrawer(typeof(Game.Enemy.Components.ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Store the current GUI enabled state
            bool wasEnabled = GUI.enabled;
            
            // Disable GUI for this property
            GUI.enabled = false;
            
            // Draw the property field
            EditorGUI.PropertyField(position, property, label, true);
            
            // Restore the previous GUI enabled state
            GUI.enabled = wasEnabled;
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}