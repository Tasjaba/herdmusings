using UnityEditor;
using UnityEngine;
using System.Collections;

public class NormalTextureTextureProcessor : AssetPostprocessor {
	void OnPostprocessTexture(Texture2D texture) {

		string lowerCaseAssetPath = assetPath.ToLower();

		if (lowerCaseAssetPath.IndexOf("_n.") >= 0 
            || lowerCaseAssetPath.IndexOf("_n_pad.") >= 0
            || lowerCaseAssetPath.IndexOf("_normal.") >= 0
        )
		{
			Debug.Log ("Recognizing normal image: " + assetPath);

			TextureImporter importer = assetImporter as TextureImporter;
			if (importer.textureType != TextureImporterType.Bump)
			{
				Debug.Log ("Fixing texture type to be normal");
				importer.textureType = TextureImporterType.Bump;
				importer.SaveAndReimport();
			}
		}
	}
}