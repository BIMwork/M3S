using System;
using Autodesk.Revit.DB;

namespace BIMwork
{
    public static class AngleUtils
    {
        public static string OXY = "oxy";
        public static string OXZ = "oxz";
        public static string OYZ = "oyz";

        public static string getCoordinateAxisTextNote(ref XYZ coord)
        {
            // oyz
            double coordX = Math.Round(coord.X, 5);
            double coordY = Math.Round(coord.Y, 5);
            double coordZ = Math.Round(coord.Z, 5);
            if (coordX == 0)
            {
                return OYZ;
            }

            // oxz
            if (coordY == 0)
            {
                return OXZ;
            }

            // oxy
            if (coordZ == 0)
            {
                return OXY;
            }
            return "";
        }

        public static string getCoordinateAxisTag(ref XYZ elbow, ref XYZ end)
        {
            double elbowX = Math.Round(elbow.X, 5);
            double endX = Math.Round(end.X, 5);

            double elbowY = Math.Round(elbow.Y, 5);
            double endY = Math.Round(end.Y, 5);

            double elbowZ = Math.Round(elbow.Z, 5);
            double endZ = Math.Round(end.Z, 5);

            // oyz
            bool isOYZ = (elbowX == endX || endX == 0) && endY != 0 && elbowZ != 0;
            if (isOYZ)
            {
                return OYZ;
            }

            // oxz
            bool isOXZ = (elbowY == endY || endY == 0) && endX != 0 && elbowZ != 0;
            if (isOXZ)
            {
                return OXZ;
            }

            // oxy
            bool isOXY = (elbowZ == endZ || endZ == 0) && endX != 0 && elbowY != 0;
            if (isOXY)
            {
                return OXY;
            }
            return "";
        }
    }
}
