using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAICourse
{

    public class CreatePathNetwork
    {

        public const string StudentAuthorName = "Yu-Chang Cheng";

        // Helper method provided to help you implement this file. Leave as is.
        // Returns Vector2 converted to Vector2Int according to default scaling factor (1000)
        public static Vector2Int ConvertToInt(Vector2 v)
        {
            return CG.Convert(v);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns float converted to int according to default scaling factor (1000)
        public static int ConvertToInt(float v)
        {
            return CG.Convert(v);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns Vector2Int converted to Vector2 according to default scaling factor (1000)
        public static Vector2 ConvertToFloat(Vector2Int v)
        {
            float f = 1f / (float)CG.FloatToIntFactor;
            return new Vector2(v.x * f, v.y * f);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns int converted to float according to default scaling factor (1000)
        public static float ConvertToFloat(int v)
        {
            float f = 1f / (float)CG.FloatToIntFactor;
            return v * f;
        }


        // Helper method provided to help you implement this file. Leave as is.
        // Returns true is segment AB intersects CD properly or improperly
        static public bool Intersects(Vector2Int a, Vector2Int b, Vector2Int c, Vector2Int d)
        {
            return CG.Intersect(a, b, c, d);
        }


        //Get the shortest distance from a point to a line
        //Line is defined by the lineStart and lineEnd points
        public static float DistanceToLineSegment(Vector2Int point, Vector2Int lineStart, Vector2Int lineEnd)
        {
            return CG.DistanceToLineSegment(point, lineStart, lineEnd);
        }


        //Get the shortest distance from a point to a line
        //Line is defined by the lineStart and lineEnd points
        public static float DistanceToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            return CG.DistanceToLineSegment(point, lineStart, lineEnd);
        }


        // Helper method provided to help you implement this file. Leave as is.
        // Determines if a point is inside/on a CCW polygon and if so returns true. False otherwise.
        public static bool IsPointInPolygon(Vector2Int[] polyPts, Vector2Int point)
        {
            return CG.PointPolygonIntersectionType.Outside != CG.InPoly1(polyPts, point);
        }

        // Returns true iff p is strictly to the left of the directed
        // line through a to b.
        // You can use this method to determine if 3 adjacent CCW-ordered
        // vertices of a polygon form a convex or concave angle

        public static bool Left(Vector2Int a, Vector2Int b, Vector2Int p)
        {
            return CG.Left(a, b, p);
        }

        // Vector2 version of above
        public static bool Left(Vector2 a, Vector2 b, Vector2 p)
        {
            return CG.Left(CG.Convert(a), CG.Convert(b), CG.Convert(p));
        }

        //Student code to build the path network from the given pathNodes and Obstacles
        //Obstacles - List of obstacles on the plane
        //agentRadius - the radius of the traversing agent
        //minPoVDist AND maxPoVDist - used for Points of Visibility (see assignment doc)
        //pathNodes - ref parameter that contains the pathNodes to connect (or return if pathNetworkMode is set to PointsOfVisibility)
        //pathEdges - out parameter that will contain the edges you build.
        //  Edges cannot intersect with obstacles or boundaries. Edges must be at least agentRadius distance
        //  from all obstacle/boundary line segments. No self edges, duplicate edges. No null lists (but can be empty)
        //pathNetworkMode - enum that specifies PathNetwork type (see assignment doc)

        public static void Create(Vector2 canvasOrigin, float canvasWidth, float canvasHeight,
            List<Polygon> obstacles, float agentRadius, float minPoVDist, float maxPoVDist, ref List<Vector2> pathNodes, out List<List<int>> pathEdges,
            PathNetworkMode pathNetworkMode)
        {    
            pathEdges = new List<List<int>>();
            // Initialize pathEdges list
            for (int i = 0; i < pathNodes.Count; i++)
            {
                pathEdges.Add(new List<int>());
            }

            Vector2Int canvasStart = ConvertToInt(canvasOrigin);
            Vector2Int canvasEnd = ConvertToInt(canvasOrigin) + ConvertToInt(new Vector2(canvasWidth, canvasHeight));
            int agentRadiusInt = ConvertToInt(agentRadius);

            for (int i = 0; i < pathNodes.Count; i++)
            {
                for (int j = i + 1; j < pathNodes.Count; j++)
                {
                    Vector2Int nodeA = ConvertToInt(pathNodes[i]);
                    Vector2Int nodeB = ConvertToInt(pathNodes[j]);

                    // Check if the direct line between nodes intersects any obstacle or the boundary
                    if (IsValidEdge(nodeA, nodeB, obstacles, canvasStart, canvasEnd, agentRadiusInt))
                    {
                        pathEdges[i].Add(j);
                        pathEdges[j].Add(i);
                    }
                }
            }
        }

        private static bool IsWithinBoundary(Vector2Int point, Vector2Int start, Vector2Int end, int radius)
        {
            return point.x >= start.x + radius && point.y >= start.y + radius && point.x <= end.x - radius && point.y <= end.y - radius;
        }

        private static bool IsValidEdge(Vector2Int nodeA, Vector2Int nodeB, List<Polygon> obstacles, Vector2Int canvasStart, Vector2Int canvasEnd, int agentRadiusInt)
        {
            // First, check if the nodes themselves are within the boundary with sufficient clearance
            if (!IsWithinBoundary(nodeA, canvasStart, canvasEnd, agentRadiusInt) || !IsWithinBoundary(nodeB, canvasStart, canvasEnd, agentRadiusInt))
                return false;

            // Check clearance from obstacles for both nodes and the edge
            foreach (var obstacle in obstacles)
            {
                Vector2Int[] vertices = obstacle.getIntegerPoints();
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector2Int currentVertex = vertices[i];
                    Vector2Int nextVertex = vertices[(i + 1) % vertices.Length];

                    // Check if the edge intersects with obstacle edges
                    if (Intersects(nodeA, nodeB, currentVertex, nextVertex))
                        return false;

                    // Check if the edge is too close to the obstacle edge
                    if (IsEdgeTooCloseToObstacle(nodeA, nodeB, currentVertex, nextVertex, agentRadiusInt))
                        return false;
                }
            }

            return true;
        }

        private static bool IsEdgeTooCloseToObstacle(Vector2Int nodeA, Vector2Int nodeB, Vector2Int obstacleStart, Vector2Int obstacleEnd, int agentRadiusInt)
        {
            // Use the provided DistanceToLineSegment method to check the distance from the edge to the obstacle segment
            float distance = DistanceToLineSegment(ConvertToFloat(nodeA), ConvertToFloat(obstacleStart), ConvertToFloat(obstacleEnd));
            float distance2 = DistanceToLineSegment(ConvertToFloat(nodeB), ConvertToFloat(obstacleStart), ConvertToFloat(obstacleEnd));
            if (distance < ConvertToFloat(agentRadiusInt) || distance2 < ConvertToFloat(agentRadiusInt))
                return true;

            // Additionally, check if any point on the edge is too close to the obstacle vertices
            if (DistanceToLineSegment(obstacleStart, nodeA, nodeB) < agentRadiusInt || DistanceToLineSegment(obstacleEnd, nodeA, nodeB) < agentRadiusInt)
                return true;

            return false;
        }




    }

}