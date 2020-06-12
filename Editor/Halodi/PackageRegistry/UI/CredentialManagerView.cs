using System;
using System.Collections;
using System.Collections.Generic;
using Halodi.PackageRegistry.Core;
using UnityEditor;
using UnityEngine;

namespace Halodi.PackageRegistry.UI
{
    public class CredentialManagerView : EditorWindow
    {
        [MenuItem("Packages/Manage credentials", false, 20)]
        internal static void ManageCredentials()
        {
            EditorWindow.GetWindow<CredentialManagerView>(true, "Credential Manager", true);
        }

        private Vector2 credentialScrollPos;

        private CredentialManager credentialManager;

        void OnEnable()
        {
            credentialManager = new CredentialManager();
            minSize = new Vector2(640, 320);
        }


        void OnDisable()
        {
            credentialManager = null;
        }

        void OnGUI()
        {

            EditorGUILayout.LabelField("Credentials", EditorStyles.whiteLargeLabel);
            credentialScrollPos = EditorGUILayout.BeginScrollView(credentialScrollPos);

            foreach (NPMCredential credential in credentialManager.CredentialSet)
            {
                GUIStyle boxStyle = new GUIStyle();
                boxStyle.padding = new RectOffset(10, 10, 0, 0);

                EditorGUILayout.BeginHorizontal(boxStyle);
                EditorGUILayout.LabelField(credential.url);
                if (GUILayout.Button("Edit"))
                {
                    EditCredential(credential);
                    CloseWindow();
                }

                if (GUILayout.Button("Remove"))
                {
                    RemoveCredential(credential);
                    CloseWindow();
                }

                EditorGUILayout.EndHorizontal();

            }


            EditorGUILayout.EndScrollView();



            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Credential"))
            {
                AddCredential();
                CloseWindow();
            }

            if (GUILayout.Button("Close"))
            {
                CloseWindow();
            }

            EditorGUILayout.EndHorizontal();

        }

        private void CloseWindow()
        {
            Close();
            GUIUtility.ExitGUI();
        }



        private void RemoveCredential(NPMCredential credential)
        {
            credentialManager.RemoveCredential(credential.url);
            credentialManager.Write();
        }

        private void EditCredential(NPMCredential credential)
        {
            CredentialManager thisManager = credentialManager;
            CredentialEditorView credentialEditor = EditorWindow.GetWindow<CredentialEditorView>(true, "Edit credential", true);
            credentialEditor.Edit(credential, thisManager);
        }

        private void AddCredential()
        {
            CredentialManager thisManager = credentialManager;
            CredentialEditorView credentialEditor = EditorWindow.GetWindow<CredentialEditorView>(true, "Add credential", true);
            credentialEditor.CreateNew(thisManager);
        }
    }
}