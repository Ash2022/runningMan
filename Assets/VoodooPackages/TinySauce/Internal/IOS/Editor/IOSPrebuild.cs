﻿using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Globalization;
using Google;
using Voodoo.Tiny.Sauce.Internal.Editor.ApplePrivacy;

namespace Voodoo.Tiny.Sauce.Internal.Editor
{
    public class TinySaucePrebuildiOS : IPreprocessBuildWithReport
    {
        private const string TAG = "IOSPrebuild";
        private const float MinIosVersion = 11.0f;

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS) {
                return;
            }
            ApplePrivacyManifestHelper.ProcessManifest();
            PrepareResolver();
            PreparePlayerSettings();
        }

        private static void PrepareResolver()
        {
            // force Play Services Resolver to generate Xcode project and not workspace
            IOSResolver.PodfileGenerationEnabled = true;
            IOSResolver.PodToolExecutionViaShellEnabled = true;
            IOSResolver.AutoPodToolInstallInEditorEnabled = true;
            IOSResolver.PodfileStaticLinkFrameworks = false;
            IOSResolver.UseProjectSettings = true;
            IOSResolver.CocoapodsIntegrationMethodPref = IOSResolver.CocoapodsIntegrationMethod.Project;
        }

        private static void PreparePlayerSettings()
        {
            // enable insecure HTTP downloads (mandatory for few ad networks)
            PlayerSettings.iOS.allowHTTPDownload = true;
            //set iOS CPU Architecture to Universal
            PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 2);

            // set iOS compatible min version
            var changeMinVersion = true;
            if (float.TryParse(PlayerSettings.iOS.targetOSVersionString, out float iosMinVersion)) {
                if (iosMinVersion >= MinIosVersion) {
                    changeMinVersion = false;
                }
            }

            if (changeMinVersion) {
                PlayerSettings.iOS.targetOSVersionString = MinIosVersion.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}