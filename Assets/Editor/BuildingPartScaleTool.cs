using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using System;
using System.Collections.Generic;

[EditorToolContext("Building Part", typeof(BuildingPart))]
public class BuildingPartScaleContext : EditorToolContext
{
    public override void OnToolGUI(EditorWindow _) { }
    
    protected override Type GetEditorToolType(Tool tool)
    {
        switch (tool)
        {
            case Tool.Scale:
                return typeof(BuildingPartScaleTool);
            default:
                return null;
        }
    }
}

public class BuildingPartScaleTool : EditorTool
{
    struct Selected
    {
        public Transform transform;
    }

    List<Selected> m_Selected = new List<Selected>();

    void StartScale(Vector3 origin)
    {
        m_Selected.Clear();

        foreach (var trs in Selection.transforms)
            m_Selected.Add(new Selected() { transform = trs });

        Undo.RecordObjects(Selection.transforms, "Building Part");
    }
    
    public override bool IsAvailable()
    {
        return Selection.activeGameObject != null &&
            Selection.activeGameObject.GetComponent<BuildingPart>() != null;
    }

    public override void OnToolGUI(EditorWindow _)
    {
        var evt = Event.current.type;
        var hot = GUIUtility.hotControl;

        EditorGUI.BeginChangeCheck();
        Vector3 mouseDelta = Handles.ScaleHandle(Vector3.one, Tools.handlePosition, Tools.handleRotation, HandleUtility.GetHandleSize(Tools.handlePosition));
        
        if (evt == EventType.MouseDown && hot != GUIUtility.hotControl)StartScale(mouseDelta);

        if (EditorGUI.EndChangeCheck())
        {
            Vector3 scaleDelta = (mouseDelta - Vector3.one) * 0.1f;

            foreach (var selected in m_Selected)
            {
                BuildingPart buildingPart = selected.transform.gameObject.GetComponent<BuildingPart>();
                buildingPart.UpdateScale(scaleDelta);
            }
        }
    }
}
