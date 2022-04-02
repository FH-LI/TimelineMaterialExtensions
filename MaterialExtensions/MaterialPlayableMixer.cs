using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class MaterialPlayableMixer : PlayableBehaviour
{
    public MaterialControl[] controls = new MaterialControl[0];
    MaterialControlAction[] _actions;
    private DirectorWrapMode mode;
    public float endTime;

    public override void OnGraphStart(Playable playable)
    {
        var resolver = playable.GetGraph().GetResolver();

        if (_actions == null)
            _actions = new MaterialControlAction[controls.Length];

        for (var i = 0; i < controls.Length; i++)
        {
            Material mat = controls[i].targetMaterial.Resolve(resolver);
            if (mat)
                _actions[i] = MaterialControlAction.CreateAction(mat, controls[i].propertyName, controls[i].propertyType);
        }
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        for (var ci = 0; ci < controls.Length; ci++)
        {
            var ctrl = controls[ci];
            if (!ctrl.enabled) continue;

            var acc = 0.0f;

            for (var i = 0; i < playable.GetInputCount(); i++)
            {
                var clip = (ScriptPlayable<MaterialPlayableBehaviour>)playable.GetInput(i);
                acc += playable.GetInputWeight(i) * clip.GetBehaviour().GetValue(clip, ctrl);
            }

            if (acc > 0)
                _actions[ci]?.Invoke(Vector4.Lerp(ctrl.vector0, ctrl.vector1, acc));
            else if (mode != DirectorWrapMode.Hold || (mode == DirectorWrapMode.Hold && playable.GetTime() < endTime))
                _actions[ci]?.Invoke(ctrl.originValue);
        }
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        var resolver = playable.GetGraph().GetResolver();
        mode = (resolver as PlayableDirector).extrapolationMode;
        InitMaterialProperty();
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        InitMaterialProperty();
    }

    void InitMaterialProperty()
    {
        for (var ci = 0; ci < controls.Length; ci++)
        {
            var ctrl = controls[ci];
            if (!ctrl.enabled) continue;
            if (_actions != null && _actions.Length > ci)
                _actions[ci]?.Invoke(ctrl.originValue);
        }
    }
}
