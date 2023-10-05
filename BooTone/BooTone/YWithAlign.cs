using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace BooTone
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class YWithAlign : IExternalCommand
    {
        private const string Y_WITH_ALIGN = "Y_WITH_ALIGN";
        private const string TEXT_NOTE_CATEGOR_NAME = "Text Notes";
        private const string TAG_CATEGORY_NAME = "Structural Framing Tags";
        private const string LINE_CATEGORY_NAME = "Lines";

        private double stepLen = 0;
        private BoundingBoxXYZ boxMin;
        private BoundingBoxXYZ boxMax;
        private double pointLen = 0;

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
                if (count <= 2)
                {
                    message = "3つ以上を選択してください。";
                    return Result.Failed;
                }

                using (Transaction trans = new Transaction(doc, Y_WITH_ALIGN))
                {
                    trans.Start();
                    // get middle
                    bool isCaclulateLen = caclulateLen(ref doc, ref view, ref ids);
                    if (!isCaclulateLen)
                    {
                        message = "調整できない";
                        trans.Commit();
                        trans.Dispose();
                        return Result.Failed;
                    }

                    List<ElementId> sorted = sortData(doc, view, ids);

                    for (int i = 0; i < sorted.Count; i++)
                    {
                        Element el = doc.GetElement(sorted[i]);
                        if (el == null)
                        {
                            continue;
                        }
                        switch (el.Category.Name)
                        {
                            // text notes
                            case TEXT_NOTE_CATEGOR_NAME:
                                handleTextNotes(ref view, ref el);
                                break;
                            // tag
                            case TAG_CATEGORY_NAME:
                                handleTag(ref view, ref el);
                                break;
                            // line
                            case LINE_CATEGORY_NAME:
                                handleLine(ref doc, ref view, ref el, ids[i]);
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

        private List<ElementId> sortData(Document doc, View view, List<ElementId> ids)
        {
            //Sort the array in ascending order using two for loops    
            for (int i = 0; i < ids.Count; i++)
            {
                for (int j = i + 1; j < ids.Count; j++)
                {
                    Element elI = doc.GetElement(ids[i]);
                    BoundingBoxXYZ boxI = elI.get_BoundingBox(view);

                    Element elJ = doc.GetElement(ids[j]);
                    BoundingBoxXYZ boxJ = elJ.get_BoundingBox(view);
                    if (boxI.Min.Y > boxJ.Min.Y)
                    {
                        //swap elements if not in order
                        ElementId temp = ids[i];
                        ids[i] = ids[j];
                        ids[j] = temp;
                    }
                }
            }
            return ids;
        }


        private bool caclulateLen(ref Document doc, ref View view, ref List<ElementId> ids)
        {
            bool isCaclulateLen;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            double totalBoundingBox = 0;

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
                totalBoundingBox = totalBoundingBox + Math.Abs(box.Max.Y - box.Min.Y);
                if (box.Min.Y < minY)
                {
                    minY = box.Min.Y;
                    boxMin = box;
                }
                if (box.Max.Y > maxY)
                {
                    maxY = box.Max.Y;
                    boxMax = box;
                }
            }

            double total = Math.Abs(boxMax.Max.Y - boxMin.Min.Y);
            double totalSpace = total - totalBoundingBox;
            stepLen = totalSpace / (ids.Count - 1);

            isCaclulateLen = stepLen <= 0 ? false : true;

            return isCaclulateLen;
        }


        private void handleTextNotes(ref View view, ref Element el)
        {
            TextNote txtNode = el as TextNote;
            XYZ currentPoint = txtNode.Coord;

            BoundingBoxXYZ box = txtNode.get_BoundingBox(view);
            double boxMinY = box.Min.Y;
            double boxMaxY = box.Max.Y;

            if (boxMinY == boxMin.Min.Y || boxMinY == boxMax.Min.Y)
            {
                return;
            }
            double len = pointLen == 0 ?
                stepLen - (boxMinY - boxMin.Max.Y)
                : stepLen - (boxMinY - pointLen);
            pointLen = boxMaxY + len;
            double targetY = currentPoint.Y + len;
            XYZ newPoint = new XYZ(currentPoint.X, targetY, currentPoint.Z);

            txtNode.Coord = newPoint;
        }


        private void handleTag(ref View view, ref Element el)
        {
            IndependentTag iTag = el as IndependentTag;
            XYZ currentPoint = iTag.TagHeadPosition;

            BoundingBoxXYZ box = iTag.get_BoundingBox(view);
            double boxMinY = box.Min.Y;
            double boxMaxY = box.Max.Y;

            if (boxMinY == boxMin.Min.Y || boxMinY == boxMax.Min.Y)
            {
                return;
            }
            double len = pointLen == 0 ?
                stepLen - (boxMinY - boxMin.Max.Y)
                : stepLen - (boxMinY - pointLen);
            pointLen = boxMaxY + len;
            double targetY = currentPoint.Y + len;
            XYZ newPoint = new XYZ(currentPoint.X, targetY, currentPoint.Z);

            iTag.TagHeadPosition = newPoint;
        }


        private void handleLine(ref Document doc, ref View view, ref Element el,
            ElementId elId)
        {
            DetailLine detailLine = el as DetailLine;
            Line xline = detailLine.GeometryCurve as Line;
            XYZ currentStart = xline.GetEndPoint(0);
            XYZ currentEnd = xline.GetEndPoint(1);

            BoundingBoxXYZ box = detailLine.get_BoundingBox(view);
            double boxMinY = box.Min.Y;
            double boxMaxY = box.Max.Y;

            if (boxMinY == boxMin.Min.Y || boxMinY == boxMax.Min.Y)
            {
                return;
            }

            double len = pointLen == 0 ?
                stepLen - (boxMinY - boxMin.Max.Y)
                : stepLen - (boxMinY - pointLen);
            pointLen = boxMaxY + len;

            XYZ newStart = new XYZ(currentStart.X, currentStart.Y + len, currentStart.Z);
            XYZ endStart = new XYZ(currentEnd.X, currentEnd.Y + len, currentEnd.Z);

            Line newLine = Line.CreateBound(newStart, endStart);
            doc.Create.NewDetailCurve(view, newLine);
            doc.Delete(elId);
        }

        private void handleNone(ref View view, ref Element el, double minX)
        {
            // todo hainv
        }

    }
}
