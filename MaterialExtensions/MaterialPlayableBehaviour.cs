using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class MaterialPlayableBehaviour : PlayableBehaviour
{
    public float GetValue(Playable playable, MaterialControl control)
    {
        float percent = (float)playable.GetTime() / (float)playable.GetDuration();
        float weight = 1;
        if (control.curve.length > 0)
            weight = control.curve.Evaluate(percent);
        return percent * weight;
    }
}
