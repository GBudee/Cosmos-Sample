using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public class CharacterPoser : MonoBehaviour
    {
        [SerializeField] public TransformData[] data;
        
        // How to Use:
        // 1. Add the CharacterPoser component to your character
        // 2. Set the Parent transform
        // 3. Open the unity Animation (Window > Animation > Animation)
        // 4. Select an Animation and a key frame to the Pose you want
        // 5. Click the SavePose button on CharacterPoser
        
        void Start()
        {
            if (data != null)
                foreach (var d in data) d.Apply();
        }
        
        [Serializable]
        public class TransformData
        {
            public Transform jointTransform;
            public Vector3 LocalPosition = Vector3.zero;
            public Vector3 LocalEulerRotation = Vector3.zero;
            public Vector3 LocalScale = Vector3.one;
            
            // Unity requires a default constructor for serialization
            public TransformData() { }
            
            public TransformData(Transform transform)
            {
                jointTransform = transform;
                LocalPosition = transform.localPosition;
                LocalEulerRotation = transform.localEulerAngles;
                LocalScale = transform.localScale;
            }
            
            public void Apply()
            {
                jointTransform.localPosition = LocalPosition;
                jointTransform.localEulerAngles = LocalEulerRotation;
                jointTransform.localScale = LocalScale;
            }
        }
    }
}