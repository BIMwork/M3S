using System;
using Autodesk.Revit.DB;

namespace BIMwork
{
    public static class Angle60Utils
    {
        private const double ANGLE_TARGET = 60;

        // oxz
        private static void calcNewElbowTextNotesOXZ(ref XYZ end, ref XYZ elbow, ref XYZ coord)
        {
            // oxz -> oxy
            // ox -> ox
            // oz -> oy
            double absDz = Math.Abs(elbow.Z - end.Z);
            double lenX = absDz / Math.Tan(Convert.ToDouble(ANGLE_TARGET) * Math.PI / 180.0);
            double newElbowX = elbow.X - lenX;
            if (coord.X > elbow.X)
            {
                newElbowX = elbow.X + lenX;
            }
            XYZ newElbow = new XYZ(newElbowX, 0, elbow.Z);
            elbow = newElbow;
        }

        public static TargetPoint handleTextNotes60OXZ(ref XYZ coord, ref XYZ anchor, ref XYZ elbow, ref XYZ end)
        {
            TargetPoint targetPoint = null;

            // update
            // 3 điểm thẳng hàng chéo
            XYZ angle90Elbow = new XYZ(end.X, 0, anchor.Z);
            XYZ angle90End = new XYZ(end.X, 0, end.Z);

            /*// anchor.Y == end.Y
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
            }*/

            calcNewElbowTextNotesOXZ(ref angle90End, ref angle90Elbow, ref coord);

            targetPoint = new TargetPoint(angle90Elbow, angle90End);
            return targetPoint;
        }

        // oxy
        private static void calcNewElbowTextNotesOXY(ref XYZ end, ref XYZ elbow, ref XYZ coord)
        {
            /* double absDy = Math.Abs(elbow.Y - end.Y);
             double lenX = absDy / Math.Tan(Convert.ToDouble(ANGLE_TARGET) * Math.PI / 180.0);
             lenX = Math.Abs(lenX);
             double newEndX = end.X + lenX;
             if (end.X < coord.X)
             {
                 newEndX = end.X - lenX;

             }
             XYZ newEnd = new XYZ(newEndX, end.Y, end.X);
             end = newEnd;*/


            double absDy = Math.Abs(end.Y - elbow.Y);
            double lenX = absDy / Math.Tan(Convert.ToDouble(ANGLE_TARGET) * Math.PI / 180.0);
            double newElbowX = elbow.X - lenX;
            if (coord.X > elbow.X)
            {
                newElbowX = elbow.X + lenX;
            }
            XYZ newElbow = new XYZ(newElbowX, elbow.Y, elbow.X);
            elbow = newElbow;
        }

        public static TargetPoint handleTextNotes60OXY(ref XYZ coord, ref XYZ anchor, ref XYZ elbow, ref XYZ end)
        {
            TargetPoint targetPoint = null;

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

            calcNewElbowTextNotesOXY(ref angle90End, ref angle90Elbow, ref coord);

            targetPoint = new TargetPoint(angle90Elbow, angle90End);
            return targetPoint;
        }

        // oyz
        private static void calcNewElbowTextNotesOYZ(ref XYZ end, ref XYZ elbow, ref XYZ coord)
        {
            // oyz -> oxy
            // ox -> oy
            // oy -> oz
            double absDz = Math.Abs(elbow.Z - end.Z);
            double lenY = absDz / Math.Tan(Convert.ToDouble(ANGLE_TARGET) * Math.PI / 180.0);
            double newElbowY = elbow.Y - lenY;
            if (coord.Y > elbow.Y)
            {
                newElbowY = elbow.Y + lenY;
            }
            XYZ newElbow = new XYZ(0, newElbowY, elbow.Z);
            elbow = newElbow;
        }

        public static TargetPoint handleTextNotes60OYZ(ref XYZ coord, ref XYZ anchor, ref XYZ elbow, ref XYZ end)
        {
            TargetPoint targetPoint = null;

            // update
            // 3 điểm thẳng hàng chéo
            XYZ angle90Elbow = new XYZ(0, end.Y, anchor.Z);
            XYZ angle90End = new XYZ(0, end.Y, end.Z);

            /*// anchor.Y == end.Y
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
            }*/

            calcNewElbowTextNotesOYZ(ref angle90End, ref angle90Elbow, ref coord);

            targetPoint = new TargetPoint(angle90Elbow, angle90End);
            return targetPoint;
        }



        // tag
        // oxz
        private static void calcNewElbowTagOXZ(ref XYZ tagHeadPosition, ref XYZ end, ref XYZ elbow)
        {
            // oxz -> oxy
            // ox -> ox
            // oz -> oy
            double absDz = Math.Abs(elbow.Z - end.Z);
            double lenX = absDz / Math.Tan(Convert.ToDouble(ANGLE_TARGET) * Math.PI / 180.0);
            double newElbowX = elbow.X - lenX;
            if (tagHeadPosition.X > elbow.X)
            {
                newElbowX = elbow.X + lenX;
            }
            XYZ newElbow = new XYZ(newElbowX, 0, elbow.Z);
            elbow = newElbow;
        }

        public static TargetPoint handleTag60OXZ(ref XYZ anchor, ref XYZ elbow, ref XYZ end)
        {
            TargetPoint targetPoint;

            elbow = new XYZ(end.X, 0, end.Z);
            XYZ angle90Elbow = new XYZ(end.X, 0, anchor.Z);
            XYZ angle90End = new XYZ(end.X, 0, end.Z);

            calcNewElbowTagOXZ(ref anchor, ref angle90End, ref angle90Elbow);
            targetPoint = new TargetPoint(angle90Elbow, angle90End);
            return targetPoint;
        }

        // oyz
        private static void calcNewElbowTagOYZ(ref XYZ tagHeadPosition, ref XYZ end, ref XYZ elbow)
        {
            // oyz -> oxy
            // ox -> oy
            // oy -> oz
            double absDz = Math.Abs(elbow.Z - end.Z);
            double lenY = absDz / Math.Tan(Convert.ToDouble(ANGLE_TARGET) * Math.PI / 180.0);
            double newElbowY = elbow.Y - lenY;
            if (tagHeadPosition.Y > elbow.Y)
            {
                newElbowY = elbow.Y + lenY;
            }
            XYZ newElbow = new XYZ(0, newElbowY, elbow.Z);
            elbow = newElbow;
        }

        public static TargetPoint handleTag60OYZ(ref XYZ anchor, ref XYZ elbow, ref XYZ end)
        {
            TargetPoint targetPoint = null;

            // oyz -> oxy
            elbow = new XYZ(0, anchor.Y, end.Z);
            XYZ angle90Elbow = new XYZ(0, end.Y, anchor.Z);
            XYZ angle90End = new XYZ(0, end.Y, end.Z);

            calcNewElbowTagOYZ(ref anchor, ref angle90End, ref angle90Elbow);
            targetPoint = new TargetPoint(angle90Elbow, angle90End);
            return targetPoint;
        }

        // oxy
        private static void calcNewElbowTagOXY(ref XYZ tagHeadPosition, ref XYZ end, ref XYZ elbow)
        {
            /* double absDy = Math.Abs(elbow.Y - end.Y);
             double lenX = absDy / Math.Tan(Convert.ToDouble(ANGLE_TARGET) * Math.PI / 180.0);
             lenX = Math.Abs(lenX);
             double newEndX = end.X + lenX;
             if (tagHeadPosition.X > elbow.X)
             {
                 newEndX = end.X - lenX;
             }
             XYZ newEnd = new XYZ(newEndX, end.Y, end.X);
             end = newEnd;*/

            double absDy = Math.Abs(elbow.Y - end.Y);
            double lenX = absDy / Math.Tan(Convert.ToDouble(ANGLE_TARGET) * Math.PI / 180.0);
            double newElbowX = elbow.X - lenX;
            if (tagHeadPosition.X > elbow.X)
            {
                newElbowX = elbow.X + lenX;
            }
            XYZ newElbow = new XYZ(newElbowX, elbow.Y, 0);
            elbow = newElbow;

        }

        public static TargetPoint handleTag60OXY(ref XYZ anchor, ref XYZ elbow, ref XYZ end)
        {
            TargetPoint targetPoint = null;

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

            calcNewElbowTagOXY(ref anchor, ref angle90End, ref angle90Elbow);
            targetPoint = new TargetPoint(angle90Elbow, angle90End);
            return targetPoint;
        }
    }
}
