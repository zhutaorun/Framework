using UnityEditor;

public class BigEnumEditor : Editor
{
    private SpecialEnum se;
    private SerializedObject obj;//序列化SpecialEnum

    private void OnEnable()
    {
        obj = new SerializedObject(target);
        se = target as SpecialEnum;
    }

    public override void OnInspectorGUI()
    {
        obj.Update();
        EditorGUI.BeginChangeCheck();//开始检测更新
        se.bigType = (BigTypes)EditorGUILayout.EnumMaskField("类型选择",se.bigType);
        EditorGUI.EndChangeCheck();

        //结束检查是否有修改
        //if (.changed)
        //{
        //    obj.ApplyModifiedProperties();
        //}

    }
}
