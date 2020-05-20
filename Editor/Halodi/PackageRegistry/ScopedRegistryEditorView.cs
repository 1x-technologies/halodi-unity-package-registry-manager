
using System;
using UnityEditor;
using UnityEngine;

namespace Halodi.PackageRegistry
{
    class ScopedRegistryEditorView : EditorWindow
    {
        private bool initialized = false;

        private RegistryManagerController controller;

        private bool createNew;

        private ScopedRegistry registry;

        void OnEnable()
        {
        }

        void OnDisable()
        {
            initialized = false;
        }

        public void CreateNew(RegistryManagerController controller)
        {
            this.controller = controller;
            this.createNew = true;
            this.registry = new ScopedRegistry();
            this.initialized = true;
        }

        public void Edit(ScopedRegistry registry, RegistryManagerController controller)
        {
            this.controller = controller;
            this.registry = registry;
            this.createNew = false;
            this.initialized = true;
        }


        void OnGUI()
        {
            if (initialized)
            {


                if (createNew)
                {
                    EditorGUILayout.LabelField("Add scoped registry ");
                    registry.name = EditorGUILayout.TextField("name: ", registry.name);

                    EditorGUI.BeginChangeCheck();
                    registry.url = EditorGUILayout.TextField("url: ", registry.url);
                    if (EditorGUI.EndChangeCheck())
                    {
                        UpdateCredential();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Edit scoped registry");
                    EditorGUILayout.LabelField("Name: " + registry.name);
                    EditorGUILayout.LabelField("url: " + registry.url);
                }


                string scope = String.Join(",", registry.scopes);
                scope = EditorGUILayout.TextField("Scope: ", scope);
                registry.scopes = scope.Split(',');


                registry.auth = EditorGUILayout.Toggle("Always auth: ", registry.auth);
                registry.token = EditorGUILayout.TextField("Token: ", registry.token);



                if (GUILayout.Button("Login to registry and get token"))
                {
                    GetTokenView.CreateWindow(registry);
                }                
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Tip: Restart Unity to reload credentials after saving.");



                if (createNew)
                {
                    if (GUILayout.Button("Add"))
                    {
                        Add();
                    }
                }
                else
                {
                    if (GUILayout.Button("Save"))
                    {
                        Save();
                    }
                }


                if (GUILayout.Button("Cancel"))
                {
                    Close();
                    GUIUtility.ExitGUI();
                }
            }
        }

        private void Save()
        {
            if (registry.isValid())
            {
                controller.Save(registry);
                Close();
                GUIUtility.ExitGUI();
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid", "Invalid settings for registry.", "Ok");
            }
        }

        private void Add()
        {
            if (registry.isValid())
            {
                controller.Save(registry);
                Close();
                GUIUtility.ExitGUI();
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid", "Invalid settings for registry.", "Ok");
            }

        }

        private void UpdateCredential()
        {
            if (controller.credentialManager.HasRegistry(registry.url))
            {
                NPMCredential cred = controller.credentialManager.GetCredential(registry.url);
                registry.auth = cred.alwaysAuth;
                registry.token = cred.token;
            }
        }



    }
}
