using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CarTimelineController : MonoBehaviour
{
    private PlayableDirector director;
    void Start()
    {
        director = GetComponent<PlayableDirector>();
    }

    public void StartTimeline()
    {
        director.Play();
    }

    public GameObject[] objectPool;
    private int currentIndex = 0;

    public void NewRandomObject()
    {
        int newIndex = Random.Range(0, objectPool.Length);
        // Deactivate old gameobject
        objectPool[currentIndex].SetActive(false);
        // Activate new gameobject
        currentIndex = newIndex;
        objectPool[currentIndex].SetActive(true);
    }

}
