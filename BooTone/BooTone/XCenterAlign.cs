using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace BooTone
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class XCenterAlign : IExternalCommand
    {
        private const string X_CENTER_ALIGN = "X_CENTER_ALIGN";
        private const string TEXT_NOTE_CATEGOR_NAME = "Text Notes";
        private const string TAG_CATEGORY_NAME = "Structural Framing Tags";
        private const string LINE_CATEGORY_NAME = "Lines";

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

               /* IList<Reference> refEls = uiDoc.Selection.PickObjects(ObjectType.Element);
                List<ElementId> ids = (from Reference r in refEls select r.ElementId).ToList();*/

                List<ElementId> ids = (List<ElementId>)uiDoc.Selection.GetElementIds();

                int count = ids.Count();
                if (count <= 1)
                {
                    message = "2つ以上を選択してください。";
                    return Result.Failed;
                }


                using (Transaction trans = new Transaction(doc, X_CENTER_ALIGN))
                {
                    trans.Start();
                    // get middle
                    double middleX = caclulateMiddle(ref doc, ref view, ref ids);

                    for (int i = 0; i < count; i++)
                    {
                        Element el = doc.GetElement(ids[i]);
                        if (el == null)
                        {
                            continue;
                        }
                        switch (el.Category.Name)
                        {
                            // text notes
                            case TEXT_NOTE_CATEGOR_NAME:
                                handleTextNotes(ref view, ref el, middleX);
                                break;
                            // tag
                            case TAG_CATEGORY_NAME:
                                handleTag(ref view, ref el, middleX);
                                break;
                            // line
                            case LINE_CATEGORY_NAME:
                                handleLine(ref doc, ref view, ref el, ids[i], middleX);
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
            double minX = double.MaxValue;
            double maxX = double.MinValue;
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
                if (box.Min.X < minX)
                {
                    minX = box.Min.X;
                }
                if (box.Max.X > maxX)
                {
                    maxX = box.Max.X;
                }
            }
            double middle = Math.Abs(maxX - minX) / 2;
            return minX + middle;
        }

        private void handleTextNotes(ref View view, ref Element el, double middleX)
        {
            TextNote txtNode = el as TextNote;
            XYZ currentPoint = txtNode.Coord;
            BoundingBoxXYZ box = el.get_BoundingBox(view);
            double boxMinX = box.Min.X;
            double boxMaxX = box.Max.X;

            double boxMiddle = boxMinX + Math.Abs(boxMaxX - boxMinX) / 2;
            double len = middleX - boxMiddle;

            double targetX = currentPoint.X + len;
            XYZ newPoint = new XYZ(targetX, currentPoint.Y, currentPoint.Z);
            txtNode.Coord = newPoint;
        }


        private void handleTag(ref View view, ref Element el, double middleX)
        {
            IndependentTag iTag = el as IndependentTag;
            XYZ currentPoint = iTag.TagHeadPosition;
            BoundingBoxXYZ box = iTag.get_BoundingBox(view);

            double boxMinX = box.Min.X;
            double boxMaxX = box.Max.X;

            double boxMiddle = boxMinX + Math.Abs(boxMaxX - boxMinX) / 2;
            double len = middleX - boxMiddle;

            double targetX = currentPoint.X + len;
            XYZ newPoint = new XYZ(targetX, currentPoint.Y, currentPoint.Z);
            iTag.TagHeadPosition = newPoint;
        }


        private void handleLine(ref Document doc, ref View view, ref Element el, ElementId elId, double middleX)
        {
            DetailLine detailLine = el as DetailLine;
            Line xline = detailLine.GeometryCurve as Line;
            XYZ currentStart = xline.GetEndPoint(0);
            XYZ currentEnd = xline.GetEndPoint(1);

            Parameter lengthParam = el.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
            double length = lengthParam.AsDouble();


            XYZ newStart = new XYZ(middleX - length / 2, currentStart.Y, currentStart.Z);
            XYZ endStart = new XYZ(middleX + length / 2, currentEnd.Y, currentEnd.Z);
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




