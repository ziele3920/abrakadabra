using UnityEngine;
using System.Collections.Generic;

namespace VRMind.Animations
{ 

    public struct SeqCell
    {
        public bool newClip;
        public bool wait;
        public bool playOnce;
        public bool moveNext;
        public int coordNo;
        public Layer layer;
        public AnimationClip clip;
        public float time;
        public float exitTime;
        public float inTime;

        /// <summary>
        /// Stays in actual point over waitingTime seconds
        /// </summary>
        /// <param name="waitingTime"></param>
        public SeqCell(Layer layer, float waitingTime)
        {
            wait = true;
            newClip = false;
            coordNo = 0;
            clip = null;
            time = waitingTime;
            playOnce = false;
            exitTime = -1;
            inTime = -1;
            moveNext = false;
            this.layer = layer;
        }

        /// <summary>
        /// move to destination point over duration seconds
        /// if moveNext it moves to the next point
        /// </summary>
        /// <param name="destinationPoint"></param>
        /// <param name="duration"></param>
        public SeqCell(Layer layer, int destinationPoint, float duration, bool moveNext = false)
        {
            wait = false;
            newClip = false;
            coordNo = destinationPoint;
            clip = null;
            time = duration;
            exitTime = -1;
            inTime = -1;
            playOnce = false;
            this.moveNext = moveNext;
            this.layer = layer;
        }

        /// <summary>
        /// load new clip to point in blend tree
        /// </summary>
        /// <param name="clipToLad"></param>
        /// <param name="point"></param>
        public SeqCell(Layer layer, AnimationClip clipToLad, int point)
        {
            wait = false;
            newClip = true;
            coordNo = point;
            clip = clipToLad;
            time = 0;
            playOnce = false;
            exitTime = -1;
            inTime = -1;
            moveNext = false;
            this.layer = layer;
        }

        /// <summary>
        /// load new clip to next point in blend tree
        /// </summary>
        /// <param name="clipToLad"></param>
        public SeqCell(Layer layer, AnimationClip clipToLad)
        {
            wait = false;
            newClip = true;
            coordNo = -1;
            clip = clipToLad;
            time = 0;
            playOnce = false;
            exitTime = -1;
            inTime = -1;
            moveNext = false;
            this.layer = layer;
        }
        /// <summary>
        /// load new clip to next point in blend tree and autoamatically move over the blen tree
        /// </summary>
        /// <param name="clipToLad"></param>
        /// <param name="inTime"></param>
        /// <param name="exitTime"></param>
        public SeqCell(Layer layer, AnimationClip clipToLad, float inTime, float exitTime)
        {
            playOnce = true;
            wait = false;
            newClip = true;
            coordNo = -5;
            clip = clipToLad;
            time = -1;
            float tmpInT = (clip.length > (inTime + exitTime)) ? inTime : clip.length / 2;
            this.inTime =  Mathf.Clamp(tmpInT, 0, AnimationManager.maxInTime);
            float tmpExT = (clip.length > (exitTime + inTime)) ? exitTime : clip.length / 2;
            this.exitTime =  Mathf.Clamp(tmpInT, 0, AnimationManager.maxInTime);
            moveNext = false;
            this.layer = layer;
        }
    }

    public class BehaviourSequences
    {
        
        public List<SeqCell> BTBackAndForthSC(AnimationManager manager)
        {
            List<SeqCell> list = new List<SeqCell>();
            list.Add(new SeqCell(manager.FullBodyLayer, 1, 2));
            list.Add(new SeqCell(manager.FullBodyLayer, 3));
            list.Add(new SeqCell(manager.FullBodyLayer, 0, 1));
            list.Add(new SeqCell(manager.FullBodyLayer, 2));
            list.Add(new SeqCell(manager.FullBodyLayer, 1, 1));
            list.Add(new SeqCell(manager.FullBodyLayer, 3));
            list.Add(new SeqCell(manager.FullBodyLayer, manager.FullBodyLayer.StandTurnRight.clips[0], 4));
            list.Add(new SeqCell(manager.FullBodyLayer, 4, 3));

            return list;
        }

        public List<SeqCell> Test(AnimationManager mg)
        {
            FullBodyLayer layer = mg.FullBodyLayer;
            List<SeqCell> list = new List<SeqCell>();
            list.Add(SetAutoClipFractionTime(layer, layer.Walk.clips[0], 0.45f, 0.45f));
            list.Add(WaitForSecondsInCurrentPoint(layer, 3));
            list.Add(SetAutoClipFractionTime(layer, layer.StandTurnLeft.clips[0], 0.45f, 0.45f));
            list.Add(WaitForSecondsInCurrentPoint(layer, 1));
            list.Add(SetAutoClipFractionTime(layer, layer.Walk.clips[0], 0.45f, 0.45f));
            list.Add(SetAutoClipFractionTime(layer, layer.Walk.clips[0], 0.45f, 0.45f));
            list.Add(SetAutoClipFractionTime(layer, layer.Walk.clips[0], 0.45f, 0.45f));
            list.Add(SetAutoClipFractionTime(layer, layer.Walk.clips[0], 0.45f, 0.45f));
            list.Add(SetAutoClipFractionTime(layer, layer.StandTurnLeft.clips[0], 0.45f, 0.45f));
            return list;
        }

        SeqCell WaitForSecondsInCurrentPoint(Layer layer, float time)
        {
            return new SeqCell(layer, time);
        }

        SeqCell MoveToPointOverTime(Layer layer, int destinationPoint, float duration)
        {
            return new SeqCell(layer, destinationPoint, duration);
        }

        SeqCell SetClipInPoint(Layer layer, AnimationClip clipToLoad, int point)
        {
            return new SeqCell(layer, clipToLoad, point);
        }

        SeqCell SetNextClip(Layer layer, AnimationClip clipToLoad)
        {
            return new SeqCell(layer, clipToLoad);
        }

        SeqCell MoveToNextPoint(Layer layer, float duration)
        {
            return new SeqCell(layer, -1, duration, true);
        }

        SeqCell SetAutoClip(Layer layer, AnimationClip clipToLoad, float inTime, float exitTime)
        {
            return new SeqCell(layer, clipToLoad, inTime, exitTime);
        }

        SeqCell SetAutoClipFractionTime(Layer layer, AnimationClip clipToLoad, float fractionInTime, float fractionExitTime)
        {
            return new SeqCell(layer, clipToLoad, fractionInTime * clipToLoad.length, fractionExitTime * clipToLoad.length);
        }
    }
}
// Blend Tree Representation: //
//                            //
//     0 --> 1 --> 2          //   
//                 |          //
//                 V          //
//     5 <-- 4 <-- 3          //
//     |                      //
//     V                      //
//     6 --> 7 --> 8          //
//                            //
// // // // // // // // // // //