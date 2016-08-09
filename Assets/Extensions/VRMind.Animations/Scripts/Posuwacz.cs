using UnityEngine;

namespace VRMind.Animations
{
    /// <summary>
    /// Posuwacz który interpoluje
    /// </summary>
    public class Posuwacz : MonoBehaviour
    {
        public delegate void OneArg(int layerNo);
        public event OneArg Moved; 

        Vector2[] stepLength;
        Vector2[] destination;
        int[] counterX;
        int[] counterY;
        float timeCounter = -1;
        float period = 0.05f;
        float[] isWaiting;

        Animator myAnim;
        bool initialised = false;

        void Update()
        {
            if (!initialised)
                return;
            timeCounter -= Time.deltaTime;
            if (timeCounter < 0) {
                Jump();
                timeCounter = period;
                DoWait();
            }
        }

        public void Init(int layersNumber, Animator anim)
        {
            stepLength = new Vector2[layersNumber];
            destination = new Vector2[layersNumber];
            counterX = new int[layersNumber];
            counterY = new int[layersNumber];
            isWaiting = new float[layersNumber];
            myAnim = anim;
           
            for (int i = 0; i < layersNumber; ++i) {
                counterX[i] = -1;
                counterY[i] = -1;
                isWaiting[i] = -1;
            }
            initialised = true;
        }

        /// <summary>
        /// Move animator parameter from current value to the set value over a given time 
        /// </summary>
        /// <param name="layerNo"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
        /// <param name="time"></param>
        public void TranslateConst(Layer layer, float toX, float toY, float time)
        {

            float diffX = toX - myAnim.GetFloat(layer.animatorLayerName + ".x");
            float diffY = toY - myAnim.GetFloat(layer.animatorLayerName + ".y");

            if (diffX != 0) {
                destination[layer.number].x = toX;
                stepLength[layer.number].x = diffX / (time / period);
                counterX[layer.number] = (int)(diffX / stepLength[layer.number].x);
            }
            else {
                stepLength[layer.number].x = 0f;
                counterX[layer.number] = -1;
            }

            if (diffY != 0) {
                destination[layer.number].y = toY;
                stepLength[layer.number].y = diffY / (time / period);
                counterY[layer.number] = (int)(diffY / stepLength[layer.number].y);
            }
            else {
                stepLength[layer.number].y = 0f;
                counterY[layer.number] = -1;
            }
            if (stepLength[layer.number].x == 0 && stepLength[layer.number].y == 0)
                Debug.LogWarning("you trying to make null transation!");
            //Debug.Log("set pos x " + destination[layerNo].x + " y " + destination[layerNo].y);
            //Debug.Log("counter x " + counterX[layerNo] + " y " + counterY[layerNo]);
            //Debug.Log("step x " + stepLength[layerNo].x + " y " + stepLength[layerNo].y);
        }

        public void ForceMove(int layerNo, float toX, float toY)
        {
            myAnim.SetFloat(myAnim.GetLayerName(layerNo) + ".x", toX);
            myAnim.SetFloat(myAnim.GetLayerName(layerNo) + ".y", toY);
        }

        public void Wait(int layerNo, float time)
        {
            isWaiting[layerNo] = time;
        }

        void DoWait()
        {
            for (int i = 0; i < stepLength.Length; ++i) {
                if (isWaiting[i] > 0) {
                    if ((isWaiting[i] -= period) < 0)
                        Moved(i);
                }
            }
        }

        void Jump()
        {
            for (int i = 0; i < stepLength.Length; ++i) {

                if (isWaiting[i] > 0) 
                    return;

                if (counterX[i] > -1) {
                    if (counterX[i] > 0)
                        myAnim.SetFloat(myAnim.GetLayerName(i) + ".x", myAnim.GetFloat(myAnim.GetLayerName(i) + ".x") + stepLength[i].x);
                    else {
                        myAnim.SetFloat(myAnim.GetLayerName(i) + ".x", destination[i].x);
                        Moved(i);
                    }
                    --counterX[i];
                }

                if (counterY[i] > -1) {
                    if (counterY[i] > 0)
                        myAnim.SetFloat(myAnim.GetLayerName(i) + ".y", myAnim.GetFloat(myAnim.GetLayerName(i) + ".y") + stepLength[i].y);
                    else {
                        myAnim.SetFloat(myAnim.GetLayerName(i) + ".y", destination[i].y);
                        Moved(i);
                    }
                    --counterY[i];
                }
            }
        }

        //public void Sequence(Queue<Vector3> vec, int layerNo)
        //{
        //    sequence = vec;
        //    DoSequence(layerNo);
        //}

        //void DoSequence(int layerNo)
        //{
        //    if (sequence.Count == 0)
        //        return;
        //    Vector3 currentMove = sequence.Dequeue();
        //    if (currentMove.x == 99) {
        //        Invoke("DoSequence", currentMove.z);
        //        return;
        //    }
        //    TranslateConst(0, currentMove.x, currentMove.y, currentMove.z);
        //}
    }
}
