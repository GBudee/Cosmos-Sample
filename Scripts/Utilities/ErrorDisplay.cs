using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text _Text;
    
    private static ErrorDisplay _instance;
    
    public static void ShowError(string message)
    {
        if (_instance == null)
        {
            _instance = Instantiate(Resources.Load<GameObject>("Error Display")).GetComponent<ErrorDisplay>();
            DontDestroyOnLoad(_instance.gameObject);
        }
        
        _instance._Text.text += message;
    }
}
