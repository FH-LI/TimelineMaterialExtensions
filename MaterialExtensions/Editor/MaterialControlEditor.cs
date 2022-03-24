using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomPropertyDrawer(typeof(MaterialControl), true)]
public class MaterialControlEditor : PropertyDrawer
{
    Dictionary<string, MaterialControlInternalDrawer> _drawers = new Dictionary<string, MaterialControlInternalDrawer>();

    MaterialControlInternalDrawer GetCachedDrawer(SerializedProperty property)
    {
        MaterialControlInternalDrawer drawer;

        var path = property.propertyPath;
        _drawers.TryGetValue(path, out drawer);

        if (drawer == null)
        {
            drawer = new MaterialControlInternalDrawer(property);
            _drawers[path] = drawer;
        }

        return drawer;
    }

    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        var drawer = GetCachedDrawer(property);

        drawer.SetRect(rect);

        drawer.DrawCommonSettings();

        if (drawer.TargetMaterial != null)
        {
            drawer.DrawPropertySelector();
            drawer.DrawPropertyOptions();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return GetCachedDrawer(property).GetTotalHeight();
    }
}

sealed class MaterialControlInternalDrawer
{
    SerializedProperty _renderer;
    SerializedProperty _targetMaterial;
    SerializedProperty _propertyName;
    SerializedProperty _propertyType;
    SerializedProperty _curve;

    SerializedProperty _vector0;
    SerializedProperty _vector1;
    SerializedProperty _originValue;

    string[] _propertyNames = new string[0];
    string[] _propertyTypes;
    string[] _materialNames;

    string renderMaterialName;
    Material[] _renderMaterials;

    Rect _baseRect;
    Rect _rect;

    static readonly GUIContent _labelRender = new GUIContent("Render");
    static readonly GUIContent _labelMaterial = new GUIContent("Material");

    public MaterialControlInternalDrawer(SerializedProperty property)
    {
        _renderer = property.FindPropertyRelative("renderer");
        _targetMaterial = property.FindPropertyRelative("targetMaterial");
        _propertyName = property.FindPropertyRelative("propertyName");
        _propertyType = property.FindPropertyRelative("propertyType");
        _curve = property.FindPropertyRelative("curve");

        _vector0 = property.FindPropertyRelative("vector0");
        _vector1 = property.FindPropertyRelative("vector1");
        _originValue = property.FindPropertyRelative("originValue");
    }
    public Material TargetMaterial
    {
        get { return (Material)(_targetMaterial.exposedReferenceValue); }
    }

    public void SetRect(Rect rect)
    {
        _baseRect = _rect = rect;
        _rect.height = EditorGUIUtility.singleLineHeight;
    }

    public float GetTotalHeight()
    {
        return _rect.y - _baseRect.y;
    }

    void MoveRectToNextLine()
    {
        _rect.y += EditorGUIUtility.singleLineHeight + 2;
    }

    void MoveRectToNextLineInNarrowMode()
    {
        if (!EditorGUIUtility.wideMode)
            _rect.y += EditorGUIUtility.singleLineHeight;
    }

    public void DrawCommonSettings()
    {
        EditorGUI.PropertyField(_rect, _curve);
        MoveRectToNextLine();
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(_rect, _renderer, _labelRender);
        if (EditorGUI.EndChangeCheck())
        {
            _propertyName.stringValue = "";
            _propertyType.stringValue = "";
            Renderer renderer = _renderer.exposedReferenceValue as Renderer;
            if (renderer)
            {
                _materialNames = renderer.sharedMaterials.Select(m => m.name).ToArray();
                renderMaterialName = _materialNames[0];
                _renderMaterials = renderer.sharedMaterials;
            }
            else
            {
                _materialNames = null;
                _renderMaterials = null;
            }
        }
        MoveRectToNextLine();
        if (_materialNames != null && _materialNames.Length > 0)
        {
            EditorGUI.BeginChangeCheck();
            int matIndexindex = System.Array.IndexOf(_materialNames, renderMaterialName);
            matIndexindex = EditorGUI.Popup(_rect, "Materials", matIndexindex, _materialNames);
            _targetMaterial.exposedReferenceValue = _renderMaterials[matIndexindex];
            if (EditorGUI.EndChangeCheck())
            {
                _propertyName.stringValue = "";
                _propertyType.stringValue = "";
            }
        }
        else
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(_rect, _targetMaterial, _labelMaterial);
            if (EditorGUI.EndChangeCheck())
            {
                _propertyName.stringValue = "";
                _propertyType.stringValue = "";
            }
        }
        MoveRectToNextLine();
    }

    public void DrawPropertySelector()
    {
        CachePropertiesInTargetComponent();

        if (_propertyNames.Length == 0)
        {
            _propertyName.stringValue = "";
            _propertyType.stringValue = "";
        }
        else
        {
            EditorGUI.BeginChangeCheck();

            var index = System.Array.IndexOf(_propertyNames, _propertyName.stringValue);
            index = EditorGUI.Popup(_rect, "Property", index, _propertyNames);
            if (index < 0)
            {
                _propertyName.stringValue = "";
                _propertyType.stringValue = "";
            }
            else if (EditorGUI.EndChangeCheck())
            {
                _propertyName.stringValue = _propertyNames[index];
                _propertyType.stringValue = _propertyTypes[index];
                SavePropertyOriginValue();
            }

            MoveRectToNextLine();
        }
    }

    void SavePropertyOriginValue()
    {
        var so = new SerializedObject(TargetMaterial);
        var sp = so.FindProperty("m_SavedProperties");
        switch (_propertyType.stringValue)
        {
            case MaterialDefind.INT:
                sp = sp.FindPropertyRelative("m_Floats");
                break;
            case MaterialDefind.FLOAT:
                sp = sp.FindPropertyRelative("m_Floats");
                break;
            case MaterialDefind.COLOR:
                sp = sp.FindPropertyRelative("m_Colors");
                break;
        }
        for (int i = sp.arraySize - 1; i >= 0; i--)
        {
            string propName = sp.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue;
            if (_propertyName.stringValue.Equals(propName))
            {
                var prop = sp.GetArrayElementAtIndex(i).FindPropertyRelative("second");
                if (prop.propertyType == SerializedPropertyType.Integer)
                {
                    _originValue.vector4Value = new Vector4(prop.intValue, 0, 0, 0);
                }
                else if (prop.propertyType == SerializedPropertyType.Float)
                {
                    _originValue.vector4Value = new Vector4(prop.floatValue, 0, 0, 0);
                }
                else if (prop.propertyType == SerializedPropertyType.Color)
                {
                    _originValue.vector4Value = prop.colorValue;
                }
                break;
            }
        }
    }

    void CachePropertiesInTargetComponent()
    {
        var itr = (new SerializedObject(TargetMaterial)).GetIterator();

        var pnames = new List<string>();
        var labels = new List<string>();
        var fnames = new List<string>();
        var types = new List<string>();

        var so = new SerializedObject(TargetMaterial);
        var sp = so.FindProperty("m_SavedProperties.m_Floats");
        for (int i = sp.arraySize - 1; i >= 0; i--)
        {
            string floatPropName = sp.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue;
            pnames.Add(floatPropName);
            var propType = sp.GetArrayElementAtIndex(i).FindPropertyRelative("second").propertyType;
            if (propType == SerializedPropertyType.Integer)
                types.Add(MaterialDefind.INT);
            else if (propType == SerializedPropertyType.Float)
                types.Add(MaterialDefind.FLOAT);
        }

        sp = so.FindProperty("m_SavedProperties.m_Colors");
        for (int i = sp.arraySize - 1; i >= 0; i--)
        {
            string colorPropName = sp.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue;
            pnames.Add(colorPropName);
            types.Add(MaterialDefind.COLOR);
        }

        _propertyNames = pnames.ToArray();
        _propertyTypes = types.ToArray();

    }

    public void DrawPropertyOptions()
    {
        var pidx = System.Array.IndexOf(_propertyNames, _propertyName.stringValue);
        var type = pidx < 0 ? null : _propertyTypes[pidx];

        var v0 = _vector0.vector4Value;
        var v1 = _vector1.vector4Value;

        if (type == MaterialDefind.FLOAT)
        {
            EditorGUI.BeginChangeCheck();
            v0.x = EditorGUI.FloatField(_rect, "Float at 0", v0.x);
            if (EditorGUI.EndChangeCheck()) _vector0.vector4Value = v0;

            MoveRectToNextLine();

            EditorGUI.BeginChangeCheck();
            v1.x = EditorGUI.FloatField(_rect, "Float at 1", v1.x);
            if (EditorGUI.EndChangeCheck()) _vector1.vector4Value = v1;

            MoveRectToNextLine();
        }
        else if (type == MaterialDefind.INT)
        {
            EditorGUI.BeginChangeCheck();
            v0.x = EditorGUI.IntField(_rect, "Int at 0", (int)v0.x);
            if (EditorGUI.EndChangeCheck()) _vector0.vector4Value = v0;

            MoveRectToNextLine();

            EditorGUI.BeginChangeCheck();
            v1.x = EditorGUI.IntField(_rect, "Int at 1", (int)v1.x);
            if (EditorGUI.EndChangeCheck()) _vector1.vector4Value = v1;

            MoveRectToNextLine();
        }
        else if (type == MaterialDefind.COLOR)
        {
            EditorGUI.BeginChangeCheck();
            v0 = EditorGUI.ColorField(_rect, "Color at 0", v0);
            if (EditorGUI.EndChangeCheck()) _vector0.vector4Value = v0;

            MoveRectToNextLine();

            EditorGUI.BeginChangeCheck();
            v1 = EditorGUI.ColorField(_rect, "Color at 1", v1);
            if (EditorGUI.EndChangeCheck()) _vector1.vector4Value = v1;

            MoveRectToNextLine();
        }
    }

}