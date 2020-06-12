using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using System.Threading;
using System.Collections.Generic;

namespace Halodi.PackageRegistry.Core
{
    internal class UpgradePackagesManager
    {

        public List<UnityEditor.PackageManager.PackageInfo> UpgradeablePackages = new List<UnityEditor.PackageManager.PackageInfo>();

        private ListRequest request;

        public bool packagesLoaded = false;

        internal UpgradePackagesManager()
        {
            request = Client.List(false, false);
        }

        internal void Update()
        {
            if (!packagesLoaded && request.IsCompleted)
            {
                if (request.Status == StatusCode.Success)
                {
                    PackageCollection collection = request.Result;
                    foreach (UnityEditor.PackageManager.PackageInfo info in collection)
                    {
                        if (info.source == PackageSource.Git)
                        {
                            UpgradeablePackages.Add(info);
                        }
                        else if (info.source == PackageSource.Registry)
                        {
                            AddRegistryPackage(info);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Cannot query package manager for packages");
                }

                packagesLoaded = true;
            }
        }

        private void AddRegistryPackage(UnityEditor.PackageManager.PackageInfo info)
        {
            System.Version latestVersion = System.Version.Parse(GetLatestVersion(info));
            System.Version currentVersion = System.Version.Parse(info.version);

            if (currentVersion < latestVersion)
            {
                UpgradeablePackages.Add(info);
            }
        }

        public string GetLatestVersion(UnityEditor.PackageManager.PackageInfo info)
        {
            if (info.source == PackageSource.Git)
            {
                return info.packageId;
            }
            else
            {
                string latest = "";
                if (string.IsNullOrEmpty(info.versions.verified))
                {
                    latest = info.versions.latestCompatible;
                }
                else
                {
                    latest = info.versions.verified;
                }
                return latest;
            }
        }

        internal bool UpgradePackage(UnityEditor.PackageManager.PackageInfo info, ref string error)
        {
            string latestVersion = GetLatestVersion(info);

            AddRequest request = UnityEditor.PackageManager.Client.Add(info.packageId);

            while (!request.IsCompleted)
            {
                Thread.Sleep(100);
            }

            if (request.Status == StatusCode.Success)
            {
                return true;
            }
            else
            {
                error = request.Error.message;
                return false;
            }


        }


    }
}