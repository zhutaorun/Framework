using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Types
{
    Type1,
    Type2,
    Type3
}

public class NormalEnum : MonoBehaviour
{
    public Types type;
    // Start is called before the first frame update
    void Start()
    {
        type = Types.Type1;
        Debug.Log(type);

        type = Types.Type2;
        Debug.Log(type);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
