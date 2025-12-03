using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using UnityEditor.Search;

/// <summary>
/// A tool for adding Windows to BuildingParts.
/// </summary>
[EditorTool("Window Tool", typeof(BuildingPart))]
public class WindowTool : EditorTool
{   
    /// <summary>
    /// An overlay that appears when the window tool is active.
    /// Adapted from: https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Overlays.Overlay.html
    /// </summary>
    [Overlay(defaultDisplay = true)]
    public class WindowSelectorOverlay : Overlay, ITransientOverlay
    {   
        ObjectField objectField;
        public Window window => objectField?.value as Window;

        public override VisualElement CreatePanelContent()
        {
            VisualElement visualElement = new VisualElement();

            objectField = new ObjectField();
            objectField.objectType = typeof(Window);
            objectField.label = "Select a window to place:";

            visualElement.Add(objectField);

            return visualElement;
        }

        public bool visible => true;
    }

    // Used to set an icon for the tool
    [SerializeField]
    private Texture2D toolIcon;
    private GUIContent toolInfo;

    // The overlay defined above
    private WindowSelectorOverlay overlay;

    // Create toolInfo if not yet done
    private void OnEnable()
    {
        if (toolInfo == null) {
            toolInfo = new GUIContent() { image = toolIcon, text = "Window Tool" };
        }
    }

    // Add toolInfo to the tool
    public override GUIContent toolbarIcon
    {
        get { return toolInfo; }
    }

    // Adds the overlay to the scene viewer
    public override void OnActivated()
    {
        SceneView.AddOverlayToActiveView(overlay = new WindowSelectorOverlay());
    }

    // Removes the overlay to the scene viewer
    public override void OnWillBeDeactivated()
    {
        SceneView.RemoveOverlayFromActiveView(overlay);
    }

    // Implements the window tool
    public override void OnToolGUI(EditorWindow _)
    {
        if (overlay == null) return;
        if (overlay.window == null) return;

        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        LayerMask layerMask = LayerMask.GetMask("Walls");

        // If the mouse is on top of a wall...
        if (Physics.Raycast(ray, out RaycastHit hit, 10.0f, layerMask)) {
            if (hit.collider.gameObject.TryGetComponent<WallCollider>(out WallCollider wallCollider)) {
                Transform buildingPartTransform = wallCollider.transform.root;

                // and the part of the wall it is hovering over is the outside surface...
                if (buildingPartTransform.TransformDirection(wallCollider.normal) == hit.normal) {
                    // draw an indicator to show where a window will be placed
                    Handles.color = Color.white;
                    Handles.DrawWireDisc(hit.point, hit.normal, 0.1f);

                    // Add the window to the wall on click
                    if (e.type == EventType.MouseDown && e.button == 0) {
                        BuildingPart buildingPart = buildingPartTransform.gameObject.GetComponent<BuildingPart>();
                        wallCollider.PlaceWindow(overlay.window, buildingPart, hit.point);
                        e.Use();
                    }
                }
            }
        }

        if (e.type == EventType.MouseMove) {
            SceneView.RepaintAll();
        }
    }
}
