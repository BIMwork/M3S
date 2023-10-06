using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIMwork
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class XRightAlign : IExternalCommand
    {
        private const string X_RIGHT_ALIGN = "X_RIGHT_ALIGN";
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


                using (Transaction trans = new Transaction(doc, X_RIGHT_ALIGN))
                {
                    trans.Start();
                    // get minX
                    XYZ targetPoint = caclulateTargetPoint(ref doc, ref view, ref ids);

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
                                handleTextNotes(ref view, ref el, targetPoint);
                                break;
                            // tag
                            case TAG_CATEGORY_NAME:
                                handleTag(ref view, ref el, targetPoint);
                                break;
                            // line
                            case LINE_CATEGORY_NAME:
                                handleLine(ref doc, ref view, ref el, ids[i], targetPoint);
                                break;
                            default:
                                // handleNone(ref view, ref el, minX);
                                break;
                        }
                    }
                    trans.Commit();
                    trans.Dispose();
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }


        private XYZ caclulateTargetPoint(ref Document doc, ref View view, ref List<ElementId> ids)
        {
            XYZ targetPoint = null;
            double max = double.MinValue;
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
                if (max < box.Max.X)
                {
                    max = box.Max.X;
                    targetPoint = new XYZ(box.Max.X, box.Max.Y, box.Max.Z);
                }
            }
            return targetPoint;
        }


        private void handleTextNotes(ref View view, ref Element el, XYZ targetPoint)
        {
            TextNote txtNode = el as TextNote;
            XYZ currentPoint = txtNode.Coord;
            BoundingBoxXYZ box = txtNode.get_BoundingBox(view);
            double boxMaxX = box.Max.X;
            if (boxMaxX != targetPoint.X)
            {
                double len = Math.Abs(currentPoint.X - boxMaxX);
                double targetX = targetPoint.X - len;
                XYZ newPoint = new XYZ(targetX, currentPoint.Y, currentPoint.Z);
                txtNode.Coord = newPoint;
            }
        }

        private void handleTag(ref View view, ref Element el, XYZ targetPoint)
        {
            IndependentTag iTag = el as IndependentTag;
            XYZ currentPoint = iTag.TagHeadPosition;
            BoundingBoxXYZ box = iTag.get_BoundingBox(view);
            double boxMaxX = box.Max.X;
            if (boxMaxX != targetPoint.X)
            {
                double len = Math.Abs(currentPoint.X - boxMaxX);
                double targetX = targetPoint.X - len;

                XYZ newPoint = new XYZ(targetX, currentPoint.Y, currentPoint.Z);
                iTag.TagHeadPosition = newPoint;
            }
        }

        private void handleLine(ref Document doc, ref View view, ref Element el, ElementId elId, XYZ targetPoint)
        {
            DetailLine detailLine = el as DetailLine;
            Line xline = detailLine.GeometryCurve as Line;
            XYZ currentStart = xline.GetEndPoint(0);
            XYZ currentEnd = xline.GetEndPoint(1);

            BoundingBoxXYZ box = detailLine.get_BoundingBox(view);
            double boxMaxX = box.Max.X;
            if (boxMaxX != targetPoint.X)
            {
                double newStartX = targetPoint.X;
                Parameter lengthParam = el.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
                double length = lengthParam.AsDouble();

                XYZ newStart = new XYZ(newStartX, currentStart.Y, currentStart.Z);
                XYZ endStart = new XYZ(newStart.X - length, currentEnd.Y, currentEnd.Z);
                Line newLine = Line.CreateBound(newStart, endStart);
                doc.Create.NewDetailCurve(view, newLine);
                doc.Delete(elId);
            }
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

