using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildAndroid
{
    [MenuItem("Build/Build Android APK")]
    public static void Build()
    {
        const string scenePath="Assets/Scenes/TrainingRange.unity";
        Directory.CreateDirectory("Assets/Scenes");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var root=new GameObject("Training Range"); root.AddComponent<TrainingRange>();
        EditorSceneManager.SaveScene(scene,scenePath);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scenePath,true)};
        EditorUserBuildSettings.buildAppBundle=false;
        EditorUserBuildSettings.androidBuildSystem=AndroidBuildSystem.Gradle;
        PlayerSettings.productName="Mobil Hedefleme Laboratuvari";
        PlayerSettings.applicationIdentifier="com.mancheasebey.trainingrange";
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android,ScriptingImplementation.IL2CPP);
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{scenePath},locationPathName="Build/TrainingRange.apk",target=BuildTarget.Android,options=BuildOptions.None});
        if(report.summary.result!=BuildResult.Succeeded) throw new System.Exception("Android build failed: "+report.summary.result);
        Debug.Log("APK: "+Path.GetFullPath("Build/TrainingRange.apk"));
    }
}