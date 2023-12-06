using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

public class SceneObjectCounter : MonoBehaviour
{
    
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            List<GameObject> everyObject = new List<GameObject>();
            foreach (var rootObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                AddObject_Recursive(rootObject);
            }
            void AddObject_Recursive(GameObject obj)
            {
                everyObject.Add(obj);
                foreach (Transform childObject in obj.transform)
                {
                    AddObject_Recursive(childObject.gameObject);
                }
            }
            
            var result = everyObject.GroupBy(x => x.name)
                .Select(x => (x.Count(), x.Key))
                .OrderByDescending(x => x.Item1)
                .Select(x => $"{x.Item1}: {x.Key}")
                .Take(40).ToVerboseString();
            
            Debug.Log(result);
        }
    }
}
