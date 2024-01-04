using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMwork;

namespace BIMwork
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class JoinGeometry : IExternalCommand
    {
        private const string FLOOR_CATEGORY_NAME = "Autodesk.Revit.DB.Floor";
        private const string TRANSACTION_NAME = "JOIN_GEOMETRY";

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

                // get ref of emplement
                // IList<Reference> refEls = uiDoc.Selection.PickObjects(ObjectType.Element);

                List<ElementId> ids = (List<ElementId>)uiDoc.Selection.GetElementIds();

                // int count = refEls.Count;
                int count = ids.Count;
                if (count < 2)
                {
                    message = "２個以上の床を選択してください";
                    return Result.Failed;
                }


                // start transaction
                using (Transaction trans = new Transaction(doc, TRANSACTION_NAME))
                {
                    trans.Start();
                    bool isError = true;
                    for (int i = 0; i < count; i++)
                    {
                        // get element
                        Element elStart = doc.GetElement(ids[i]);
                        if (elStart == null)
                        {
                            continue;
                        }
                        string elStartType = elStart.GetType().ToString();
                        if (elStartType != FLOOR_CATEGORY_NAME)
                        {
                            continue;
                        }

                        // get parameter of elStart
                        Parameter typeParamStart = elStart
                            .get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM);
                        string typeStart = typeParamStart.AsValueString();

                        Parameter levelParamStart = elStart
                            .get_Parameter(BuiltInParameter.LEVEL_PARAM);
                        string levelStart = levelParamStart.AsValueString();

                        Parameter heightOffsetParamStart = elStart
                            .get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);
                        string heightOffsetStart = heightOffsetParamStart.AsValueString();

                        for (int j = i + 1; j < count; j++)
                        {
                            Element elEnd = doc.GetElement(ids[j]);
                            string elEndType = elEnd.GetType().ToString();
                            if (elEndType != FLOOR_CATEGORY_NAME)
                            {
                                continue;
                            }

                            // get parameter of elEnd
                            Parameter typeParamEnd = elEnd
                                .get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM);
                            string typeEnd = typeParamEnd.AsValueString();

                            Parameter levelParamEnd = elEnd
                                .get_Parameter(BuiltInParameter.LEVEL_PARAM);
                            string levelEnd = levelParamEnd.AsValueString();

                            Parameter heightOffsetParamEnd = elEnd
                                .get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);
                            string heightOffsetEnd = heightOffsetParamEnd.AsValueString();

                            // check join
                            bool isJoin = typeStart == typeEnd
                                && levelStart == levelEnd
                                && heightOffsetStart == heightOffsetEnd;
                            bool isValidIntersection = isIntersection(ref doc, ref view, ref elStart, ref elEnd);
                            if (isJoin && isValidIntersection)
                            {
                                if (JoinGeometryUtils.AreElementsJoined(doc, elStart, elEnd))
                                {
                                    continue;
                                }
                                JoinGeometryUtils.JoinGeometry(doc, elStart, elEnd);
                                isError = false;
                            }
                        }

                    }

                    if (isError)
                    {
                        message = "２個以上の床を選択してください";
                        return Result.Failed;
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

        private bool isIntersection(ref Document doc, ref View view, ref Element elStart, ref Element elEnd)
        {
            BoundingBoxXYZ bb = elStart.get_BoundingBox(view);
            Outline outline = new Outline(bb.Min, bb.Max);
            BoundingBoxIntersectsFilter bbfilter
                = new BoundingBoxIntersectsFilter(outline);

            FilteredElementCollector collector
                = new FilteredElementCollector(
                doc, view.Id);
            ICollection<ElementId> idsExclude
                = new List<ElementId>();
            idsExclude.Add(elStart.Id);

            collector.Excluding(idsExclude)
              .WherePasses(bbfilter);

            // Generate a report to display in the dialog.
            foreach (Element e in collector)
            {
                if (e.Id == elEnd.Id)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
