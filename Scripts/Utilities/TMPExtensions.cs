using UnityEngine;
using TMPro;

namespace Utilities
{
    public static class TMPExtensions
    {
        public static void MatchTextToCurve(this TMP_Text textComponent, System.Func<float, float> curve)
        {
            textComponent.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.
            
            TMP_TextInfo textInfo = textComponent.textInfo;
            int characterCount = textInfo.characterCount;
            if (characterCount == 0) return;
            
            float boundsMinX = textComponent.bounds.min.x;
            float boundsMaxX = textComponent.bounds.max.x;
    
            for (int i = 0; i < characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;
    
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
    
                // Get the index of the mesh used by this character.
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
    
                Vector3[] ref_textVertices = textInfo.meshInfo[materialIndex].vertices;
    
                // Compute the baseline mid point for each character
                Vector3 offsetToMidBaseline = new Vector2((ref_textVertices[vertexIndex + 0].x + ref_textVertices[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);
    
                // Apply offset to adjust our pivot point.
                ref_textVertices[vertexIndex + 0] += -offsetToMidBaseline;
                ref_textVertices[vertexIndex + 1] += -offsetToMidBaseline;
                ref_textVertices[vertexIndex + 2] += -offsetToMidBaseline;
                ref_textVertices[vertexIndex + 3] += -offsetToMidBaseline;
    
                // Compute the angle of rotation for each character based on the animation curve
                float x0 = (offsetToMidBaseline.x - boundsMinX) / (boundsMaxX - boundsMinX); // Character's position relative to the bounds of the mesh.
                float x1 = x0 + 0.0001f;
                float y0 = curve.Invoke(x0);
                float y1 = curve.Invoke(x1);
                
                Vector3 horizontal = new Vector3(1, 0, 0);
                //Vector3 normal = new Vector3(-(y1 - y0), (x1 * (boundsMaxX - boundsMinX) + boundsMinX) - offsetToMidBaseline.x, 0);
                Vector3 tangent = new Vector3(x1 * (boundsMaxX - boundsMinX) + boundsMinX, y1) - new Vector3(offsetToMidBaseline.x, y0);
                
                float dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * 57.2957795f;
                Vector3 cross = Vector3.Cross(horizontal, tangent);
                float angle = cross.z > 0 ? dot : 360 - dot;
                
                Matrix4x4 charTRS = Matrix4x4.TRS(new Vector3(0, y0, 0), Quaternion.Euler(0, 0, angle), Vector3.one);
                
                ref_textVertices[vertexIndex + 0] = charTRS.MultiplyPoint3x4(ref_textVertices[vertexIndex + 0]);
                ref_textVertices[vertexIndex + 1] = charTRS.MultiplyPoint3x4(ref_textVertices[vertexIndex + 1]);
                ref_textVertices[vertexIndex + 2] = charTRS.MultiplyPoint3x4(ref_textVertices[vertexIndex + 2]);
                ref_textVertices[vertexIndex + 3] = charTRS.MultiplyPoint3x4(ref_textVertices[vertexIndex + 3]);
                
                ref_textVertices[vertexIndex + 0] += offsetToMidBaseline;
                ref_textVertices[vertexIndex + 1] += offsetToMidBaseline;
                ref_textVertices[vertexIndex + 2] += offsetToMidBaseline;
                ref_textVertices[vertexIndex + 3] += offsetToMidBaseline;
            }
    
    
            // Upload the mesh with the revised information
            textComponent.UpdateVertexData();
        }
    }
}