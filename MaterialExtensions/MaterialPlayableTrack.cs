using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(MaterialPlayableAsset))]
public class MaterialPlayableTrack : TrackAsset
{

    [HideInInspector] public MaterialPlayableMixer template = new MaterialPlayableMixer();
    IExposedPropertyTable resolver;
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var mixer = ScriptPlayable<MaterialPlayableMixer>.Create(graph, template, inputCount);
        resolver = mixer.GetGraph().GetResolver();
        return mixer;
    }

    public bool SetControlsObject(int index, Object go)
    {
        if (go == null || index > template.controls.Length - 1) return false;
        resolver.SetReferenceValue(template.controls[index].targetMaterial.exposedName, go);
        return true;
    }
}
