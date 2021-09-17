using UnityEngine;
using System.Text;
using System.IO;
using UnityEditor;
using System.Reflection;

//统计Shader变体数量收集统计小工具，批量查询一下游戏中的Shader的变体数量
//原理：收集所有Shader文件，通过反射拿到UnityEditor.ShaderUtil.GetVariantCount,获取到变体数量
//问题：当前Shader只是编辑器状态下的，如果想要获得针对平台的检测使用ShaderControl
//ShaderControl插件查看Shader的变体数量，冗余关键字，查看哪些材质引用了Shader的关键字
//插件地址:https://assetstore.unity.com/packages/vfx/shaders/shader-control-74817#content
public class ShaderVariantCount
{
    [MenuItem("Tools/ShaderVariantCount")]
    public static void GetAllShaderVariantCount()
    {
        Assembly asm = Assembly.LoadFile(@"D:\Unity\Unity2019.4.15f1\Editor\Data\Managed\UnityEditor.dll");
        System.Type t2 = asm.GetType("UnityEditor.ShaderUtil");
        MethodInfo method = t2.GetMethod("GetVariantCount", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var shaderList = AssetDatabase.FindAssets("t:Shader");

        var output = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
        string pathF = string.Format("{0}/ShaderVariantCount.csv",output);

        FileStream fs = new FileStream(pathF, FileMode.Create, FileAccess.Write);
        StreamWriter sw = new StreamWriter(fs,Encoding.UTF8);

        EditorUtility.DisplayProgressBar("Shader统计文件","正在写入统计文件中...",0f);
        int ix = 0;
        sw.WriteLine("ShaderFile,VariantCount");
        foreach(var i in shaderList)
        {
            EditorUtility.DisplayProgressBar("Shader统计文件", "正在写入统计文件夹中...", ix / shaderList.Length);
            var path = AssetDatabase.GUIDToAssetPath(i);
            Shader s = AssetDatabase.LoadAssetAtPath(path, typeof(Shader)) as Shader;
            var variantCount = method.Invoke(null, new System.Object[] { s, true });
            sw.WriteLine(path+","+variantCount.ToString());
            ++ix;
        }

        EditorUtility.ClearProgressBar();
        sw.Close();
        fs.Close();
       
    }

}
