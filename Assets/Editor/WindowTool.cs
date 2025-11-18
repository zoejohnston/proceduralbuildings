using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using UnityEditor.Search;

[EditorTool("Window Tool", typeof(BuildingPart))]
public class WindowTool : EditorTool
{   
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

    [SerializeField]
    private Texture2D toolIcon;
    private GUIContent toolInfo;

    private WindowSelectorOverlay overlay;

    private void OnEnable()
    {
        if (toolInfo == null) {
            toolInfo = new GUIContent() { image = toolIcon, text = "Window Tool" };
        }
    }

    public override GUIContent toolbarIcon
    {
        get { return toolInfo; }
    }

    public override void OnActivated()
    {
        SceneView.AddOverlayToActiveView(overlay = new WindowSelectorOverlay());
    }

    public override void OnWillBeDeactivated()
    {
        SceneView.RemoveOverlayFromActiveView(overlay);
    }

    public override void OnToolGUI(EditorWindow _)
    {
        if (overlay == null) return;
        if (overlay.window == null) return;

        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        LayerMask layerMask = LayerMask.GetMask("Walls");

        if (Physics.Raycast(ray, out RaycastHit hit, 10.0f, layerMask))
        {
            if (hit.collider.gameObject.TryGetComponent<WallCollider>(out WallCollider wallCollider))
            {
                Transform buildingPartTransform = wallCollider.transform.root;

                if (buildingPartTransform.TransformDirection(wallCollider.normal) == hit.normal)
                {
                    Handles.color = Color.white;
                    Handles.DrawWireDisc(hit.point, hit.normal, 0.1f);

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
