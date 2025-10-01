
using Silk.NET.Maths;
using System.Data;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace maelstrom_poc
{
    using Point = Vector2D<float>;

    public static class Voronoi
    {
        private static List<DisplayObject> _displayObjects;

        public static void InitVoronoi(List<DisplayObject> displayObjects)
        {
            VoronoiTests.Test();
            _displayObjects = displayObjects;
        }

        public static void ComputeVoronoi(float time = 0.0f)
        {

            foreach (var displayObject in _displayObjects)
            {
                ComputeCellVoronoi(displayObject, time);
            }
        }

        private static void ComputeCellVoronoi(DisplayObject displayObject, float time = 0.0f)
        {
            Point currentCenter = displayObject.Position;

            // Start woth the screen as the first vertices, knwoing we have 8 vertices (center is 0.0f, 0.0f)
            Point[] currentVertices = new Point[] {
                new(1f, -1f),
                new(1.0f, 0.0f),
                new(1f, 1f),
                new(0.0f, 1.0f),
                new(-1f, 1f),
                new(-1.0f, 0.0f),
                new(-1f, -1f),
                new(0.0f, -1.0f),
            };

            List<DisplayObject> closestObjects = _displayObjects.Where(o => Vector2D.Distance(o.Position, currentCenter) < 0.3f).ToList();

            // Process each other object
            foreach (var otherObject in closestObjects)
            {
                if (otherObject == displayObject) continue;

                // ## SPLIT IN TWO GROUPS ##
                List<Point> newObjectVertices = new(); //the group we will want to make our current polygon
                List<Point> leftOverGroup = new();
                Bisect(displayObject.Position, otherObject.Position, currentVertices, ref newObjectVertices, ref leftOverGroup);

                if (leftOverGroup.Count == 0)
                {
                    // ## NO INTERSECTION FOUND, SO THE CELL IS ALREADY SHORTER ##
                    continue;
                }

                List<Point> shortenedCellVertices = BringPointsAboveLimitCloserToObject(displayObject.Position, newObjectVertices, 0.3f);

                // ## UPDATE THE DISPLAY OBJECT  WITH THE SHORTENED CELL ##
                currentVertices = shortenedCellVertices.ToArray();
                displayObject.SetVertexPositions(newObjectVertices);
            }
        }

        public static void Bisect(Point objectCenter, Point otherCenter, Point[] currentVertices, ref List<Point> groupWithObjectPosition, ref List<Point> groupToProject)
        {
            Line2D line = new(objectCenter, otherCenter);
            Line2D bisector = line.GetMidpointPerpendicular();

            bool isObjectGroupVertex = true;

            //at the start, we estimate that the target object, is at the left of the bisector, in groupWithObjectPosition
            Point previousIntersection = Point.Zero;

            for (int i = 0; i < currentVertices.Length; i++)
            {
                Line2D edge = new(currentVertices[i], currentVertices[(i + 1) % currentVertices.Length]);
                Point intersection = edge.Intersect(bisector, true);
                bool isIntersection = intersection != Point.Zero;
                if (isIntersection && intersection == previousIntersection)
                {
                    // if the intersection is the same as the previous intersection, we have found a loop
                    continue;
                }
                previousIntersection = intersection;

                (isObjectGroupVertex ? groupWithObjectPosition : groupToProject).Add(currentVertices[i]);
                if (isIntersection)
                {
                    (isObjectGroupVertex ? groupWithObjectPosition : groupToProject).Add(intersection); // add the intersection to the first group to begin closing the polygon
                    isObjectGroupVertex = !isObjectGroupVertex;
                    (isObjectGroupVertex ? groupWithObjectPosition : groupToProject).Add(intersection); // add the intersection to the other group to have fully connected polygons
                }
            }

            // ## CHOOSE THE GROUP TO USE ##
            if (IsPointInHalfPlane(objectCenter, groupToProject))
            {
                List<Point> tmpVerticesGroup = groupWithObjectPosition;
                groupWithObjectPosition = groupToProject;
                groupToProject = tmpVerticesGroup;
            }
        }

        public static List<Point> BringPointsAboveLimitCloserToObject(Point objectPosition, List<Point> points, float limit)
        {
            List<Point> result = new();
            foreach (var point in points)
            {
                if (Vector2D.Distance(point, objectPosition) < limit)
                {
                    result.Add(point);
                }
                else
                {
                    //bring the point in the direction but the distance is the limit
                    result.Add(objectPosition + Utils.Normalized(point - objectPosition) * limit);
                }
            }
            return result;
        }

        public static List<Point> projectOtherGroupIntoObjectGroup(Point objectPosition, List<Point> objectGroup, List<Point> groupToProject)
        {
            for (int projIdx = 0; projIdx < groupToProject.Count; projIdx++)
            {

                if (projIdx == 0 || projIdx == groupToProject.Count - 1)
                {
                    // Don't project the first and last vertex becaue it result of the polygon being closed by the bisector 
                    // which is already present in the objectGroup
                    continue;
                }

                bool hasFoundIntersection = false;

                Line2D positionToGroupBVertex = new(objectPosition, groupToProject[projIdx]);
                for (int objGrpIdx = 0; objGrpIdx < objectGroup.Count && !hasFoundIntersection; objGrpIdx++)
                {
                    Line2D objectEdge = new(objectGroup[objGrpIdx], objectGroup[(objGrpIdx + 1) % objectGroup.Count]);
                    Point intersection = objectEdge.Intersect(positionToGroupBVertex);
                    if (intersection != Point.Zero)
                    {
                        objectGroup.Insert(objGrpIdx + 1, intersection);
                        objGrpIdx++;

                        hasFoundIntersection = true;
                    }
                }

                if (!hasFoundIntersection) { throw new Exception("No intersection found to bring vertex from group B to group A"); }
            }

            return objectGroup;
        }

        public static bool IsPointInHalfPlane(Point point, List<Point> verticies)
        {
            HashSet<Point> intersections = new();
            Line2D pointTowardOutside = new(point, point + new Point(15, 15));

            for (int i = 0; i < verticies.Count; i++)
            {
                Point currentVertex = verticies[i];
                Point nextVertex = verticies[(i + 1) % verticies.Count];
                Line2D edge = new(currentVertex, nextVertex);
                Point intersection = edge.Intersect(pointTowardOutside);
                if (intersection != Point.Zero)
                {
                    intersections.Add(intersection);
                }
            }

            return intersections.Count % 2 == 1;
        }
    }

    class Line2D
    {
        public Point PointA;
        public Point PointB;
        public Point Direction;

        public Line2D(Point point1, Point point2)
        {
            PointA = point1;
            PointB = point2;
            Direction = point2 - point1;
        }

        public Line2D GetMidpointPerpendicular()
        {
            Point midpoint = (PointA + PointB) * 0.5f;
            Point perpendicular = new(-Direction.Y, Direction.X);

            return new Line2D(midpoint, midpoint + perpendicular);
        }

        public Point GetMidpoint()
        {
            return (PointA + PointB) * 0.5f;
        }

        public Point Intersect(Line2D other, bool infiniteOther = false)
        {
            // Line 1: this line from PointA to PointB
            // Line 2: other line from other.PointA to other.PointB

            float x1 = PointA.X, y1 = PointA.Y;
            float x2 = PointB.X, y2 = PointB.Y;
            float x3 = other.PointA.X, y3 = other.PointA.Y;
            float x4 = other.PointB.X, y4 = other.PointB.Y;

            // Calculate direction vectors
            float dx1 = x2 - x1;
            float dy1 = y2 - y1;
            float dx2 = x4 - x3;
            float dy2 = y4 - y3;

            // Calculate denominator (cross product of direction vectors)
            float denominator = dx1 * dy2 - dy1 * dx2;

            // Check if lines are parallel
            if (Math.Abs(denominator) < 1e-6f)
            {
                return Point.Zero; // Lines are parallel or colinear
            }

            // Calculate parameters t and u
            float t = ((x3 - x1) * dy2 - (y3 - y1) * dx2) / denominator; //  t is the parameter for the current line,
            float u = ((x3 - x1) * dy1 - (y3 - y1) * dx1) / denominator; //  u is the parameter for the other line

            // Check if intersection is within both line segments
            if ((t >= 0 && t <= 1) && (infiniteOther || (u >= 0 && u <= 1)))
            {
                // Calculate intersection point
                float x = x1 + t * dx1;
                float y = y1 + t * dy1;
                return new Point(x, y);
            }

            return Point.Zero; // No intersection within segments
        }

        public override string ToString()
        {
            return $"A(\x1b[92m{Math.Round((decimal)PointA.X, 2)}, {Math.Round((decimal)PointA.Y, 2)}\x1b[39m)-B(\x1b[92m{Math.Round((decimal)PointB.X, 2)}, {Math.Round((decimal)PointB.Y, 2)}\x1b[39m), D(\x1b[92m{Math.Round((decimal)Direction.X, 2)}, {Math.Round((decimal)Direction.Y, 2)}\x1b[39m)";
        }

    }
}