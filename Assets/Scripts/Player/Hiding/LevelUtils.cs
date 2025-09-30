// Scripts/Player/LevelUtils.cs
using UnityEngine;
using UnityEngine.Rendering;

public static class LevelUtils
{
    public const int OrderStep = 100;

    // set player's gameplay level, renderer order and optional physics layer
    public static void SetEntityLevel(GameObject obj, int level, int localOffset = 0)
    {
        // ensure tag exists
        var tag = obj.GetComponent<VisibilityLevelTag>();
        if (tag == null) tag = obj.AddComponent<VisibilityLevelTag>();
        tag.level = level;

        // sorting group (grouped objects)
        var sg = obj.GetComponent<SortingGroup>();
        if (sg != null) sg.sortingOrder = level * OrderStep + localOffset;

        // all sprite renderers under object
        var srs = obj.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < srs.Length; i++)
            srs[i].sortingOrder = level * OrderStep + localOffset;

        // optional: switch physics layer if you created Level0/Level1 layers
        int layer = LayerMask.NameToLayer("Level" + level);
        if (layer >= 0) obj.layer = layer;
    }
}
