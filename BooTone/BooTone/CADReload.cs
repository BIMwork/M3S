using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BooTone
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CADReload : IExternalCommand
    {
        private const string TRANSACTION_NAME = "RELOAD_IMPORTS";
        private const string IMPORT_INSTANCE_NAME = "Autodesk.Revit.DB.ImportInstance";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // get UIDocument
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;

                // get Document
                Document doc = uiDoc.Document;

                List<ElementId> ids = (List<ElementId>)uiDoc.Selection.GetElementIds();

                int count = ids.Count;
                if (count == 0)
                {
                    message = "1つ以上を選択してください。";
                    return Result.Failed;
                }

                // start transaction
                using (Transaction trans = new Transaction(doc, TRANSACTION_NAME))
                {
                    trans.Start();
                    for (int i = 0; i < count; i++)
                    {
                        Element el = doc.GetElement(ids[i]);
                        if (el == null)
                        {
                            continue;
                        }
                        string elType = el.GetType().ToString();
                        if (elType != IMPORT_INSTANCE_NAME)
                        {
                            message = "Autodesk.Revit.DB.ImportInstanceタイプではないので"+ elType;
                            return Result.Failed;
                        }

                        ElementId eldId = ids[i];
                        ImportInstance importInstance = doc.GetElement(eldId) as ImportInstance;

                        ElementId typeId = importInstance.GetTypeId();
                        CADLinkType cadLink = doc.GetElement(typeId) as CADLinkType;
                        cadLink.Reload();
                    }
                    trans.Commit();
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                return Result.Failed;
            }
        }
    }
}
