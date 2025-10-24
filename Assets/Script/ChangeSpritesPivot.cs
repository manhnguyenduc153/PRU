using UnityEngine;
using UnityEditor;

public class ChangeSpritesPivot : EditorWindow
{
    [MenuItem("Tools/Set All Sprites Pivot to Left")]
    static void SetPivotToLeft()
    {
        Texture2D texture = Selection.activeObject as Texture2D;
        if (texture == null) return;

        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

        if (ti.spriteImportMode == SpriteImportMode.Multiple)
        {
            SpriteMetaData[] spritesheet = ti.spritesheet;
            for (int i = 0; i < spritesheet.Length; i++)
            {
                spritesheet[i].alignment = (int)SpriteAlignment.LeftCenter;
                spritesheet[i].pivot = new Vector2(0f, 0.5f);
            }
            ti.spritesheet = spritesheet;
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
        }
    }
}