using UnityEditor;
using UnityEngine;
using System.Reflection;


public static class InnerIcon 
{
    private static MethodInfo _loadInconMethod;

    private const string _Trash = "TreeEditor.Trash";
    private const string _Play = "PlayButton";
    private const string _CreateAddNew = "CreateAddNew";
    private const string _Save = "SaveAs";
    private const string _PrevKey = "Animation.PrevKey";
    private const string _NextKey = "Animation.NextKey";
    private const string _FirstKey = "Animation.FirstKey";
    private const string _LastKey = "Animation.LastKey";
    private const string _PauseButton = "PasuseButton";
    private const string _Duplicate = "TreeEditor.Duplicate";

    private static Texture trash;
    private static Texture play;
    private static Texture createAddNew;
    private static Texture save;
    private static Texture prevKey;
    private static Texture nextKey;
    private static Texture firstKey;
    private static Texture lastKey;
    private static Texture pause;
    private static Texture duplicate;
    public static Texture Trash { get { return LoadInnerIcon(_Trash,ref trash); } }
    public static Texture Play { get { return LoadInnerIcon(_Play, ref play); } }

    public static Texture CreateAddNew { get { return LoadInnerIcon(_CreateAddNew, ref createAddNew); } }
    public static Texture Save { get { return LoadInnerIcon(_Save,ref save); } }

    public static Texture PrevKey { get { return LoadInnerIcon(_PrevKey,ref prevKey); } }

    public static Texture NextKey { get { return LoadInnerIcon(_NextKey,ref nextKey); } }

    public static Texture FirstKey { get { return LoadInnerIcon(_FirstKey, ref firstKey); } }

    public static Texture LastKey { get { return LoadInnerIcon(_LastKey,ref lastKey); } }

    public static Texture Pause { get { return LoadInnerIcon(_PauseButton,ref pause); } }

    public static Texture Duplicate { get { return LoadInnerIcon(_Duplicate,ref duplicate); } }

    private static Texture LoadInnerIcon(string name,ref Texture texture)
    {
        if (texture) return texture;
        if(_loadInconMethod == null)
        {
            _loadInconMethod = typeof(EditorGUIUtility).GetMethod("LoadIcon",BindingFlags.Static| BindingFlags.NonPublic);
        }
        if (_loadInconMethod == null) return null;

        texture = _loadInconMethod.Invoke(null, new object[] { name }) as Texture;
        return texture;
    }
}
