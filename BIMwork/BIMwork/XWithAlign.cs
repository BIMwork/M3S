using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIMwork
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class XWithAlign : IExternalCommand
    {
        private const string X_WITH_ALIGN = "X_WITH_ALIGN";
        private const string LINE_CATEGORY_NAME = "Autodesk.Revit.DB.DetailLine";
        private const string TAG_CATEGORY_NAME = "Autodesk.Revit.DB.IndependentTag";
        private const string TEXT_NOTE_CATEGOR_NAME = "Autodesk.Revit.DB.TextNote";

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

                /*IList<Reference> refEls = uiDoc.Selection.PickObjects(ObjectType.Element);
                List<ElementId> ids = (from Reference r in refEls select r.ElementId).ToList();*/

                List<ElementId> ids = (List<ElementId>)uiDoc.Selection.GetElementIds();

                int count = ids.Count();
                if (count <= 2)
                {
                    message = "3つ以上を選択してください。";
                    return Result.Failed;
                }

                using (Transaction trans = new Transaction(doc, X_WITH_ALIGN))
                {
                    trans.Start();
                    // get middle
                    bool isCaclulateLen = caclulateLen(ref doc, ref view, ref ids);
                    /* if (!isCaclulateLen)
                     {
                         message = "移動できません。";
                         trans.Commit();
                         trans.Dispose();
                         return Result.Failed;
                     }*/

                    List<ElementId> sorted = sortData(doc, view, ids);

                    for (int i = 0; i < sorted.Count; i++)
                    {
                        Element el = doc.GetElement(sorted[i]);
                        if (el == null)
                        {
                            continue;
                        }
                        string elType = el.GetType().ToString();
                        switch (elType)
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
                    if (boxI.Min.X > boxJ.Min.X)
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
            double minX = double.MaxValue;
            double maxX = double.MinValue;

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
                totalBoundingBox = totalBoundingBox + Math.Abs(box.Max.X - box.Min.X);
                if (box.Min.X < minX)
                {
                    minX = box.Min.X;
                    boxMin = box;
                }
                if (box.Max.X > maxX)
                {
                    maxX = box.Max.X;
                    boxMax = box;
                }
            }

            double total = Math.Abs(boxMax.Max.X - boxMin.Min.X);
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
            double boxMinX = box.Min.X;
            double boxMaxX = box.Max.X;

            if (boxMinX == boxMin.Min.X || boxMinX == boxMax.Min.X)
            {
                return;
            }
            double len = pointLen == 0 ?
                stepLen - (boxMinX - boxMin.Max.X)
                : stepLen - (boxMinX - pointLen);
            pointLen = boxMaxX + len;
            double targetX = currentPoint.X + len;
            XYZ newPoint = new XYZ(targetX, currentPoint.Y, currentPoint.Z);

            txtNode.Coord = newPoint;
        }


        private void handleTag(ref View view, ref Element el)
        {
            IndependentTag iTag = el as IndependentTag;
            XYZ currentPoint = iTag.TagHeadPosition;

            BoundingBoxXYZ box = iTag.get_BoundingBox(view);
            double boxMinX = box.Min.X;
            double boxMaxX = box.Max.X;

            if (boxMinX == boxMin.Min.X || boxMinX == boxMax.Min.X)
            {
                return;
            }
            double len = pointLen == 0 ?
                stepLen - (boxMinX - boxMin.Max.X)
                : stepLen - (boxMinX - pointLen);
            pointLen = boxMaxX + len;
            double targetX = currentPoint.X + len;
            XYZ newPoint = new XYZ(targetX, currentPoint.Y, currentPoint.Z);

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
            double boxMinX = box.Min.X;
            double boxMaxX = box.Max.X;

            if (boxMinX == boxMin.Min.X || boxMinX == boxMax.Min.X)
            {
                return;
            }

            double len = pointLen == 0 ?
                stepLen - (boxMinX - boxMin.Max.X)
                : stepLen - (boxMinX - pointLen);
            pointLen = boxMaxX + len;

            XYZ newStart = new XYZ(currentStart.X + len, currentStart.Y, currentStart.Z);
            XYZ endStart = new XYZ(currentEnd.X + len, currentEnd.Y, currentEnd.Z);

            Line newLine = Line.CreateBound(newStart, endStart);
            doc.Create.NewDetailCurve(view, newLine);
            doc.Delete(elId);
        }
    }
}
