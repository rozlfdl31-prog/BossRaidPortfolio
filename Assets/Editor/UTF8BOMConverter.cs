using UnityEditor;
using System.IO;
using System.Text;

public class UTF8BOMConverter : AssetPostprocessor {
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedAssetPaths) {
        foreach (string assetPath in importedAssets) {
            // .cs 파일만 타겟팅
            if (assetPath.EndsWith(".cs")) {
                string fullPath = Path.GetFullPath(assetPath);
                string content = File.ReadAllText(fullPath);

                // UTF-8 with BOM 인코딩으로 다시 강제 저장
                UTF8Encoding utf8WithBOM = new UTF8Encoding(true);
                File.WriteAllText(fullPath, content, utf8WithBOM);
            }
        }
    }
}