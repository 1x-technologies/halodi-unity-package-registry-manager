using System;
using System.Collections;
using System.Collections.Generic;
using Halodi.PackageRegistry.Core;
using UnityEditor;
using UnityEngine;

namespace Halodi.PackageRegistry.UI
{
    public class RegistryManagerView : EditorWindow
    {
        [MenuItem("Packages/Manage scoped registries", false, 21)]
        internal static void ManageRegistries()
        {
            EditorWindow.GetWindow<RegistryManagerView>(true, "Registry manager", true);
        }

        private RegistryManager controller;
        private Vector2 scrollPos;

        void OnEnable()
        {
            controller = new RegistryManager();
            minSize = new Vector2(640, 320);
        }

        void OnDisable()
        {
            controller = null;
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Scoped registries", EditorStyles.whiteLargeLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);


            foreach (ScopedRegistry registry in controller.registries)
            {
                GUIStyle boxStyle = new GUIStyle();
                boxStyle.padding = new RectOffset(10, 10, 0, 0);

                EditorGUILayout.BeginHorizontal(boxStyle);
                EditorGUILayout.LabelField(registry.url);
                if (GUILayout.Button("Edit"))
                {
                    EditRegistry(registry);
                    CloseWindow();
                }

                if (GUILayout.Button("Remove"))
                {
                    Remove(registry);
                    CloseWindow();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add registry"))
            { 
                AddRegistry();
                CloseWindow();
            }

            if (GUILayout.Button("Close"))
            {
                CloseWindow();
            }

            EditorGUILayout.EndHorizontal();

        }

        private void Remove(ScopedRegistry registry)
        {
            controller.Remove(registry);
        }

        private void AddRegistry()
        {
            ScopedRegistryEditorView registryEditor = EditorWindow.GetWindow<ScopedRegistryEditorView>(true, "Add registry", true);
            registryEditor.CreateNew(controller);

        }

        private void CloseWindow()
        {
            Close();
            GUIUtility.ExitGUI();
        }

        private void EditRegistry(ScopedRegistry registry)
        {
            ScopedRegistryEditorView registryEditor = EditorWindow.GetWindow<ScopedRegistryEditorView>(true, "Edit registry", true);
            registryEditor.Edit(registry, controller);
        }
    }



}