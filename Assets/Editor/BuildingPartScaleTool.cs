using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using System;
using System.Collections.Generic;

// Code adapted from https://docs.unity3d.com/6000.2/Documentation/ScriptReference/EditorTools.EditorToolContext.html

/// <summary>
/// Adds BuildingPartScaleTool to a new EditorToolContext.
/// </summary>
[EditorToolContext("Building Part", typeof(BuildingPart))]
public class BuildingPartScaleContext : EditorToolContext
{
    //public override void OnToolGUI(EditorWindow _) { }

    // Returns the tool to be used for each of the default tools
    protected override Type GetEditorToolType(Tool tool)
    {
        switch (tool) {
            // Use BuildingPartScaleTool for scaling
            case Tool.Scale:
                return typeof(BuildingPartScaleTool);
            // Omit everything else
            default:
                return null;
        }
    }
}

/// <summary>
/// A scaling tool for BuildingParts that only affects their innerScale.
/// </summary>
public class BuildingPartScaleTool : EditorTool
{
    // Keeps track of which transforms are currently selected
    List<Transform> selectedTransforms = new List<Transform>();

    // Updates selectedTransforms
    void StartScale()
    {
        selectedTransforms.Clear();

        foreach (var trs in Selection.transforms)
            selectedTransforms.Add(trs);

        Undo.RecordObjects(Selection.transforms, "Building Part Scale Tool");
    }
    
    // Determines if the tool should be vailable or not
    public override bool IsAvailable()
    {
        return Selection.activeGameObject != null &&
            Selection.activeGameObject.GetComponent<BuildingPart>() != null;
    }

    // Implements the scaling tool
    public override void OnToolGUI(EditorWindow _)
    {
        var evt = Event.current.type;
        var hot = GUIUtility.hotControl;

        EditorGUI.BeginChangeCheck();
        Vector3 mouseDelta = Handles.ScaleHandle(Vector3.one, Tools.handlePosition, Tools.handleRotation, HandleUtility.GetHandleSize(Tools.handlePosition));
        
        if (evt == EventType.MouseDown && hot != GUIUtility.hotControl) StartScale();

        if (EditorGUI.EndChangeCheck()) {
            Vector3 scaleDelta = (mouseDelta - Vector3.one) * 0.1f;

            foreach (var selected in selectedTransforms) {
                BuildingPart buildingPart = selected.gameObject.GetComponent<BuildingPart>();
                buildingPart.UpdateScale(scaleDelta);
            }
        }
    }
}
