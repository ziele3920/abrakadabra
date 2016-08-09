using UnityEngine;

namespace VRMind.Animations
{
    /// <summary>
    /// To create new layer:
    /// 1) Inherit your new layer class from VRMind.Animations.Layer
    /// 1.1) Create public fields of Group type 
    /// 2) Create prefab with your layer from empty GameObject that contain your layer script
    /// 3) Set field AnimatorLayerName as name of this layer in Animator
    /// 4) Create public field of your layer type in AnimatorManager
    /// 5) Drag and drop your layer to this field in inspector
    /// 6) Call method Init in methot InitLayers in AnimatorManager
    /// </summary>
    public interface ILayer
    {
        void Init(Animator anim);
    }
}
