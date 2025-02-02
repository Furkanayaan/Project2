using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class CurrencyPool : MonoBehaviour
{
    public enum PoolType {
        Gold,
        Star,
        Diamond
    }
    [Serializable]
    public class Pool {
        //Parent array of each pool.
        public GameObject[] parent;
        //Array specifying pool types.
        public PoolType[] poolType;
        //Array holding inactive objects.
        public Transform[] deactives;
        //Array holding active objects.
        public Transform[] actives;

        // Method that sets up arrays and references needed for the pool.
        public void SetPool(ref Dictionary<PoolType, int> parentDic) {
            //Adjust the size of the active array to match the parent array.
            Array.Resize(ref actives, parent.Length);
            //Adjust the size of the inactive array to match the parent array.
            Array.Resize(ref deactives, parent.Length);

            for(int i = 0; i < parent.Length; i++) {
                //Define inactive and active objects (first and second child).
                deactives[i] = parent[i].transform.GetChild(0);
                actives[i] = parent[i].transform.GetChild(1);
                //Disable the inactive object initially.
                parent[i].transform.GetChild(0).gameObject.SetActive(false);
                //Add index information corresponding to the pool type.
                parentDic.Add(poolType[i], i);
            }
        }
    } public Pool pool = new();
	
    public static CurrencyPool I;
    //Dictionary holding the indices corresponding to the pool types.
    private Dictionary<PoolType, int> _pools = new ();
	

    private void Start() {
        I = this;
        //Set up the pool arrays and dictionary.
        pool.SetPool(ref _pools);
    }
    
    //Method for distributing currency.
    public void CurrencyAllocation(int count, PoolType type, Transform targetParent, Vector3 currentPos, Action endAction = null) {
        //Do not proceed if the amount is 0.
        if (count == 0) return;
        int totalObj = 0;
        //If 50 or less, take all directly.
        if (count <= 50) totalObj = count;
        else {
            //Use a maximum of 50 objects.
            totalObj = 50;
            //Determine the remainder.
            int remain = count - totalObj;
            //Increase the remaining amount through the GameManager.
            if (type == PoolType.Gold) GameManager.I.IncreaseGold(remain);
            if (type == PoolType.Star) GameManager.I.IncreaseStar(remain);
            if(type == PoolType.Diamond) GameManager.I.IncreaseDiamond(remain);
        }

        //Handle animation and pool management for each object.
        for (int i = 0; i < totalObj; i++) {
            //Check object pool, if there is enough object.
            if(GetDeactivePool(type).childCount == 1) {
                string parentName = GetDeactivePool(type).GetChild(0).name;
                Transform instantiated = Instantiate(GetDeactivePool(type).GetChild(0));
                instantiated.transform.SetParent(GetDeactivePool(type));
                instantiated.name = parentName;
            }

            //Get an object from the pool
            Transform target = GetDeactivePool(type).GetChild(0);
            target.localScale = Vector3.one;
            Vector3 defaultScale = target.localScale;

            // Reference the inactive parent
            Transform deactiveParent = GetDeactivePool(type);

            //Set the object's parent to the active pool.
            target.SetParent(GetActivePool(type));

            //Define the action to be executed at the end of the animation.
            Action<Transform> end = (_transform) => {
                //Move the object back to the inactive pool.
                _transform.SetParent(deactiveParent);
                _transform.localScale = defaultScale;
                //Increase the resource based on the pool type.
                if (type == PoolType.Gold) GameManager.I.IncreaseGold(1);
                if (type == PoolType.Star) GameManager.I.IncreaseStar(1);
                if(type == PoolType.Diamond) GameManager.I.IncreaseDiamond(1);
                // Temporarily enlarge the target parent's size and reset it.
                targetParent.DOScale(new Vector3(1.3f,1.3f,1.3f), 0.2f).OnComplete(() => {
                    targetParent.DOScale(Vector3.one, 0.25f);
                });
                // Execute the end action if defined.
                if (endAction != null) endAction();
            };
            //Current position.
            Vector3 pos0 = Camera.main.WorldToScreenPoint(currentPos);
            //Midpoint.
            Vector3 pos1 = new Vector3(Screen.currentResolution.width / 2f, Screen.currentResolution.height / 2f, 0f);
            //Target position.
            Vector3 pos2 = targetParent.position;
            //Animation path.
            List<Vector3> poses = new List<Vector3>() { 
                pos0,
                pos1,
                pos2
            };
            //Set animation types and parameters.
            AnimationManager.Type animationType = AnimationManager.Type.QuadraticThree;
            AnimationManager.SObjectsToAnim sObjectsToAnim = new();
            AnimationManager.AnimationProperties props = new(target, animationType, poses, end, 0, 0, UnityEngine.Random.Range(0.6f, 0.8f));
            sObjectsToAnim.Add(props);
            //Add the animation.
            AnimationManager.I.ObjectsToAnim.Add(sObjectsToAnim);
        }

    }

	//Function that returns the inactive pool
    private Transform GetDeactivePool(PoolType type) {
        return pool.deactives[_pools[type]];
    }
	
    //Function that returns the active pool
    private Transform GetActivePool(PoolType type) {
        return pool.actives[_pools[type]];
    }
}
