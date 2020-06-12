
using System;
using Halodi.PackageRegistry.Core;
using UnityEditor;
using UnityEngine;

namespace Halodi.PackageRegistry.UI
{
    class CredentialEditorView : EditorWindow
    {
        private bool initialized = false;

        private CredentialManager credentialManager;

        private bool createNew;

        private ScopedRegistry registry;

        private int tokenMethod;


        void OnEnable()
        {
            tokenMethod = 0;
            minSize = new Vector2(480, 320);
        }

        void OnDisable()
        {
            initialized = false;
        }

        public void CreateNew(CredentialManager credentialManager)
        {
            this.credentialManager = credentialManager;
            this.createNew = true;
            this.registry = new ScopedRegistry();
            this.initialized = true;
        }

        public void Edit(NPMCredential credential, CredentialManager credentialManager)
        {
            this.credentialManager = credentialManager;
            this.registry = new ScopedRegistry();
            this.registry.url = credential.url;
            this.registry.auth = credential.alwaysAuth;
            this.registry.token = credential.token;

            this.createNew = false;
            this.initialized = true;
        }


        void OnGUI()
        {
            if (initialized)
            {


                if (createNew)
                {
                    EditorGUILayout.LabelField("Add credential ", EditorStyles.whiteLargeLabel);

                    registry.url = EditorGUILayout.TextField("url: ", registry.url);
                }
                else
                {
                    EditorGUILayout.LabelField("Edit credential", EditorStyles.whiteLargeLabel);
                    EditorGUILayout.LabelField("url: " + registry.url);
                }

                registry.auth = EditorGUILayout.Toggle("Always auth: ", registry.auth);
                registry.token = EditorGUILayout.TextField("Token: ", registry.token);

                EditorGUILayout.Space();

                tokenMethod = GetTokenView.CreateGUI(tokenMethod, registry);


                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Tip: Restart Unity to reload credentials after saving.");


                if (createNew)
                {
                    if (GUILayout.Button("Add"))
                    {
                        Save();
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
            if (registry.isValidCredential() && !string.IsNullOrEmpty(registry.token))
            {
                credentialManager.SetCredential(registry.url, registry.auth, registry.token);
                credentialManager.Write();
                Close();
                GUIUtility.ExitGUI();
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid", "Invalid settings for credential.", "Ok");
            }
        }



    }
}
