
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using GameAI;


namespace GameAIStudent
{

    public class ShotSelection
    {

        public const string StudentName = "Yu-Chang Cheng";


        public enum SelectThrowReturn
        {
            DoThrow,
            NoThrowTargettingFailed,
            NoThrowOpponentCurrentlyAccelerating,
            NoThrowOpponentWillAccelerate,
            NoThrowOpponentOccluded
        }

        public static SelectThrowReturn SelectThrow(
                // the minion doing the throwing, can also be used to query generic params true of all minions
                MinionScript thisMinion,
                // info about the target
                PrisonDodgeballManager.OpponentInfo opponent,
                // What is the navmask that defines where on the navmesh the opponent can traverse
                int opponentNavmask,
                // typically this is a value a tiny bit smaller than the radius of minion added with radius of the dodgeball
                float maxAllowedThrowErrDist,
                // Time since last frame
                float deltaT,
                // Output param: The solved projectileDir for ballistic trajectory that intercepts target                
                out Vector3 projectileDir,
                // Output param: The speed the projectile is launched at in projectileDir such that
                // there is a collision with target. projectileSpeed must be <= maxProjectileSpeed
                out float projectileSpeed,
                // Output param: The time at which the projectile and target collide
                out float interceptT,
                // Output param: where the shot is expected to hit
                out Vector3 interceptPos
            )
        {
            var Mgr = PrisonDodgeballManager.Instance;
            PrisonDodgeballManager.DodgeballInfo ballInfo;
            float ballRadius;

            if (Mgr.GetDodgeballInfo(PrisonDodgeballManager.Team.TeamA, thisMinion.DodgeballIndex, out ballInfo, false))
            {
                ballRadius = ballInfo.Radius;
            }
            else
            {
                ballRadius = 0.25f; // Default radius in case of failure to get info
            }

            //// Dynamically calculate maxAllowedThrowErrDist based on the ball radius
            maxAllowedThrowErrDist = (ballRadius + 0.5f) * 0.99f;

            var opponentVel = opponent.Vel; // Or perhaps use thisMinion.MaxPathSpeed (max speed a minion can go)
                                            // times dir if you think minion is nearly there.
                                            // Using something other than the opponent's current Vel requires extra logic

            interceptPos = opponent.Pos;

            // see if throw is even possible, before deciding whether to actually do it
            if (!ThrowMethods.PredictThrow(thisMinion.HeldBallPosition, thisMinion.ThrowSpeed, Physics.gravity, opponent.Pos,
                opponentVel, opponent.Forward, maxAllowedThrowErrDist,
                out projectileDir, out projectileSpeed, out interceptT, out float altT))
            {
                return SelectThrowReturn.NoThrowTargettingFailed;
            }

            interceptPos = opponent.Pos + opponent.Vel * interceptT;

            // OK, the throw is possible based on assumptions. But there are other reasons why we might skip throwing right now.


            // TODO Screen opponent. Consider if there are obvious signs that the agent is accelerating (breaking constant acceleration assumption)
            // possibilities:
            // * agent not currently sufficiently stopped or sufficiently up to full speed
            // * agent appears to be turning significantly from previous direction

            // On failure, return NoThrowOpponentCurrentlyAccelerating
            if (IsOpponentAccelerating(opponent))
            {
                return SelectThrowReturn.NoThrowOpponentCurrentlyAccelerating;
            }


            // TODO Next consider the impact of the environment on future agent movement. 
            // Use NavMesh.Raycast() to determine if opponent would run into barrier before throw would get there
            // We know the opponent won't actually run into a barrier, so if you get a raycast hit that means the opponent
            // is going to be changing direction or stopping. So it is probably good to not throw in these circumstances.

            // NavMesh.Raycast() call and appropriate logic goes here (also see: opponentNavmask)

            // On failure, return NoThrowOpponentWillAccelerate
            if (IsPathObstructed(opponent.Pos, interceptPos, opponentNavmask))
            {
                return SelectThrowReturn.NoThrowOpponentWillAccelerate;
            }

            // TODO next consider the possibility that if the ball is thrown, it will hit something before it gets to the agent.
            // This isn't helpful for a normal game of prison dodgeball (nothing to get in the way) but it is important
            // for the AdvancedMinionTestThrowScenario.
            // You should use Physics.Raycast()
            // TIP: For best result in AdvancedMinionTestThrowScenario map, cast two parallel rays ball width apart

            // Use the mask below for ignoring geometry we don't care about.

            // carverMask exclusion only needed for AdvancedMinionTestThrowScenario
            int carverMask = ~(1 << Mgr.NavMeshCarverLayerIndex);
            // We don't care about minion hits from raycast. Self hits should already be avoided but will filter all minions.
            // And the whole point of the throw is to hit the opponent minion, so we don't want a raycast hit stopping us.
            int minionMask = ~(1 << Mgr.MinionTeamBLayerIndex) & ~(1 << Mgr.MinionTeamALayerIndex);
            // Ignore dodgeballs. They'll most likely be out of the way before they collide
            int ballMask = ~(1 << Mgr.BallTeamALayerIndex) & ~(1 << Mgr.BallTeamBLayerIndex);
            int mask = Physics.AllLayers & carverMask & ballMask & minionMask;

            // On failure due to Physics.Raycast() hit, return NoThrowOpponentOccluded
            RaycastHit raycastHit;
            Vector3 direction = (interceptPos - thisMinion.HeldBallPosition).normalized;
            float distance = Vector3.Distance(thisMinion.HeldBallPosition, interceptPos);

            if (!IsThrowPathClear(thisMinion.HeldBallPosition, interceptPos, ballRadius, mask))
            {
                return SelectThrowReturn.NoThrowOpponentOccluded;
            }


            // We got this far, so the throw is probably a good idea!
            return SelectThrowReturn.DoThrow;
        }

        // Check if the opponent is accelerating
        private static bool IsOpponentAccelerating(PrisonDodgeballManager.OpponentInfo opponent)
        {
            float velocityChange = (opponent.Vel - opponent.PrevVel).magnitude;
            float directionChange = Vector3.Angle(opponent.Forward, opponent.PrevForward);

            float significantVelocityChange = 0.9f;
            float significantDirectionChange = 1f;

            bool isAccelerating = velocityChange > significantVelocityChange || directionChange> significantDirectionChange;

            return isAccelerating;
        }

        // Check if the path between two points on the NavMesh is obstructed
        private static bool IsPathObstructed(Vector3 startPosition, Vector3 endPosition, int navLayerMask)
        {
            NavMeshHit hit;
            bool isObstructed = NavMesh.Raycast(startPosition, endPosition, out hit, navLayerMask);
            return isObstructed;
        }

        // Check if the path of the throw is clear of any obstructions, considering the width of the ball
        private static bool IsThrowPathClear(Vector3 startPosition, Vector3 endPosition, float ballRadius, int layerMask)
        {
            Vector3 direction = (endPosition - startPosition).normalized;
            float distance = Vector3.Distance(startPosition, endPosition);

            Vector3 rightOffset = Vector3.Cross(direction, Vector3.up) * ballRadius;
            Vector3 leftStartPosition = startPosition - rightOffset;
            Vector3 rightStartPosition = startPosition + rightOffset;

            RaycastHit hit;
            bool leftClear = !Physics.Raycast(leftStartPosition, direction, out hit, distance, layerMask);
            bool rightClear = !Physics.Raycast(rightStartPosition, direction, out hit, distance, layerMask);

            return leftClear && rightClear;
        }

    }


}