using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour {
    public static AnimationManager I;
    public enum Type {
        Linear,
        QuadraticThree,
        QuadraticFour
    }

    public class AnimationProperties {
        //Target object to apply the animation.
        public Transform TargetTransform; 
        //Animation type (Linear, Quadratic, etc.).
        public Type AnimType;
        //List of positions for the animation path.
        public List<Vector3> Posses;
        //Action to be executed at the end of the animation.
        public Action<Transform> Action;
        //Animation curve index for the position animation.
        public int AnimCurvePos;
        //Animation curve index for the sca;e animation.
        public int AnimCurveScale;
        //Animation speed.
        public float Speed = 1f;
        //Elapsed animation time.
        public float Time = 0f;

        //Constructor that takes and assigns all values.
        public AnimationProperties(Transform targetTransform, Type animType, List<Vector3> posses, Action<Transform> action, int animCurvePos, int animCurveScale, float speed = 1f, float time = 0f) {
            TargetTransform = targetTransform;
            AnimType = animType;
            Posses = posses;
            Action = action;
            AnimCurvePos = animCurvePos;
            AnimCurveScale = animCurveScale;
            Speed = speed;
            Time = time;
        }
    }
    
    [Serializable]
    public class SObjectsToAnim {


        public Transform Transforms;

        public List<Vector3> Posses;
        public Vector3 StartScales;

        public float Times;
        public float Speeds;

        public Type AnimTypes;
        public int animationCurvePoses;
        public int animationCurveScales;

        public Action<Transform> Actions;

        public SObjectsToAnim() {
            Transforms = null;
            Posses = new();
            StartScales = new();
            Times = 0;
            Speeds = 0;
            AnimTypes = Type.Linear;

            animationCurvePoses = 0;
            animationCurveScales = 0;

            Actions = null;
        }

        //Adds a new animation property.
        public void Add(AnimationProperties animationProperties) {
            if(animationProperties == null || animationProperties.TargetTransform == null) {
                return;
            }

            Transforms = animationProperties.TargetTransform;
            Posses = animationProperties.Posses;
            StartScales = animationProperties.TargetTransform.localScale;
            Actions = animationProperties.Action;
            Speeds = animationProperties.Speed;

            animationCurvePoses = animationProperties.AnimCurvePos;
            animationCurveScales = animationProperties.AnimCurveScale;
            AnimTypes = animationProperties.AnimType;

            Times = animationProperties.Time;
        }
        //Called when the animation is complete and performs necessary cleanup.
        public void Remove(Transform targetTransform) {
            try {
                if(Actions != null) 
                    Actions.Invoke(targetTransform);
            }
            catch (System.Exception e) {
                string error = e.ToString();
            }
                
            Transforms = null;
            Posses = new List<Vector3>();
            StartScales = new Vector3();
            Times = 0;
            Speeds = 0;
            animationCurvePoses = 0;
            animationCurveScales = 0;
            AnimTypes = Type.Linear;
            Actions = null;
        }
    }
    
    //List of animations.
    public List<SObjectsToAnim> ObjectsToAnim = new List<SObjectsToAnim>();

    //Position animation curves.
    [SerializeField] private AnimationCurve[] curvesPos;
    //Scale animation curves.
    [SerializeField] private AnimationCurve[] curvesScale;

    public void Awake() {
        I = this;
    }

    public void Update() {
        OneTimeAnimations();
    }


    public void OneTimeAnimations() {
        for (int i = 0; i < ObjectsToAnim.Count; i++) {
            if(ObjectsToAnim[i] == null || ObjectsToAnim[i].Transforms == null ) {
                ObjectsToAnim.RemoveAt(i);
                return;
            }
            if(!ObjectsToAnim[i].Transforms.gameObject.activeSelf) continue;
            //Evaluate animation curves.
            float evaluated = curvesPos[ObjectsToAnim[i].animationCurvePoses].Evaluate(ObjectsToAnim[i].Times);
            float evaluatedScale = curvesScale[ObjectsToAnim[i].animationCurveScales].Evaluate(ObjectsToAnim[i].Times);

            //Apply position and scale.
            ObjectsToAnim[i].Transforms.position = PosReturner(i, evaluated);
            ObjectsToAnim[i].Transforms.localScale = ObjectsToAnim[i].StartScales + Vector3.one * evaluatedScale;

            //Update animation duration.
            ObjectsToAnim[i].Times += Time.smoothDeltaTime / ObjectsToAnim[i].Speeds;
            if (ObjectsToAnim[i].Times > 1f) {
                //Set the position and scale to the final values when the animation ends.
                ObjectsToAnim[i].Transforms.position =
                    PosReturner(i, curvesPos[ObjectsToAnim[i].animationCurvePoses].keys[^1].value);

                ObjectsToAnim[i].Transforms.localScale = 
                    ObjectsToAnim[i].StartScales + Vector3.one * curvesScale[ObjectsToAnim[i].animationCurveScales].keys[^1].value;
                //Clear the animation
                ObjectsToAnim[i].Remove(ObjectsToAnim[i].Transforms);
                ObjectsToAnim.RemoveAt(i);
            }
        }
    }

    
    //Function to calculate position.
    public Vector3 PosReturner(int index, float time) {
        switch (ObjectsToAnim[index].AnimTypes) {
            default:
                return new Vector3();
            case Type.Linear:
                return Linear(index, time);
            
            case Type.QuadraticThree:
                return QuadraticThree(index, time);
            
            case Type.QuadraticFour:
                return QuadraticFour(index, time);
        }
    }

    //Calculate position using linear interpolation.
    public Vector3 Linear(int index, float t) {
        return ObjectsToAnim[index].Posses[0] + t * (ObjectsToAnim[index].Posses[1] - ObjectsToAnim[index].Posses[0]);
    }
    
    //Calculate position using three-point quadratic interpolation
    public Vector3 QuadraticThree(int index, float t) {
        Vector3 p = (1f - t) * ((1 - t) * ObjectsToAnim[index].Posses[0] + t * ObjectsToAnim[index].Posses[1]) + t * ((1 - t) * ObjectsToAnim[index].Posses[1] + t * ObjectsToAnim[index].Posses[2]);
        return p;
    }
    
    //Calculate position using four-point quadratic interpolation.
    public Vector3 QuadraticFour(int index, float t) {
        Vector3 p = Mathf.Pow(1f - t, 3f) * ObjectsToAnim[index].Posses[0] + 
                    3f * Mathf.Pow(1f - t, 2f) * t * ObjectsToAnim[index].Posses[1] + 
                    3f * (1f - t) * t * t * ObjectsToAnim[index].Posses[2] + 
                    Mathf.Pow(t, 3f) * ObjectsToAnim[index].Posses[3];
        
        return p;
    }
}
