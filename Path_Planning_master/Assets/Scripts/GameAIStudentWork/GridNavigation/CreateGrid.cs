using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAICourse
{

    public class CreateGrid
    {

        // Please change this string to your name
        public const string StudentAuthorName = "Yu-Chang Cheng";


        // Helper method provided to help you implement this file. Leave as is.
        // Returns true if point p is inside (or on edge) the polygon defined by pts (CCW winding). False, otherwise
        static bool IsPointInsidePolygon(Vector2Int[] pts, Vector2Int p)
        {
            return CG.InPoly1(pts, p) != CG.PointPolygonIntersectionType.Outside;
        }


        // Helper method provided to help you implement this file. Leave as is.
        // Returns float converted to int according to default scaling factor (1000)
        static int Convert(float v)
        {
            return CG.Convert(v);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns Vector2 converted to Vector2Int according to default scaling factor (1000)
        static Vector2Int Convert(Vector2 v)
        {
            return CG.Convert(v);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns true is segment AB intersects CD properly or improperly
        static bool Intersects(Vector2Int a, Vector2Int b, Vector2Int c, Vector2Int d)
        {
            return CG.Intersect(a, b, c, d);
        }

        // IsPointInsideBoundingBox(): Determines whether a point (Vector2Int:p) is On/Inside a bounding box (such as a grid cell) defined by
        // minCellBounds and maxCellBounds (both Vector2Int's).
        // Returns true if the point is ON/INSIDE the cell and false otherwise
        // This method should return true if the point p is on one of the edges of the cell.
        // This is more efficient than PointInsidePolygon() for an equivalent dimension poly
        // Preconditions: minCellBounds <= maxCellBounds, per dimension
        static bool IsPointInsideAxisAlignedBoundingBox(Vector2Int minCellBounds, Vector2Int maxCellBounds, Vector2Int p)
        {
            if (p.x >= minCellBounds.x  && p.x <= maxCellBounds.x && p.y >= minCellBounds.y  && p.y <= maxCellBounds.y)
            {
                // Point is inside the bounding box
                return true;
            }
            else
            {
                // Point is outside the bounding box or on the edge
                return false;
            }
        }

        // IsRangeOverlapping(): Determines if the range (inclusive) from min1 to max1 overlaps the range (inclusive) from min2 to max2.
        // The ranges are considered to overlap if one or more values is within the range of both.
        // Returns true if overlap, false otherwise.
        // Preconditions: min1 <= max1 AND min2 <= max2
        static bool IsRangeOverlapping(int min1, int max1, int min2, int max2)
        {
            if (max1 >= min2 && max2 >= min1)
            {
                // There is overlap
                return true;
            }

            // No overlap
            return false;
        }

        // IsAxisAlignedBouningBoxOverlapping(): Determines if the AABBs defined by min1,max1 and min2,max2 overlap or touch
        // Returns true if overlap, false otherwise.
        // Preconditions: min1 <= max1, per dimension. min2 <= max2 per dimension
        static bool IsAxisAlignedBoundingBoxOverlapping(Vector2Int min1, Vector2Int max1, Vector2Int min2, Vector2Int max2)
        {
            // Check for overlap along the x-axis
            bool isXOverlapping = IsRangeOverlapping(min1.x, max1.x, min2.x, max2.x);

            // Check for overlap along the y-axis
            bool isYOverlapping = IsRangeOverlapping(min1.y, max1.y, min2.y, max2.y);

            // If both x and y axes are overlapping, then the AABBs are overlapping
            return isXOverlapping && isYOverlapping;
        }

        // IsTraversable(): returns true if the grid is traversable from grid[x,y] in the direction dir, false otherwise.
        // The grid boundaries are not traversable. If the grid position x,y is itself not traversable but the grid cell in direction
        // dir is traversable, the function will return false.
        // returns false if the grid is null, grid rank is not 2 dimensional, or any dimension of grid is zero length
        // returns false if x,y is out of range
        // Note: public methods are autograded
        public static bool IsTraversable(bool[,] grid, int x, int y, TraverseDirection dir)
        {
            // Check if the grid is null or if dimensions are invalid or x, y is out of range
            if (grid == null || grid.GetLength(0) == 0 || grid.GetLength(1) == 0 || x < 0 || y < 0 || x >= grid.GetLength(0) || y >= grid.GetLength(1))
            {
                return false;
            }

            // Check if the current cell is not traversable
            if (!grid[x, y])
            {
                return false;
            }

            // Calculate the adjacent cell's coordinates based on direction
            int adjacentX = x, adjacentY = y;
            switch (dir)
            {
                case TraverseDirection.Up:
                    adjacentY += 1;
                    break;
                case TraverseDirection.Down:
                    adjacentY -= 1;
                    break;
                case TraverseDirection.Left:
                    adjacentX -= 1;
                    break;
                case TraverseDirection.Right:
                    adjacentX += 1;
                    break;
                case TraverseDirection.UpLeft:
                    adjacentX -= 1;
                    adjacentY += 1;
                    break;
                case TraverseDirection.UpRight:
                    adjacentX += 1;
                    adjacentY += 1;
                    break;
                case TraverseDirection.DownLeft:
                    adjacentX -= 1;
                    adjacentY -= 1;
                    break;
                case TraverseDirection.DownRight:
                    adjacentX += 1;
                    adjacentY -= 1;
                    break;
            }

            // Check if the adjacent cell is within grid bounds
            if (adjacentX < 0 || adjacentY < 0 || adjacentX >= grid.GetLength(0) || adjacentY >= grid.GetLength(1))
            {
                return false;
            }

            // Return true if the adjacent cell is traversable
            return grid[adjacentX, adjacentY];
        }


        // Create(): Creates a grid lattice discretized space for navigation.
        // canvasOrigin: bottom left corner of navigable region in world coordinates
        // canvasWidth: width of navigable region in world dimensions
        // canvasHeight: height of navigable region in world dimensions
        // cellWidth: target cell width (of a grid cell) in world dimensions
        // obstacles: a list of collider obstacles
        // grid: an array of bools. A cell is true if navigable, false otherwise
        //    Example: grid[x_pos, y_pos]

        public static void Create(Vector2 canvasOrigin, float canvasWidth, float canvasHeight, float cellWidth,
            List<Polygon> obstacles,
            out bool[,] grid
            )

        {// Calculate the number of rows and columns
            int rows = Mathf.FloorToInt(canvasHeight / cellWidth);
            int cols = Mathf.FloorToInt(canvasWidth / cellWidth);

            // Initialize the grid
            grid = new bool[cols, rows];

            // Iterate through each cell in the grid
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    // Initialize each cell as traversable
                    grid[i, j] = true;

                    // Determine the world position of the current cell's corners
                    Vector2 cellBottomLeft = canvasOrigin + new Vector2(i * cellWidth, j * cellWidth);
                    Vector2 cellTopRight = cellBottomLeft + new Vector2(cellWidth, cellWidth);

                    // Convert it into Int
                    Vector2Int cellMin = Convert(cellBottomLeft);
                    Vector2Int cellMax = Convert(cellTopRight);

                    // Check if the cell intersects with any obstacle
                    foreach (Polygon obstacle in obstacles)
                    {
                        if (DoesObstacleIntersectCell(obstacle, cellMin, cellMax, i, j))
                        {
                            grid[i, j] = false;
                            break;
                        }
                    }
                }
            }
        }

        // Shrinks the cell bounds by a specified amount
        private static void ShrinkCellBounds(ref Vector2Int minBounds, ref Vector2Int maxBounds, int shrinkAmount = 1)
        {
            minBounds += new Vector2Int(shrinkAmount, shrinkAmount);
            maxBounds -= new Vector2Int(shrinkAmount, shrinkAmount);
        }

        // Helper method to check if an obstacle intersects a cell or vice versa
        private static bool DoesObstacleIntersectCell(Polygon obstacle, Vector2Int cellMin, Vector2Int cellMax, int cellX, int cellY)
        {

            // Shrink the cell bounds before checking for intersections
            ShrinkCellBounds(ref cellMin, ref cellMax);

            // Check if any point of the obstacle is inside the shrunken cell's bounding box
            foreach (Vector2 point in obstacle.getPoints())
            {
                Vector2Int pointInt = Convert(point);
                if (IsPointInsideAxisAlignedBoundingBox(cellMin, cellMax, pointInt))
                {
                    return true; // Point of the obstacle is inside the shrunken cell's bounding box
                }
            }

            // Check if any corner of the cell is inside the obstacle
            Vector2Int[] cellCorners = new Vector2Int[]
            {
                cellMin, new Vector2Int(cellMin.x, cellMax.y), cellMax, new Vector2Int(cellMax.x, cellMin.y)
            };

            Vector2Int[] obstaclePointsInt = Array.ConvertAll(obstacle.getPoints(), point => Convert(point));

            foreach (Vector2Int corner in cellCorners)
            {
                //bool cornerIsPartOfObstacle = false;
                //foreach (Vector2Int obstaclePoint in obstaclePointsInt)
                //{
                //    if (corner == obstaclePoint)
                //    {
                //        cornerIsPartOfObstacle = true;
                //        break;
                //    }
                //}

                if (IsPointInsidePolygon(obstaclePointsInt, corner))
                {
                    //Debug.Log($"Cell ({cellX}, {cellY}): Corner inside polygon");
                    return true; // The obstacle intersects the cell
                }
            }

            // Check edge intersections
            Vector2Int[] cellEdges = { cellMin, new Vector2Int(cellMin.x, cellMax.y), cellMax, new Vector2Int(cellMax.x, cellMin.y) };
            for (int i = 0; i < cellEdges.Length; i++)
            {
                Vector2Int a = cellEdges[i];
                Vector2Int b = cellEdges[(i + 1) % cellEdges.Length];
                for (int j = 0; j < obstaclePointsInt.Length; j++)
                {
                    Vector2Int c = obstaclePointsInt[j];
                    Vector2Int d = obstaclePointsInt[(j + 1) % obstaclePointsInt.Length];

                    // Check for intersection
                    if (Intersects(a, b, c, d))
                    {
                        return true;
                        //// Ensure intersection is not at the cell's corners
                        //if (!Array.Exists(cellCorners, corner => corner == c || corner == d))
                        //{
                        //    //Debug.Log($"Cell ({cellX}, {cellY}): Edge intersection");
                        //    return true;
                        //}
                    }
                }
            }

            return false; // No intersection found
        }
    }
}