using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathG
{
    public static Vector3? LineIntersectionXZ(Vector3 line1Start, Vector3 line1End, Vector3 line2Start, Vector3 line2End)
    {
        float A1 = line1End.z - line1Start.z;
        float B1 = line1Start.x - line1End.x;
        float C1 = A1 * line1Start.x + B1 * line1Start.z;

        float A2 = line2End.z - line2Start.z;
        float B2 = line2Start.x - line2End.x;
        float C2 = A2 * line2Start.x + B2 * line2Start.z;
        
        float determinant = A1 * B2 - A2 * B1;

        if (Mathf.Abs(determinant) < 0.001f) // Lines are (approximately) parallel
            return null;

        Vector3 resultPoint = new Vector3((B2 * C1 - B1 * C2), 0, (A1 * C2 - A2 * C1));
        return resultPoint / determinant;
    }
    
    /// <summary>
    /// For speed, this algorithm modifies the input list.
    /// </summary>
    public static List<T> GetRandomSubset<T>(this List<T> input, int subsetSize)
    {
        int inputSize = input.Count;
        if (subsetSize > inputSize) subsetSize = inputSize;
        for (int i = 0; i < subsetSize; i++)
        {
            int indexToSwap = i + Random.Range(0, inputSize - i);
            (input[i], input[indexToSwap]) = (input[indexToSwap], input[i]);
        }
        return input.GetRange(0, subsetSize);
    }
    
    public static bool LEqual(this float lhs, float rhs)
    {
        return lhs < rhs || Mathf.Approximately(lhs, rhs);
    }
    
    public static bool GEqual(this float lhs, float rhs)
    {
        return lhs > rhs || Mathf.Approximately(lhs, rhs);
    }
    
    public static bool LessThan(this float lhs, float rhs)
    {
        return lhs < rhs && !Mathf.Approximately(lhs, rhs);
    }
    
    public static bool GreaterThan(this float lhs, float rhs)
    {
        return lhs > rhs && !Mathf.Approximately(lhs, rhs);
    }
    
    public static Vector2 Rotate(this Vector2 v, float degrees) {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);
         
        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
    
    public static Vector2 Project(Vector2 vector, Vector2 onNormal)
    {
        float num1 = Vector2.Dot(onNormal, onNormal);
        if ((double) num1 < (double) Mathf.Epsilon)
            return Vector2.zero;
        float num2 = Vector2.Dot(vector, onNormal);
        return new Vector2(onNormal.x * num2 / num1, onNormal.y * num2 / num1);
    }
    
    public static float NormalizedProjectionOfPointOnSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 startToPoint = point - lineStart;
        Vector3 lineSegment = lineEnd - lineStart;
        return Vector3.Dot(lineSegment, startToPoint) / lineSegment.sqrMagnitude;
    }

    public static Vector3 BoundLineSegment(float normalizedProjectionValue, Vector3 lineStart, bool boundStart, Vector3 lineEnd, bool boundEnd)
    {
        if (boundStart && normalizedProjectionValue < 0f)
        {
            return lineStart;
        }
        else if (boundEnd && normalizedProjectionValue > 1f)
        {
            return lineEnd;
        }
        else
        {
            return lineStart + (lineEnd - lineStart) * normalizedProjectionValue;
        }
    }
    
    public static float QuadraticBezierLength(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return Mathf.Lerp(Vector3.Distance(p0, p2), Vector3.Distance(p0, p1) + Vector3.Distance(p1, p2), .5f);
    }
    
    public static Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        return Vector3.Lerp(Vector3.Lerp(p0, p1, t), Vector3.Lerp(p1, p2, t), t);
    }

    public static Vector3 PointProjectionOnUnboundedLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        return BoundLineSegment(NormalizedProjectionOfPointOnSegment(point, lineStart, lineEnd), lineStart, boundStart: false, lineEnd, boundEnd: false);
    }
    
    public static Vector3 LineIntersectionWithPlane(Vector3 pointOnPlane, Vector3 planeNormal, Vector3 lineOrigin, Vector3 lineDirection)
    {
        float numerator = Vector3.Dot(pointOnPlane - lineOrigin, planeNormal);
        float denominator = Vector3.Dot(lineDirection, planeNormal);

        if (denominator == 0)
        {
            return lineOrigin;
        }
        else
        {
            return lineOrigin + lineDirection * numerator / denominator;
        }
    }

    public static float Remap(float inLower, float inUpper, float outLower, float outUpper, float t)
    {
        return Mathf.Lerp(outLower, outUpper, Mathf.InverseLerp(inLower, inUpper, t));
    }

    public static Vector2 UnitCircleToUnitSquare(Vector2 normalizedDir)
    {
        Vector2 result;
        if (Mathf.Abs(normalizedDir.x) <= Mathf.Abs(normalizedDir.y))
        {
            var ySign = Mathf.Sign(normalizedDir.y);
            result = new Vector2(1 / Mathf.Tan(ySign * Mathf.Atan2(normalizedDir.y, normalizedDir.x)), ySign);
        }
        else
        {
            var xSign = Mathf.Sign(normalizedDir.x);
            result = new Vector2(xSign, Mathf.Tan(xSign * Mathf.Atan2(normalizedDir.y, normalizedDir.x)));
        }
        return new Vector2(Mathf.InverseLerp(-1, 1, result.x), Mathf.InverseLerp(-1, 1, result.y));
    }
    
    public static int TrueModulo(int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }

    public static float TrueModulo(float x, float m)
    {
        float r = x % m;
        return r < 0 ? r + m : r;
    }

    public static Vector3 NearestPointOnRay(Ray ray, Vector3 pointToProject)
    {
        return Vector3.Project(pointToProject - ray.origin, ray.direction) + ray.origin;
    }

    public static Vector3 ReflectVectorAcrossYZPlane(Vector3 toReflect)
    {
        return new Vector3(-toReflect.x, toReflect.y, toReflect.z);
    }

    public static Quaternion ReflectRotationAcrossYZPlane(Quaternion toReflect)
    {
        return new Quaternion(-toReflect.x, toReflect.y, toReflect.z, -toReflect.w);
    }

    /// <summary>
    /// Like in NVidia CG, clamps the function to between these two boundaries, and normalizes to return {0 ... 1}
    /// </summary>
    /// <param name="lowerBound"></param>
    /// <param name="upperBound"></param>
    /// <param name="value"></param>
    /// <param name="curve">Optional animation curve to apply to the saturated value</param>
    /// <returns></returns>
    public static float Saturate(float lowerBound, float upperBound, float value, AnimationCurve curve = null)
    {
        float scaledVal = Mathf.Clamp((value - lowerBound) / (upperBound - lowerBound), 0f, 1f);
        return curve != null ? curve.Evaluate(scaledVal) : scaledVal;
    }

    public static void ToSwingTwist(this Quaternion rotation, Vector3 axis, out Quaternion swing, out Quaternion twist)
    {
        Vector3 rotationAsVector = new Vector3(rotation.x, rotation.y, rotation.z);
        Vector3 projection = Vector3.Project(rotationAsVector, axis);
        if (Vector3.Dot(rotationAsVector, axis) >= 0f)
        {
            twist = new Quaternion(projection.x, projection.y, projection.z, rotation.w);
        }
        else
        {
            twist = new Quaternion(-projection.x, -projection.y, -projection.z, -rotation.w);
        }
        twist.Normalize();
        swing = rotation * Quaternion.Inverse(twist);
    }

    public static Vector3 ReplaceX(this Vector3 input, float value)
    {
        return new Vector3(value, input.y, input.z);
    }
    
    public static Vector3 ReplaceY(this Vector3 input, float value)
    {
        return new Vector3(input.x, value, input.z);
    }
    
    public static Vector3 ReplaceZ(this Vector3 input, float value)
    {
        return new Vector3(input.x, input.y, value);
    }
    
    public static Vector3 KeepXY(this Vector3 input)
    {
        return new Vector3(input.x, input.y, 0);
    }
    public static Vector3 KeepXZ(this Vector3 input)
    {
        return new Vector3(input.x, 0, input.z);
    }
    public static Vector3 KeepY(this Vector3 input)
    {
        return new Vector3(0, input.y, 0);
    }
    public static Vector2 KeepXZasXY(this Vector3 input)
    {
        return new Vector2(input.x, input.z);
    }
    public static Vector3 XYtoXZ(this Vector2 input)
    {
        return new Vector3(input.x, 0, input.y);
    }
}
