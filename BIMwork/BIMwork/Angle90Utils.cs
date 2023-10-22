using System;
using Autodesk.Revit.DB;

namespace BIMwork
{
    public static class Angle90Utils
    {
        public static TargetPoint handleTextNotes90OXZ(ref XYZ coord, ref XYZ anchor, ref XYZ elbow, ref XYZ end)
        {
            TargetPoint targetPoint;

            elbow = new XYZ(end.X, 0, end.Z);
            XYZ angle90Elbow = new XYZ(end.X, 0, anchor.Z);
            XYZ angle90End = new XYZ(end.X, 0, end.Z);

            targetPoint = new TargetPoint(angle90Elbow, angle90End);
            return targetPoint;
        }

        public static TargetPoint handleTextNotes90OXY(ref XYZ coord, ref XYZ anchor, ref XYZ elbow, ref XYZ end)
        {
            TargetPoint targetPoint;

            // update
            // 3 điểm thẳng hàng chéo
            XYZ angle90Elbow = new XYZ(end.X, anchor.Y, 0);
            XYZ angle90End = new XYZ(end.X, end.Y, 0);

            // anchor.Y == end.Y
            if (Math.Abs(anchor.Y - end.Y) <= 0.0001)
            {
                double middle = (anchor.X - end.X) / 2;
                double elbowY = coord.Y > elbow.Y ? anchor.Y - Math.Abs(middle) : anchor.Y + Math.Abs(middle);

                angle90Elbow = new XYZ(anchor.X - middle, elbowY, 0);
            }

            // anchor.X == end.X
            if (Math.Abs(anchor.X - end.X) <= 0.0001)
            {
                double middle = (anchor.Y - end.Y) / 2;
                double elbowX = coord.X > anchor.X ? anchor.X - Math.Abs(middle) : anchor.X + Math.Abs(middle);

                angle90Elbow = new XYZ(elbowX, anchor.Y - middle, 0);
            }

            targetPoint = new TargetPoint(angle90Elbow, angle90End);
            return targetPoint;
        }

        public static TargetPoint handleTextNotes90OYZ(ref XYZ coord, ref XYZ anchor, ref XYZ elbow, ref XYZ end)
        {
            TargetPoint targetPoint;

            elbow = new XYZ(0, anchor.Y, end.Z);
            XYZ angle90Elbow = new XYZ(0, end.Y, anchor.Z);
            XYZ angle90End = new XYZ(0, end.Y, end.Z);

            targetPoint = new TargetPoint(angle90Elbow, angle90End);
            return targetPoint;
        }

        public static TargetPoint handleTag90OXZ(ref XYZ anchor, ref XYZ elbow, ref XYZ end)
        {
            TargetPoint targetPoint;

            elbow = new XYZ(end.X, 0, end.Z);
            XYZ angle90Elbow = new XYZ(end.X, 0, anchor.Z);
            XYZ angle90End = new XYZ(end.X, 0, end.Z);

            targetPoint = new TargetPoint(angle90Elbow, angle90End);
            return targetPoint;
        }

        public static TargetPoint handleTag90OYZ(ref XYZ anchor, ref XYZ elbow, ref XYZ end)
        {
            TargetPoint targetPoint;

            elbow = new XYZ(0, anchor.Y, end.Z);
            XYZ angle90Elbow = new XYZ(0, end.Y, anchor.Z);
            XYZ angle90End = new XYZ(0, end.Y, end.Z);

            targetPoint = new TargetPoint(angle90Elbow, angle90End);
            return targetPoint;
        }

        public static TargetPoint handleTag90OXY(ref XYZ anchor, ref XYZ elbow, ref XYZ end)
        {
            elbow = new XYZ(end.X, anchor.Y, end.Z);
            XYZ angle90Elbow = new XYZ(end.X, anchor.Y, 0);
            XYZ angle90End = new XYZ(end.X, end.Y, 0);

            // anchor.Y == end.Y
            if (Math.Abs(anchor.Y - end.Y) <= 0.0001)
            {
                double middle = (anchor.X - end.X) / 2;
                double elbowY = anchor.Y > elbow.Y ? anchor.Y - Math.Abs(middle) : anchor.Y + Math.Abs(middle);

                angle90Elbow = new XYZ(anchor.X - middle, elbowY, 0);
            }

            // anchor.X == end.X
            if (Math.Abs(anchor.X - end.X) <= 0.0001)
            {
                double middle = (anchor.Y - end.Y) / 2;
                double elbowX = anchor.X > anchor.X ? anchor.X - Math.Abs(middle) : anchor.X + Math.Abs(middle);

                angle90Elbow = new XYZ(elbowX, anchor.Y - middle, 0);
            }

            TargetPoint targetPoint = new TargetPoint(angle90Elbow, angle90End);
            return targetPoint;
        }
    }
}
