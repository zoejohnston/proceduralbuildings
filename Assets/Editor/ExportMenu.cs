using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Reflection;

public class ExportMenu : MonoBehaviour
{
    /// <summary>
    /// Exports the GameObject <c>objectToExport</c> as an FBX file at file path <c>filePath</c>.
    /// The FBX file is in binary format so that programs like Blender can open it. This is not an easy
    /// thing to do in this version of Unity! I follow a workaround discussed here:
    /// https://discussions.unity.com/t/fbx-exporter-binary-export-doesnt-work-via-editor-scripting/841939/3
    /// </summary>
    /// <param name="objectToExport">The GameObject to export as an FBX file.</param>
    /// <param name="filePath">The filepath to export to.</param>
    private static void ExportWorkaround(GameObject objectToExport, string filePath)
    {
        string assemblyName = "Unity.Formats.Fbx.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        Type[] types = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName == assemblyName).GetTypes();
        Type optionsInterfaceType = types.First(x => x.Name == "IExportOptions");
        Type optionsType = types.First(x => x.Name == "ExportOptionsSettingsSerializeBase");

        MethodInfo optionsProperty = typeof(ModelExporter).GetProperty("DefaultOptions", BindingFlags.Static | BindingFlags.NonPublic).GetGetMethod(true);
        object optionsInstance = optionsProperty.Invoke(null, null);

        FieldInfo exportFormatField = optionsType.GetField("exportFormat", BindingFlags.Instance | BindingFlags.NonPublic);
        exportFormatField.SetValue(optionsInstance, 1);

        MethodInfo exportObjectMethod = typeof(ModelExporter).GetMethod("ExportObject", BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new Type[] { typeof(string), typeof(UnityEngine.Object), optionsInterfaceType }, null);
        exportObjectMethod.Invoke(null, new object[] { filePath, objectToExport, optionsInstance });
    }

    /// <summary>
    /// Exports the selected BuildingParts as an FBX file.
    /// </summary>
    [MenuItem("Export/Export selected BuildingParts")]
    private static void ExportBuilding()
    {
        // Builds a GameObject to export
        GameObject exportParent = new GameObject("Export");

        GameObject bricksToExport = new GameObject("Bricks");
        bricksToExport.transform.SetParent(exportParent.transform);
        GameObject shinglesToExport = new GameObject("Shingles");
        shinglesToExport.transform.SetParent(exportParent.transform);
        GameObject beamsToExport = new GameObject("Beams");
        beamsToExport.transform.SetParent(exportParent.transform);
        GameObject panesToExport = new GameObject("Panes");
        panesToExport.transform.SetParent(exportParent.transform);

        // Get the selected BuildingParts
        GameObject[] selectedObjects = Selection.gameObjects;
        List<BuildingPart> buildingParts = new List<BuildingPart>();

        foreach (GameObject obj in selectedObjects) {
            if (obj.TryGetComponent(out BuildingPart buildingPart)) buildingParts.Add(buildingPart);
        }

        // Combine the meshes from each BuildingParts
        CombineInstance[] brickInstances = new CombineInstance[buildingParts.Count];
        CombineInstance[] shingleInstances = new CombineInstance[buildingParts.Count];
        CombineInstance[] beamInstances = new CombineInstance[buildingParts.Count];
        CombineInstance[] paneInstances = new CombineInstance[buildingParts.Count];

        for (int i = 0; i < buildingParts.Count; i++) {
            MeshFilter[] meshFilters = buildingParts[i].PrepForExport();
            if (meshFilters == null) return;

            brickInstances[i] = new CombineInstance {
                mesh = meshFilters[0].sharedMesh,
                transform = Matrix4x4.identity
            };

            shingleInstances[i] = new CombineInstance {
                mesh = meshFilters[1].sharedMesh,
                transform = Matrix4x4.identity
            };

            beamInstances[i] = new CombineInstance {
                mesh = meshFilters[2].sharedMesh,
                transform = Matrix4x4.identity
            };

            paneInstances[i] = new CombineInstance {
                mesh = meshFilters[3].sharedMesh,
                transform = Matrix4x4.identity
            };

            DestroyImmediate(meshFilters[0]);
            DestroyImmediate(meshFilters[1]);
            DestroyImmediate(meshFilters[2]);
            DestroyImmediate(meshFilters[3]);
        }

        Mesh combinedBrickMesh = new Mesh();
        combinedBrickMesh.CombineMeshes(brickInstances);
        MeshFilter bricksToExportMeshFilter = bricksToExport.AddComponent<MeshFilter>();
        bricksToExportMeshFilter.sharedMesh = combinedBrickMesh;

        Mesh combinedShingleMesh = new Mesh();
        combinedShingleMesh.CombineMeshes(shingleInstances);
        MeshFilter shinglesToExportMeshFilter = shinglesToExport.AddComponent<MeshFilter>();
        shinglesToExportMeshFilter.sharedMesh = combinedShingleMesh;

        Mesh combinedBeamMesh = new Mesh();
        combinedBeamMesh.CombineMeshes(beamInstances);
        MeshFilter beamsToExportMeshFilter = beamsToExport.AddComponent<MeshFilter>();
        beamsToExportMeshFilter.sharedMesh = combinedBeamMesh;

        Mesh combinedPaneMesh = new Mesh();
        combinedPaneMesh.CombineMeshes(paneInstances);
        MeshFilter panesToExportMeshFilter = panesToExport.AddComponent<MeshFilter>();
        panesToExportMeshFilter.sharedMesh = combinedPaneMesh;

        // The filename used is based on the name of the scene you are workin in.
        string fileName = "Exports/" + SceneManager.GetActiveScene().name + ".fbx";
        string filePath = Path.Combine(Application.dataPath, fileName);

        ExportWorkaround(exportParent, filePath);
        DestroyImmediate(exportParent);
    }
}
