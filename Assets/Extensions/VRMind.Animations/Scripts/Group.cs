using UnityEngine;


namespace VRMind.Animations
{
    /// <summary>
    /// To create ne Group you have to:
    /// 1) Create empty GameObject on the scene
    /// 2) Add Group component
    /// 3) Drag and drop this GameObject to Assets/Resources/AnimationGroups/YourLayer/ as a prefab
    /// 4) Drag and drop your prefeab into your layers prefabs
    /// </summary>
    public class Group : MonoBehaviour
    {
        public AnimationClip[] clips;
    }
}
