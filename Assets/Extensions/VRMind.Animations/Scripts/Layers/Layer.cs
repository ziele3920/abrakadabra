
using UnityEngine;


namespace VRMind.Animations
{
    public class Layer : MonoBehaviour, ILayer
    {
        [HideInInspector]
        public int number {   get;  private set;  }
        public string animatorLayerName;

        public void Init(Animator anim)
        {
            number = anim.GetLayerIndex(animatorLayerName);
            if (number == -1)
                Debug.LogError("Animator nicht zawiera layer " + animatorLayerName + "check your layer and animator");
        }

    }
}