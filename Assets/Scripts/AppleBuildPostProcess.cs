using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_2018_3_OR_NEWER && UNITY_IOS)
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using System.IO;

public class AppleBuildPostProcess : MonoBehaviour
{
    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target != BuildTarget.iOS) return;

        var projectPath = PBXProject.GetPBXProjectPath(path);
        var project = new PBXProject();

        project.ReadFromString(File.ReadAllText(projectPath));
        var manager = new ProjectCapabilityManager(projectPath, "Entitlements.entitlements", null, project.GetUnityMainTargetGuid());
        manager.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
        manager.AddPushNotifications(false);
        manager.AddAssociatedDomains(new string[] { "applinks:app.quranreadinglive.com" });
        manager.WriteToFile();
    }
}
#endif