using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMwork;

namespace BIMwork
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class UnjoinGeometry : IExternalCommand
    {
        private const string TRANSACTION_NAME = "UNJOIN_GEOMETRY";

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
                if (count == 0)
                {
                    message = "部材を選択してから使⽤してください";
                    return Result.Failed;
                }

                // start transaction
                using (Transaction trans = new Transaction(doc, TRANSACTION_NAME))
                {
                    trans.Start();
                    for (int i = 0; i < count; i++)
                    {
                        // get element
                        Element el = doc.GetElement(ids[i]);
                        if (el == null)
                        {
                            continue;
                        }
                        // string elType = el.GetType().ToString();
                        List<ElementId> joinIds = (List<ElementId>)JoinGeometryUtils
                            .GetJoinedElements(doc, el);
                        int joinCount = joinIds.Count;
                        if (joinCount == 0)
                        {
                            continue;
                        }
                        for (int j = 0; j < joinCount; j++)
                        {
                            ElementId joinId = joinIds[j];
                            Element elJoin = doc.GetElement(joinId);
                            JoinGeometryUtils.UnjoinGeometry(doc, el, elJoin);
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
    }
}
