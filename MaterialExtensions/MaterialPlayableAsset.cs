using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class MaterialPlayableAsset : PlayableAsset, ITimelineClipAsset
{
    [HideInInspector]public MaterialPlayableBehaviour template = new MaterialPlayableBehaviour();

    public ClipCaps clipCaps
    {
        get
        {
            return ClipCaps.Blending |
                   ClipCaps.ClipIn |
                   ClipCaps.Extrapolation |
                   ClipCaps.Looping |
                   ClipCaps.SpeedMultiplier;
        }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        return ScriptPlayable<MaterialPlayableBehaviour>.Create(graph, template);
    }
}
