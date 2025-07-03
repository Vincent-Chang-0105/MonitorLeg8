using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChangeRenderer : MonoBehaviour
{
    public UniversalRenderPipelineAsset underwaterPipelineAsset;

    void OnEnable()
    {
        GraphicsSettings.renderPipelineAsset = underwaterPipelineAsset;
    }
}   