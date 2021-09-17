using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class CppInterface
{
    [DllImport("CppInterface")]
    public static extern int Add(int a, int b);
}
