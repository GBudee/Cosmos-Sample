using System;
using Audio;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


public class Service : MonoBehaviour
{
    [SerializeField] private GameController _GameController;
    [SerializeField] private AudioController _AudioController;
    [FormerlySerializedAs("_Tutorial")] [SerializeField] private SimpleTutorial _SimpleTutorial;
    [SerializeField] private TooltipManager _TooltipManager;
    
    public static GameController GameController => Instance._GameController;
    public static AudioController AudioController => Instance._AudioController;
    public static SimpleTutorial SimpleTutorial => Instance?._SimpleTutorial;
    public static TooltipManager TooltipManager => Instance?._TooltipManager;
    
    private static Service _instance;
    private static Service Instance
    {
        get
        {
            if (_instance == null && SceneManager.sceneCount > 0)
            {
                _instance = FindObjectOfType<Service>();
                SceneManager.sceneUnloaded += SceneUnloaded;
                void SceneUnloaded(Scene scene)
                {
                    _instance = null;
                    SceneManager.sceneUnloaded -= SceneUnloaded;
                }
            }
            return _instance;
        }
    }
}
