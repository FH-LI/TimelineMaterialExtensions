using UnityEngine;
using UnityEngine.Events;

sealed public class MaterialDefind
{
    public const string FLOAT = "float";
    public const string INT = "int";
    public const string COLOR = "color";
}

abstract class MaterialControlAction
{
    public static MaterialControlAction CreateAction(Material mat, string propertyName, string type)
    {
        switch (type)
        {
            case MaterialDefind.INT:
                return new MaterialIntAction(mat, propertyName);
            case MaterialDefind.FLOAT:
                return new MaterialFloatAction(mat, propertyName);
            case MaterialDefind.COLOR:
                return new MaterialColorAction(mat, propertyName);
            default:
                return null;
        }
    }

    public abstract void Invoke(Vector4 param);

}

sealed class MaterialFloatAction : MaterialControlAction
{
    public System.Action<float> action;
    public MaterialFloatAction(Material mat, string propertyName)
    {
        action = (float f) =>
        {
            if (mat.GetFloat(propertyName) != f)
                mat.SetFloat(propertyName, f);
        };
    }

    public override void Invoke(Vector4 param) => action(param.x);
}

sealed class MaterialIntAction : MaterialControlAction
{
    public System.Action<int> action;
    public MaterialIntAction(Material mat, string propertyName)
    {
        action = (int i) =>
        {
            if (mat.GetInt(propertyName) != i)
                mat.SetInt(propertyName, i);
        };
    }

    public override void Invoke(Vector4 param) => action((int)param.x);
}

sealed class MaterialColorAction : MaterialControlAction
{
    public System.Action<Color> action;
    public MaterialColorAction(Material mat, string propertyName)
    {
        action = (Color c) =>
        {
            if (mat.GetColor(propertyName) != c)
                mat.SetColor(propertyName, c);
        };
    }

    public override void Invoke(Vector4 param) => action((Color)param);
}