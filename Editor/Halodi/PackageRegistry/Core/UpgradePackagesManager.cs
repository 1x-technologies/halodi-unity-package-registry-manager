using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using System.Threading;
using System.Collections.Generic;
using Artees.UnitySemVer;

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
            try
            {
                
                SemVer latestVersion = SemVer.Parse(GetLatestVersion(info));
                SemVer currentVersion = SemVer.Parse(info.version);

                if (currentVersion < latestVersion)
                {
                    UpgradeablePackages.Add(info);
                }
            }
            catch(System.Exception e)
            {
                Debug.LogError("Invalid version for package " + info.displayName + ". Current: " + info.version + ", Latest: " + GetLatestVersion(info));
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
            string latestVersion;
            if(info.source == PackageSource.Git)
            {
                latestVersion = GetLatestVersion(info);
            }
            else if (info.source == PackageSource.Registry)
            {
                latestVersion = info.name + "@" + GetLatestVersion(info);
            }
            else
            {
                error = "Invalid source";
                return false;
            }
            

            AddRequest request = UnityEditor.PackageManager.Client.Add(latestVersion);

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