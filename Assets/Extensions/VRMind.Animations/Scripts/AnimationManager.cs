using UnityEngine;
using System.Collections.Generic;


namespace VRMind.Animations
{
    [RequireComponent(typeof(Animator))]
    public class AnimationManager : MonoBehaviour, IAnimationManager
    {
        /// <summary>
        /// first numbers have to be elements of BendTree in correct order:
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
        /// </summary>
        enum AnimatorInterface
        {
            LeftUpBT,
            CenterUpBT,
            RightUpBT,
            RightCenterBT,
            CenterBT,
            LeftCenterBT,
            LeftBottomBT,
            CenterBottomBT,
            RightBottomBT,
            State0,
            State1,
            State2
        }

        public FullBodyLayer FullBodyLayer;
        public int layersNumber = 1;

        Dictionary<AnimatorInterface, int> clipNo; //to translate clip position in blend tree to number in cliip array
        Dictionary<int, Vector2> pointBT; //to translate number of point in blend tree to x,y coordinates
        readonly int btPointsLength = 9; //Number of points in blend tree


        public Animator myAnimator { get; private set; }
        AnimatorOverrideController overridingAnimator;

        Posuwacz posuwacz; // to translate point on blend tree's plane
        BehaviourSequences bhSequence; // to get behaviours

        List<SeqCell>[] sequences;
        bool[] loopSequence;
        int[] sequenceCounter;
        int[] currentBtPos;
        float[] lastExitTime;
        int direction = 1;
        int defaultBtPos = 4;
        public static float maxInTime = 3;
        public static float maxExitTime = 3;




        public void Start()
        {
            InitializeFields();
        }

        void Update()
        {
            Examples();
        }

        private void Examples()
        {
            if (Input.GetKeyDown(KeyCode.T))
                SetSequence(FullBodyLayer, bhSequence.Test(this));

            if (Input.GetKeyDown(KeyCode.A))
                SetSequence(FullBodyLayer, bhSequence.BTBackAndForthSC(this));
            

            if (Input.GetKeyDown(KeyCode.L)) {
                SetSimpleLoop(ClipCollections.LoopBackAndForth(this));
                StartLoopState(FullBodyLayer);
            }

            if (Input.GetKeyDown(KeyCode.I))
                posuwacz.TranslateConst(FullBodyLayer, 0, 1, 6);

            if (Input.GetKeyDown(KeyCode.K))
                posuwacz.TranslateConst(FullBodyLayer, 0, 0, 1.8f);


            if (Input.GetKeyDown(KeyCode.UpArrow))
                myAnimator.SetFloat("FullBodyLayer.y", Mathf.Clamp01(myAnimator.GetFloat("FullBodyLayer.y") + 0.1f));


            if (Input.GetKeyDown(KeyCode.DownArrow))
                myAnimator.SetFloat("FullBodyLayer.y", Mathf.Clamp01(myAnimator.GetFloat("FullBodyLayer.y") - 0.1f));

            if (Input.GetKeyDown(KeyCode.LeftArrow))
                myAnimator.SetFloat("FullBodyLayer.x", Mathf.Clamp(myAnimator.GetFloat("FullBodyLayer.x") - 0.1f, -1, 1));


            if (Input.GetKeyDown(KeyCode.RightArrow))
                myAnimator.SetFloat("FullBodyLayer.x", Mathf.Clamp(myAnimator.GetFloat("FullBodyLayer.x") + 0.1f, -1, 1));


            if (Input.GetKeyDown(KeyCode.W))
                ChangeClip(FullBodyLayer.StandIdle.clips[0].name,
                           FullBodyLayer.Walk.clips[0]);
            if (Input.GetKeyDown(KeyCode.S))
                ChangeClip(FullBodyLayer.Walk.clips[0].name,
                           FullBodyLayer.StandIdle.clips[0]);
        }


        public Vector2 GetCurrentBtPos(Layer layer)
        {
            return new Vector2(myAnimator.GetFloat(layer.animatorLayerName + ".x"),
                               myAnimator.GetFloat(layer.animatorLayerName + ".y"));
        }
        /// <summary>
        /// is start animation loop over BlendTree
        /// to propertly working of loop the StatesLoop must be stopped
        /// </summary>
        /// <param name="layer">layer</param>
        void StartLoopBT(Layer layer)
        {
            loopSequence[layer.number] = true;
            sequenceCounter[layer.number] = 0;
        }

        /// <summary>
        /// is stop animation loop over BlendTree
        /// </summary>
        /// <param name="layer">layer</param>
        void StopLoopBT(Layer layer)
        {
            loopSequence[layer.number] = false;
        }

        /// <summary>
        /// is start animation loop (between three states)
        /// jum from blend tree to the states
        /// </summary>
        /// <param name="layerNo">number of layer</param>
        void StartLoopState(Layer layer)
        {
            myAnimator.SetBool(layer.animatorLayerName + ".Loop", true);
        }

        /// <summary>
        /// it stop animation loop
        /// jump from states to blend tree
        /// </summary>
        /// <param name="layerNo"> number of layer </param>
        void StopLoopState(Layer layer)
        {
            myAnimator.SetBool(layer.animatorLayerName + ".Loop", false);
        }

        /// <summary>
        /// load three clips into the states
        /// </summary>
        /// <param name="clips"> array of clips in correct order </param>
        void SetSimpleLoop(AnimationClip[] clips)
        {
            ChangeClip(clipNo[AnimatorInterface.State0], clips[0]);
            ChangeClip(clipNo[AnimatorInterface.State1], clips[1]);
            ChangeClip(clipNo[AnimatorInterface.State2], clips[2]);
        }

        /// <summary>
        /// load clips into blend tree
        /// if some clip is null the StandIdle clip is going to load
        /// </summary>
        /// <param name="clips">array of clips in correct order: from leftup to right and down (as read text)</param>
        void LoadBTClips(AnimationClip[] clips)
        {
            int i;
            for (i = 0; i < clips.Length; ++i) {
                if (clips[i] == null)
                    ChangeClip(clipNo[(AnimatorInterface)i],
                        FullBodyLayer.StandIdle.clips[0]);
                else
                    ChangeClip(clipNo[(AnimatorInterface)i], clips[i]);
            }
            for (int j = i; j < btPointsLength; ++j)
                ChangeClip(clipNo[(AnimatorInterface)j],
                    FullBodyLayer.StandIdle.clips[0]);
        }


        /// <summary>
        /// it find Animator component, create overriding animator to change clips and
        /// initialize animator interface
        /// </summary>
        void LoadAnimators()
        {
            myAnimator = GetComponent<Animator>();
            overridingAnimator = new AnimatorOverrideController();
            InitializeAnimatorInterface();
            overridingAnimator.runtimeAnimatorController = myAnimator.runtimeAnimatorController;
        }

        /// <summary>
        /// Change clip named oldClipName in Animator
        /// </summary>
        /// <param name="oldClipName"></param>
        /// <param name="newClip"></param>
        void ChangeClip(string oldClipName, AnimationClip newClip)
        {
            overridingAnimator[oldClipName] = newClip;
            UploadNewAnimations();
        }

        /// <summary>
        /// Change clip on oldClipNo position in Animator's clips list
        /// </summary>
        /// <param name="oldClipName"></param>
        /// <param name="newClip"></param>
        void ChangeClip(int oldClipNo, AnimationClip newClip)
        {
            overridingAnimator[myAnimator.runtimeAnimatorController.animationClips[oldClipNo].name] = newClip;
            UploadNewAnimations();
        }

        void UploadNewAnimations()
        {
            myAnimator.runtimeAnimatorController = overridingAnimator;
            overridingAnimator = new AnimatorOverrideController();
            overridingAnimator.runtimeAnimatorController = myAnimator.runtimeAnimatorController;
        }

        void InitializeFields()
        {
            LoadAnimators();
            InitLayers();
            LoadPosuwacz();

            bhSequence = new BehaviourSequences();
            LoadBTClips(ClipCollections.BTStdWalk(this));

            sequences = new List<SeqCell>[layersNumber];
            sequenceCounter = new int[layersNumber];
            currentBtPos = new int[layersNumber];
            lastExitTime = new float[layersNumber];
            loopSequence = new bool[layersNumber];
            for (int i = 0; i < loopSequence.Length; ++i) {
                sequences[i] = new List<SeqCell>();
                loopSequence[i] = false;
                currentBtPos[i] = defaultBtPos;
            }
            InitPointBtDic();
        }

        /// <summary>
        /// set up new sequence over Blend Tree and start it after load
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="seq"></param>
        /// <param name="loop"></param>
        void SetSequence(Layer layer, List<SeqCell> seq, bool loop = false)
        {
            sequences[layer.number] = seq;
            if (loop) {
                loopSequence[layer.number] = true;
                sequenceCounter[layer.number] = 0;
            }
            DoSequence(layer.number);
        }

        /// <summary>
        /// add new elements to Blend Tree sequence list
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="seq"></param>
        void AddToSequence(Layer layer, List<SeqCell> seq)
        {
            sequences[layer.number].AddRange(seq);
        }

        void DoSequence(int layerNo)
        {
            if (sequences[layerNo].Count > 0) {
                SeqCell currentCell;
                if (loopSequence[layerNo]) {
                    currentCell = sequences[layerNo][sequenceCounter[layerNo]++];
                    if (sequenceCounter[layerNo] == sequences[layerNo].Count)
                        sequenceCounter[layerNo] = 0;
                }
                else {
                    currentCell = sequences[layerNo][0];
                    sequences[layerNo].RemoveAt(0);
                }
                ServeSeqCell(layerNo, ref currentCell);
            }
        }

        void ServeSeqCell(int layerNo, ref SeqCell cell)
        {
            if (cell.wait)
                posuwacz.Wait(layerNo, cell.time);
            else if (cell.newClip) {
                if (cell.playOnce)
                {
                    SetDirection(ref layerNo);
                    ChangeClip(clipNo[(AnimatorInterface)(currentBtPos[layerNo]+direction)], cell.clip);
                    if (cell.coordNo == -1) {
                        DoSequence(layerNo);
                        return;
                    }
                    if (cell.clip.length > 2 * cell.time) 
                        sequences[layerNo].Insert(0, new SeqCell(cell.layer, cell.clip.length - cell.inTime - cell.exitTime));

                    sequences[layerNo].Insert(0, new SeqCell(cell.layer, currentBtPos[layerNo]+direction, (cell.inTime + lastExitTime[layerNo])/2));
                    lastExitTime[layerNo] = cell.exitTime;
                }
                else if(cell.coordNo == -1)
                    ChangeClip(currentBtPos[cell.layer.number], cell.clip);
                else
                    ChangeClip(clipNo[(AnimatorInterface)cell.coordNo], cell.clip);
                DoSequence(layerNo);
            }
            else {
                Vector2 coord = pointBT[cell.coordNo];
                posuwacz.TranslateConst(FullBodyLayer, coord.x, coord.y, cell.time);
                currentBtPos[layerNo] = cell.coordNo;
            }
        }

        void SetDirection(ref int layerNo)
        {
            if (currentBtPos[layerNo] == (btPointsLength-1))
                direction = -1;
            else if (currentBtPos[layerNo] == 0)
                direction = 1;
        }


        void InitializeAnimatorInterface()
        {
            clipNo = new Dictionary<AnimatorInterface, int>();
            myAnimator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("AnimationControllers/Initializer");
            AnimationClip[] clips = myAnimator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; ++i) {
                switch (clips[i].name) {
                    case "s0":
                        clipNo.Add(AnimatorInterface.State0, i);
                        break;
                    case "s1":
                        clipNo.Add(AnimatorInterface.State1, i);
                        break;
                    case "s2":
                        clipNo.Add(AnimatorInterface.State2, i);
                        break;
                    case "bt-11":
                        clipNo.Add(AnimatorInterface.LeftUpBT, i);
                        break;
                    case "bt01":
                        clipNo.Add(AnimatorInterface.CenterUpBT, i);
                        break;
                    case "bt11":
                        clipNo.Add(AnimatorInterface.RightUpBT, i);
                        break;
                    case "bt-10":
                        clipNo.Add(AnimatorInterface.LeftCenterBT, i);
                        break;
                    case "bt00":
                        clipNo.Add(AnimatorInterface.CenterBT, i);
                        break;
                    case "bt10":
                        clipNo.Add(AnimatorInterface.RightCenterBT, i);
                        break;
                    case "bt-1-1":
                        clipNo.Add(AnimatorInterface.LeftBottomBT, i);
                        break;
                    case "bt0-1":
                        clipNo.Add(AnimatorInterface.CenterBottomBT, i);
                        break;
                    case "bt1-1":
                        clipNo.Add(AnimatorInterface.RightBottomBT, i);
                        break;
                    default:
                        Debug.LogError("Unrecognized clip in Initialize AnimationController");
                        break;
                }
            }
        }

        void LoadPosuwacz()
        {
            gameObject.AddComponent(typeof(Posuwacz));
            posuwacz = GetComponent<Posuwacz>();
            posuwacz.Init(layersNumber, myAnimator);
            posuwacz.Moved += DoSequence;
        }

        void InitPointBtDic()
        {
            pointBT = new Dictionary<int, Vector2>();
            pointBT.Add(0, new Vector2(-1, 1));
            pointBT.Add(1, new Vector2(0, 1));
            pointBT.Add(2, new Vector2(1, 1));
            pointBT.Add(3, new Vector2(1, 0));
            pointBT.Add(4, new Vector2(0, 0));
            pointBT.Add(5, new Vector2(-1, 0));
            pointBT.Add(6, new Vector2(-1, -1));
            pointBT.Add(7, new Vector2(0, -1));
            pointBT.Add(8, new Vector2(1, -1));
        }

        public void InitLayers() {
            FullBodyLayer.Init(myAnimator);
        }
    }
}
