using UnityEngine;

namespace Utilities
{
    public class QuadraticBezier
    {
        public Vector3 P0;
        public Vector3 P1;
        public Vector3 P2;
        
        // Precalculated values for tangent formula: https://gamedev.stackexchange.com/a/27138
        private Vector3 v1;
        private Vector3 v2;
        
        public QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            P0 = p0;
            P1 = p1;
            P2 = p2;
            
            v1 = 2f * (P0 + P2) - 4f * P1;
            v2 = 2f * (P1 - P0);
        }
        
        public Vector3 GetPoint(float normalizedT)
        {
            if (normalizedT < 0) normalizedT = 0;
            if (normalizedT > 1) normalizedT = 1;
            
            return MathG.QuadraticBezier(P0, P1, P2, normalizedT);
        }
        
        public Vector3 GetTangent(float normalizedT)
        {
            return normalizedT * v1 + v2;
        }
        
        public float GetLength()
        {
            float result = 0f;
            
            // TODO: Copy BezierSolution's system for determining the interval to use
            const int ACCURACY = 30;
            Vector3 prevPos = P0;
            for (int i = 1; i <= ACCURACY; i++)
            {
                float normalizedT = i / (float)ACCURACY;
                Vector3 currentPos = GetPoint(normalizedT);
                result += Vector3.Distance(currentPos, prevPos);
                prevPos = currentPos;
            }
            
            return result;
        }
        
        public void MoveAlongSpline(ref float normalizedT, float deltaMovement, int accuracy = 6)
        {
            float constant = deltaMovement / accuracy;
            for (int i = 0; i < accuracy; i++)
                normalizedT += constant / GetTangent(normalizedT).magnitude;
        }
    }
}