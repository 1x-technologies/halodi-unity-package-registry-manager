using System;
using System.Collections;
using System.Collections.Generic;
using Halodi.PackageRegistry.Core;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Halodi.PackageRegistry.UI
{
    public class RegistryManagerView : EditorWindow
    {
        [MenuItem("Packages/Manage scoped registries", false, 21)]
        internal static void ManageRegistries()
        {
            #if UNITY_2020_1_OR_NEWER
            SettingsService.OpenProjectSettings("Project/Package Manager");
            #else
            EditorWindow.GetWindow<RegistryManagerView>(true, "Registry manager", true);
            #endif
        }

        private ReorderableList drawer;

        void OnEnable()
        {
            drawer = GetRegistryListView(new RegistryManager());
            minSize = new Vector2(640, 320);
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Scoped registries", EditorStyles.whiteLargeLabel);
            drawer.DoLayoutList();
        }

        internal static ReorderableList GetRegistryListView(RegistryManager registryManager)
        {
            ReorderableList registryList = null;
            registryList = new ReorderableList(registryManager.registries, typeof(ScopedRegistry), false, true, true, true)
            {
                drawHeaderCallback = rect =>
                {
                    GUI.Label(rect, "Scoped Registries");
                },
                drawElementCallback = (rect, index, active, focused) =>
                {
                    var registry = registryList.list[index] as ScopedRegistry;
                    if (registry == null) return;

                    var rect2 = rect;
                    rect2.width = 60;
                    EditorGUI.ToggleLeft(rect2, "Auth", !string.IsNullOrEmpty(registry.token) && registry.isValidCredential());
                    
                    rect.x += 60;
                    rect.width -= 120;
                    EditorGUI.LabelField(rect, registry.name + ": " + registry.url);
                    
                    rect.x = rect.xMax;
                    rect.width = 60;
                    if (GUI.Button(rect, "Edit"))
                    {
                        ScopedRegistryEditorView registryEditor = EditorWindow.GetWindow<ScopedRegistryEditorView>(true, "Edit registry", true);
                        registryEditor.Edit(registry, registryManager);
                    }
                },
                onAddCallback = list =>
                {
                    ScopedRegistryEditorView registryEditor = EditorWindow.GetWindow<ScopedRegistryEditorView>(true, "Add registry", true);
                    registryEditor.CreateNew(registryManager);
                },
                onRemoveCallback = list =>
                {
                    registryManager.Remove(registryManager.registries[list.index]);
                }
            };
            return registryList;
        }
    }



}