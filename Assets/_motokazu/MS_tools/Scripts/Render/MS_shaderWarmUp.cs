using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MS_shaderWarmUp : MonoBehaviour
{
    /// <summary>
    /// スタート時に指定ShaderCollections内のシェーダーをすべてコンパイルする
    /// </summary>
    [SerializeField] ShaderVariantCollection ShaderVariantCollection;
    void Start()
    {
        ShaderVariantCollection.WarmUp();
    }
}
