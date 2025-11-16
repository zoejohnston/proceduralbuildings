using System.Collections;
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
    // Code from https://discussions.unity.com/t/fbx-exporter-binary-export-doesnt-work-via-editor-scripting/841939/3
    private static void ExportWorkaround(GameObject objectToExport, string filePath)
    {
        // Find relevant internal types in Unity.Formats.Fbx.Editor assembly
        Type[] types = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName == "Unity.Formats.Fbx.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetTypes();
        Type optionsInterfaceType = types.First(x => x.Name == "IExportOptions");
        Type optionsType = types.First(x => x.Name == "ExportOptionsSettingsSerializeBase");

        // Instantiate a settings object instance
        MethodInfo optionsProperty = typeof(ModelExporter).GetProperty("DefaultOptions", BindingFlags.Static | BindingFlags.NonPublic).GetGetMethod(true);
        object optionsInstance = optionsProperty.Invoke(null, null);

        // Change the export setting from ASCII to binary
        FieldInfo exportFormatField = optionsType.GetField("exportFormat", BindingFlags.Instance | BindingFlags.NonPublic);
        exportFormatField.SetValue(optionsInstance, 1);

        // Invoke the ExportObject method with the settings param
        MethodInfo exportObjectMethod = typeof(ModelExporter).GetMethod("ExportObject", BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new Type[] { typeof(string), typeof(UnityEngine.Object), optionsInterfaceType }, null);
        exportObjectMethod.Invoke(null, new object[] { filePath, objectToExport, optionsInstance });
    }

    [MenuItem("Export/Export selected BuildingParts")]
    private static void ExportBuilding()
    {
        //
        GameObject exportParent = new GameObject("Export");

        GameObject bricksToExport = new GameObject("Bricks");
        bricksToExport.transform.SetParent(exportParent.transform);
        GameObject shinglesToExport = new GameObject("Shingles");
        shinglesToExport.transform.SetParent(exportParent.transform);
        GameObject beamsToExport = new GameObject("Beams");
        beamsToExport.transform.SetParent(exportParent.transform);

        //
        GameObject[] selectedObjects = Selection.gameObjects;
        List<BuildingPart> buildingParts = new List<BuildingPart>();

        foreach (GameObject obj in selectedObjects) {
            if (obj.TryGetComponent(out BuildingPart buildingPart)) buildingParts.Add(buildingPart);
        }

        // 
        CombineInstance[] brickInstances = new CombineInstance[buildingParts.Count];
        CombineInstance[] shingleInstances = new CombineInstance[buildingParts.Count];
        CombineInstance[] beamInstances = new CombineInstance[buildingParts.Count];

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

            DestroyImmediate(meshFilters[0]);
            DestroyImmediate(meshFilters[1]);
            DestroyImmediate(meshFilters[2]);
        }

        //
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

        //
        string fileName = "Exports/" + SceneManager.GetActiveScene().name + ".fbx";
        string filePath = Path.Combine(Application.dataPath, fileName);

        ExportWorkaround(exportParent, filePath);
        DestroyImmediate(exportParent);
    }
}
