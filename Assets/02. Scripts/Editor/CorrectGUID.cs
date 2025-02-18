using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CorrectGUID : MonoBehaviour
{
    [MenuItem("Util/Clear GUID")]
    public static void ClearGUID()
    {
        List<string> scenePaths = new List<string>();
        foreach (UnityEditor.EditorBuildSettingsScene S in UnityEditor.EditorBuildSettings.scenes)
        {
            if (S.enabled)
                scenePaths.Add(S.path);
        }
        string changeString = "texture: {fileID: 0}";
        foreach (string str in scenePaths)
        {
            string tempPath = Application.dataPath.Replace("Assets", "") + str;
            string[] allLines = System.IO.File.ReadAllLines(tempPath);
            for (int i = 0; i < allLines.Length; ++i)
            {
                string lineStr = allLines[i];
                if (lineStr.Contains("fileID:") && lineStr.Contains("guid:  00000000000000000000000000000000"))
                {
                    lineStr.Remove(0, lineStr.Length);
                    lineStr += changeString;
                }
            }
            System.IO.File.WriteAllLines(tempPath, allLines);
        }
    }
}
