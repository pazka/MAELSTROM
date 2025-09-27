
using Silk.NET.Maths;

namespace maelstrom_poc
{
    using Point = Vector2D<float>;
    public static class VoronoiTests
    {
        private static bool expectEqualTo<T>(string name, T value, T expected) where T : IEquatable<T>
        {
            if (!value.Equals(expected))
            {
                Console.WriteLine($"\x1b[91m'{name}' failed, got : {value}, expected : {expected}\x1b[39m");
                return false;
            }
            else
            {
                return true;
            }
        }
        private static bool expectDifferentThan<T>(string name, T value, T expected) where T : IEquatable<T>
        {
            if (value.Equals(expected))
            {
                Console.WriteLine($"\x1b[91m'{name}' failed, should be different than : {expected}\x1b[39m");
                return false;
            }
            else
            {
                return true;
            }
        }
        private static bool expectListEqual<T>(string name, List<T> values, List<T> expected) where T : IEquatable<T>
        {
            //test if the lists are equal and print the difference
            bool differences = false;
            List<string> comparisons = new();

            for (int i = 0; i < values.Count || i < expected.Count; i++)
            {
                //test if index is in the list
                if (i < values.Count && i < expected.Count)
                {
                    if (!values[i].Equals(expected[i]))
                    {
                        comparisons.Add($"\x1b[91m{values[i]}\x1b[39m <>  \x1b[91m{expected[i]}\x1b[39m");
                        differences = true;
                    }
                    else
                    {
                        comparisons.Add($"\x1b[92m{values[i]}\x1b[39m =  \x1b[92m{expected[i]}\x1b[39m");
                    }
                }
                else if (i < expected.Count)
                {
                    comparisons.Add($"\x1b[91mnone\x1b[39m <> \x1b[91m{expected[i]}\x1b[39m");
                }
                else if (i < values.Count)
                {
                    comparisons.Add($"\x1b[91m{values[i]}\x1b[39m <> \x1b[91mnone\x1b[39m");
                }
            }

            if (!differences)
            {
                return true;
            }
            else
            {
                Console.WriteLine($"\x1b[91m'{name}' failed, lists are not equal\x1b[39m");
                Console.WriteLine($"Got    <>    Expected");
                foreach (var comparison in comparisons)
                {
                    Console.WriteLine($"{comparison}");
                }
                return false;
            }
        }

        public static bool[] GetIntersectTests()
        {
            List<bool> tests = new();

            Line2D a = new(new Point(-5f, 5f), new Point(5f, 5f));
            Line2D bintersecta = new(new Point(1f, -2f), new Point(1f, 8f));
            Line2D bshort = new(new Point(1f, -2f), new Point(1f, 4f));

            tests.Add(expectDifferentThan("INTERSECT - intersect two segments", a.Intersect(bintersecta), Point.Zero));
            tests.Add(expectEqualTo("INTERSECT - don't intersect two segments", a.Intersect(bshort), Point.Zero));
            tests.Add(expectDifferentThan("INTERSECT - intersect two segment with one infinite", a.Intersect(bshort, true), Point.Zero));
            tests.Add(expectEqualTo("INTERSECT - don't intersect even with with one infinite", bshort.Intersect(a, true), Point.Zero));


            return tests.ToArray();
        }

        public static bool[] GetMidPointTests()
        {
            List<bool> tests = new();

            Point pointA = new(5f, 5f);
            Point pointB = new(7f, 7f);
            Line2D c = new(pointA, pointB);
            Line2D bisector = c.GetMidpointPerpendicular();

            tests.Add(expectEqualTo("MIDPOINT -  perpendicular start point", bisector.PointA, new Point(6f, 6f)));
            tests.Add(expectEqualTo("MIDPOINT - perpendicular end point", bisector.PointB, new Point(4f, 8f)));

            return tests.ToArray();
        }

        public static bool[] GetHalfPlaneTests()
        {
            List<bool> tests = new();

            Point[] verticies = {
                new(0,0),
                new(2,0),
                new(2,2),
                new(0,2),
            };

            tests.Add(expectEqualTo("HALFPANE - Is in", Voronoi.IsPointInHalfPlane(new Point(1f, 1f), [.. verticies]), true));
            tests.Add(expectEqualTo("HALFPANE - Is not in", Voronoi.IsPointInHalfPlane(new Point(3f, 3f), [.. verticies]), false));

            return tests.ToArray();
        }

        public static bool[] GetProjectOtherGroupIntoObjectGroupTests()
        {
            List<bool> tests = new();

            List<Point> resultVertices = Voronoi.projectOtherGroupIntoObjectGroup(new(1f, 1f), new() {
                new(0,0),
                new(2,0),
                new(2,2),
                new(0,2),
            }, new() {
                new(2,0),
                new(3,1),
                new(2,2),
            });

            tests.Add(expectListEqual("Project Verticies into Object Group - bring one inside", resultVertices,
             new List<Point>() {
                new(0,0),
                new(2,0),
                new(2,1),
                new(2,2),
                new(0,2),
            }));


            List<Point> resultVertices2 = Voronoi.projectOtherGroupIntoObjectGroup(new(1f, 1f), new() {
                new(0,0),
                new(2,0),
                new(2,2),
                new(0,2),
            }, new() {
                new(2,0),
                new(3,1),
                new(3,3),
                new(1,3),
                new(2,2),
            });

            tests.Add(expectListEqual("Project Verticies into Object Group - bring 3 inside with overlap", resultVertices2,
             [
                new(0,0),
                new(2,0),
                new(2,1),
                new(2,2),
                new(2,2),
                new(1,2),
                new(0,2),
            ]));

            return tests.ToArray();
        }

        public static bool[] GetSplitIntoGroupsTests()
        {
            List<bool> tests = new();
            List<Point> newVertices = new();
            List<Point> otherVertices = new();

            Voronoi.Bisect(new Point(1f, 2f), new Point(4f, 2f), [
                new(0, 0),
                new(5, 0),
                new(5, 4),
                new(0, 4),
            ], ref newVertices, ref otherVertices);

            tests.Add(expectListEqual("BISECT - first group with object", newVertices,
             new List<Point>() {
                new(0,0),
                new(2.5f,0),
                new(2.5f,4),
                new(0,4),
            }));
            tests.Add(expectListEqual("BISECT - second group without object", otherVertices,
             new List<Point>() {
                new(2.5f,0),
                new(5,0),
                new(5,4),
                new(2.5f,4),
            }));

            newVertices = new();
            otherVertices = new();
            Voronoi.Bisect(new Point(1f, 1f), new Point(3f, 3f), [
                new(0, 0),
                new(4, 0),
                new(4, 4),
                new(0, 4),

            ], ref newVertices, ref otherVertices);
            tests.Add(expectListEqual("BISECT - first group with object with overlap, TODO : maybe change this behaviour", newVertices,
             new List<Point>() {
                new(0,0),
                new(4,0),
                new(0,4),
            }));
            tests.Add(expectListEqual("BISECT - second group without object with overlap, TODO : maybe change this behaviour", otherVertices,
             new List<Point>() {
                new(4,0),
                new(4,4),
                new(0,4),
            }));

            newVertices = new();
            otherVertices = new();
            Voronoi.Bisect(new Point(2f, 2f), new Point(3f, 3f), [
                new(0, 0),
                new(4, 0),
                new(4, 4),
                new(0, 4),

            ], ref newVertices, ref otherVertices);
            tests.Add(expectListEqual("BISECT - first group with object with asymetry, TODO : maybe change this behaviour", newVertices,
             new List<Point>() {
                new(0,0),
                new(4,0),
                new(4,1),
                new(1,4),
                new(0,4),
            }));
            tests.Add(expectListEqual("BISECT - second group without object with asymetry, TODO : maybe change this behaviour", otherVertices,
             new List<Point>() {
                new(4,1),
                new(4,4),
                new(1,4),
            }));

            return tests.ToArray();
        }

        public static void Test()
        {
            List<bool> tests = new();

            tests.AddRange(GetIntersectTests());
            tests.AddRange(GetMidPointTests());
            tests.AddRange(GetHalfPlaneTests());
            tests.AddRange(GetProjectOtherGroupIntoObjectGroupTests());
            tests.AddRange(GetSplitIntoGroupsTests());

            if (tests.Any(t => !t))
            {
                throw new Exception("Some tests failed");
            }
            else
            {
                Console.WriteLine("✅ \x1b[92mAll tests passed\x1b[39m");
            }
        }
    }
}