using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class AtlasManager : Singleton<AtlasManager>
{
    public SpriteAtlas projectThumbnailAtlas;

    private Dictionary<string, Texture2D> loadedAtlases = new Dictionary<string, Texture2D>();

    public IEnumerator LoadAtlas(string atlasName)
    {
        if (!loadedAtlases.ContainsKey(atlasName))
        {
            // 리소스 로드를 위한 코루틴
            ResourceRequest request = Resources.LoadAsync<Texture2D>(atlasName);
            yield return request;

            if (request.asset != null)
            {
                loadedAtlases[atlasName] = request.asset as Texture2D;
            }
        }
    }

    public void UnloadAtlas(string atlasName)
    {
        if (loadedAtlases.TryGetValue(atlasName, out var atlas))
        {
            Resources.UnloadAsset(atlas);
            loadedAtlases.Remove(atlasName);
            // 가비지 컬렉션 고려
            Resources.UnloadUnusedAssets();
        }
    }
}
