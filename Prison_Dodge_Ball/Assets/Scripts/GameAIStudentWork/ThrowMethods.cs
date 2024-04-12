
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using GameAI;
using System;

namespace GameAIStudent
{

    public class ThrowMethods
    {

        public const string StudentName = "Yu-Chang Cheng";

        public static bool PredictThrow(
        // The initial launch position of the projectile
        Vector3 projectilePos,
        // The initial ballistic speed of the projectile
        float maxProjectileSpeed,
        // The gravity vector affecting the projectile (likely passed as Physics.gravity)
        Vector3 projectileGravity,
        // The initial position of the target
        Vector3 targetInitPos,
        // The constant velocity of the target (zero acceleration assumed)
        Vector3 targetConstVel,
        // The forward facing direction of the target. Possibly of use if the target
        // velocity is zero
        Vector3 targetForwardDir,
        // For algorithms that approximate the solution, this sets a limit for how far
        // the target and projectile can be from each other at the interceptT time
        // and still count as a successful prediction
        float maxAllowedErrorDist,
        // Output param: The solved projectileDir for ballistic trajectory that intercepts target
        out Vector3 projectileDir,
        // Output param: The speed the projectile is launched at in projectileDir such that
        // there is a collision with target. projectileSpeed must be <= maxProjectileSpeed
        out float projectileSpeed,
        // Output param: The time at which the projectile and target collide
        out float interceptT,
        // Output param: An alternate time at which the projectile and target collide
        // Note that this is optional to use and does NOT coincide with the solved projectileDir
        // and projectileSpeed. It is possibly useful to pass on to an incremental solver.
        // It only exists to simplify compatibility with the ShootingRange
        out float altT)
        {
            // TODO implement an accurate throw with prediction. This is just a placeholder

            // FYI, if Minion.transform.position is sent via param targetPos,
            // be aware that this is the midpoint of Minion's capsuleCollider
            // (Might not be true of other agents in Unity though. Just keep in mind for future game dev)

            // Only going 2D for simple demo. this is not useful for proper prediction
            // Basically, avoiding throwing down at enemies since we aren't predicting accurately here.
            //var targetPos2d = new Vector3(targetInitPos.x, 0f, targetInitPos.z);
            //var launchPos2d = new Vector3(projectilePos.x, 0f, projectilePos.z);

            //var relVec = (targetPos2d - launchPos2d);
            //interceptT = relVec.magnitude / maxProjectileSpeed;
            altT = -1f;

            // This is a hard-coded approximate sort of of method to figure out a loft angle
            // This is NOT the right thing to do for your prediction code!
            //var normAngle = Mathf.Lerp(0f, 20f, interceptT * 0.007f);
            interceptT = 0;
            Vector3 v = Vector3.zero;

            // Make sure this is normalized! (The direction of your throw)
            projectileDir = v.normalized;

            // You'll probably want to leave this as is. For advanced prediction you can slow your throw down
            // You don't need to predict the speed of your throw. Only the direction assuming full speed
            //maxProjectileSpeed = maxProjectileSpeed;
            projectileSpeed = 0.9f * maxProjectileSpeed;

            // TODO return true or false based on whether target can actually be hit
            // This implementation just thinks, "I guess so?", and returns true
            // Implementations that don't exactly solve intercepts will need to test the approximate
            // solution with maxAllowedErrorDist. If your solution does solve exactly, you will
            // probably want to add a debug assertion to check your solution against it.

            Vector3 direction_vec = targetConstVel.magnitude == 0 ? targetForwardDir : targetConstVel;
            Vector3 relative_vec = targetInitPos - projectilePos;

            float mag_a = relative_vec.magnitude;
            float targetSpeed = targetConstVel.magnitude == 0 ? targetForwardDir.magnitude : targetConstVel.magnitude;
            float cosine = Vector3.Dot(relative_vec.normalized, direction_vec.normalized);
            float a = Mathf.Pow(targetSpeed, 2) - Mathf.Pow(projectileSpeed, 2);
            float b = 2f * mag_a * targetSpeed * cosine;
            float c = mag_a * mag_a;
            float denominator = 2f * a;
            float numerator_b2_4ac = Mathf.Sqrt((b * b) - (4f * a * c));


            if (numerator_b2_4ac < 0)
            {
                return false;
            }
            else if (denominator == 0)
            {
                interceptT = relative_vec.magnitude / (2f * targetSpeed * Vector3.Dot(relative_vec.normalized, targetConstVel.normalized));
                v = relative_vec / interceptT + targetConstVel - 0.5f * projectileGravity * interceptT;
                projectileDir = v.normalized;
                projectileSpeed = v.magnitude;
            }
            else
            {
                float p = -b / denominator;
                float q = numerator_b2_4ac / denominator;
                float t1 = p - q;
                float t2 = p + q;
                if (t1 <= t2 || t2 <= 0)
                {
                    interceptT = t1;
                }
                else
                {
                    interceptT = t2;
                }
                v = relative_vec / interceptT + targetConstVel - 0.5f * projectileGravity * interceptT;
                projectileDir = v.normalized;
                projectileSpeed = v.magnitude;

                if (interceptT <= 0 || float.IsNaN(interceptT))
                {
                    return false;
                }

                if (projectileSpeed > maxProjectileSpeed){
                    return false;
                }
            }

            return IsPredictedCollisionValid(targetInitPos, projectilePos, targetConstVel, v, interceptT, maxAllowedErrorDist);
        }

        public static bool IsPredictedCollisionValid(Vector3 targetInitPos, Vector3 projectilePos, Vector3 targetConstVel, Vector3 v, float interceptT, float maxAllowedErrorDist)
        {

            Vector3 targetFinalPos = targetInitPos + targetConstVel * interceptT;
            Vector3 projectileFinalPos = projectilePos + v * interceptT;

            if (Vector3.Distance(targetFinalPos.normalized, projectileFinalPos.normalized) > maxAllowedErrorDist)
            {
                return false;
            }

            return true;
        }

    }

    
}