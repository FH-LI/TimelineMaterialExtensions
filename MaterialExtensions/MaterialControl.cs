using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MaterialControl
{
    public ExposedReference<Renderer> renderer;
    public ExposedReference<Material> targetMaterial;
    public string propertyName;
    public string propertyType;
    public bool enabled;

    public AnimationCurve curve = new AnimationCurve();

    public Vector4 vector0;
    public Vector4 vector1 = Vector3.one;
    [HideInInspector] public Vector4 originValue = Vector4.zero;
}