using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(GraphicRaycaster))]
    public class GraphicRaycasterDebugger : MonoBehaviour
    {
        void Update()
        {
            // Logs the first hit from the attached graphicraycaster
            GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
            var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            var raycastResults = new List<UnityEngine.EventSystems.RaycastResult>();
            var pointerEvent = new UnityEngine.EventSystems.PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };
            raycaster.Raycast(pointerEvent, raycastResults);
            if (raycastResults.Count > 0)
            {
                var firstResult = raycastResults.First();
                Debug.Log(firstResult.gameObject.name, firstResult.gameObject);
            }
        }
    }
}