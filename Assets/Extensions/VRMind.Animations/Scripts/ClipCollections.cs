using UnityEngine;

namespace VRMind.Animations
{

    static public class ClipCollections
    {
        static public AnimationClip[] LoopBackAndForth(AnimationManager manager)
        {
            AnimationClip[] clips = {
                manager.FullBodyLayer.Walk.clips[0],
                manager.FullBodyLayer.Walk.clips[0],
                manager.FullBodyLayer.StandTurnLeft.clips[0],
            };
            return clips;
        }

        static public AnimationClip[] BTStdWalk(AnimationManager manager)
        {
            AnimationClip[] clips = {
                manager.FullBodyLayer.WalkLeft.clips[0],
                manager.FullBodyLayer.Walk.clips[0],
                manager.FullBodyLayer.WalkRight.clips[0],
                manager.FullBodyLayer.StandTurnLeft.clips[0],
                manager.FullBodyLayer.StandIdle.clips[0],
                manager.FullBodyLayer.StandTurnRight.clips[0]                
            };
            return clips;
        }
    }
}
