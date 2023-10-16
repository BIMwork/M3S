using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIMwork
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class YCenterAlign : IExternalCommand
    {
        private const string Y_CENTER_ALIGN = "Y_CENTER_ALIGN";
        private const string LINE_CATEGORY_NAME = "Autodesk.Revit.DB.DetailLine";
        private const string TAG_CATEGORY_NAME = "Autodesk.Revit.DB.IndependentTag";
        private const string TEXT_NOTE_CATEGOR_NAME = "Autodesk.Revit.DB.TextNote";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // get UIDocument
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;

                // get Document
                Document doc = uiDoc.Document;

                // get View
                View view = doc.ActiveView;

                /*IList<Reference> refEls = uiDoc.Selection.PickObjects(ObjectType.Element);
                List<ElementId> ids = (from Reference r in refEls select r.ElementId).ToList();*/

                List<ElementId> ids = (List<ElementId>)uiDoc.Selection.GetElementIds();

                int count = ids.Count();
                if (count <= 1)
                {
                    message = "2つ以上を選択してください。";
                    return Result.Failed;
                }


                using (Transaction trans = new Transaction(doc, Y_CENTER_ALIGN))
                {
                    trans.Start();
                    // get middle
                    double middleY = caclulateMiddle(ref doc, ref view, ref ids);

                    for (int i = 0; i < count; i++)
                    {
                        Element el = doc.GetElement(ids[i]);
                        if (el == null)
                        {
                            continue;
                        }
                        string elType = el.GetType().ToString();
                        switch (elType)
                        {
                            // text notes
                            case TEXT_NOTE_CATEGOR_NAME:
                                handleTextNotes(ref view, ref el, middleY);
                                break;
                            // tag
                            case TAG_CATEGORY_NAME:
                                handleTag(ref view, ref el, middleY);
                                break;
                            // line
                            case LINE_CATEGORY_NAME:
                                handleLine(ref doc, ref view, ref el, ids[i], middleY);
                                break;
                            default:
                                // handleNone(ref view, ref el, minX);
                                break;
                        }
                    }
                    trans.Commit();
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private double caclulateMiddle(ref Document doc, ref View view, ref List<ElementId> ids)
        {
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            for (int i = 0; i < ids.Count; i++)
            {
                Element el = doc.GetElement(ids[i]);
                BoundingBoxXYZ box;
                switch (el.Category.Name)
                {
                    // text notes
                    case TEXT_NOTE_CATEGOR_NAME:
                        TextNote txtNode = el as TextNote;
                        box = txtNode.get_BoundingBox(view);
                        break;
                    // tag
                    case TAG_CATEGORY_NAME:
                        IndependentTag iTag = el as IndependentTag;
                        box = iTag.get_BoundingBox(view);
                        break;
                    // line
                    case LINE_CATEGORY_NAME:
                        DetailLine detailLine = el as DetailLine;
                        box = detailLine.get_BoundingBox(view);
                        break;
                    default:
                        box = el.get_BoundingBox(view);
                        break;
                }
                if (box.Min.Y < minY)
                {
                    minY = box.Min.Y;
                }
                if (box.Max.Y > maxY)
                {
                    maxY = box.Max.Y;
                }
            }
            double middle = Math.Abs(maxY - minY) / 2;
            return minY + middle;
        }

        private void handleTextNotes(ref View view, ref Element el, double middleY)
        {
            TextNote txtNode = el as TextNote;
            XYZ currentPoint = txtNode.Coord;
            BoundingBoxXYZ box = el.get_BoundingBox(view);
            double boxMinY = box.Min.Y;
            double boxMaxY = box.Max.Y;

            double boxMiddle = boxMinY + Math.Abs(boxMaxY - boxMinY) / 2;
            double len = middleY - boxMiddle;

            double targetY = currentPoint.Y + len;
            XYZ newPoint = new XYZ(currentPoint.X, targetY, currentPoint.Z);
            txtNode.Coord = newPoint;
        }


        private void handleTag(ref View view, ref Element el, double middleY)
        {
            IndependentTag iTag = el as IndependentTag;
            XYZ currentPoint = iTag.TagHeadPosition;
            BoundingBoxXYZ box = el.get_BoundingBox(view);
            double boxMinY = box.Min.Y;
            double boxMaxY = box.Max.Y;

            double boxMiddle = boxMinY + Math.Abs(boxMaxY - boxMinY) / 2;
            double len = middleY - boxMiddle;

            double targetY = currentPoint.Y + len;
            XYZ newPoint = new XYZ(currentPoint.X, targetY, currentPoint.Z);
            iTag.TagHeadPosition = newPoint;
        }


        private void handleLine(ref Document doc, ref View view, ref Element el, ElementId elId, double middleY)
        {
            DetailLine detailLine = el as DetailLine;
            Line xline = detailLine.GeometryCurve as Line;
            XYZ currentStart = xline.GetEndPoint(0);
            XYZ currentEnd = xline.GetEndPoint(1);

            BoundingBoxXYZ box = el.get_BoundingBox(view);
            double boxMinY = box.Min.Y;
            double boxMaxY = box.Max.Y;
            double boxMiddle = boxMinY + Math.Abs(boxMaxY - boxMinY) / 2;
            double len = middleY - boxMiddle;


            XYZ newStart = new XYZ(currentStart.X, currentStart.Y + len, currentStart.Z);
            XYZ endStart = new XYZ(currentEnd.X, currentEnd.Y + len, currentEnd.Z);
            Line newLine = Line.CreateBound(newStart, endStart);
            doc.Create.NewDetailCurve(view, newLine);
            doc.Delete(elId);
        }

        private void handleNone(ref View view, ref Element el, double minX)
        {
            LocationPoint loc = el.Location as LocationPoint;
            XYZ currentPoint = loc.Point;
            BoundingBoxXYZ box = el.get_BoundingBox(view);
            if (box.Min.X > minX)
            {
                double xPoint = minX + (box.Max.X - box.Min.X) / 2;
                XYZ newPoint = new XYZ(xPoint, currentPoint.Y, currentPoint.Z);
                loc.Point = newPoint;
            }
        }
    }
}




