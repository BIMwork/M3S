using Autodesk.Revit.DB;

namespace BIMwork
{
    public class TargetPoint
    {
        public XYZ elbow;
        public XYZ end;

        public TargetPoint(XYZ elbow, XYZ end)
        {
            this.elbow = elbow;
            this.end = end;
        }
    }
}
