using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

namespace BIMwork
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class DisallowEndpoint : IExternalCommand
    {
        private const string WALL_CATEGORY_TYPE = "Autodesk.Revit.DB.Wall";
        private const string BEAM_CATEGOR_TYPE = "Autodesk.Revit.DB.FamilyInstance";

        private const string DISALLOW_ENDPOINT = "DISALLOW_ENDPOINT";

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

                // IList<Reference> refEls = uiDoc.Selection.PickObjects(ObjectType.Element);
                // List<ElementId> ids = (from Reference r in refEls select r.ElementId).ToList();

                List<ElementId> ids = (List<ElementId>)uiDoc.Selection.GetElementIds();

                int count = ids.Count();
                if (count <= 0)
                {
                    message = "1つ以上を選択してください。";
                    return Result.Failed;
                }
                using (Transaction trans = new Transaction(doc, DISALLOW_ENDPOINT))
                {
                    trans.Start();
                    for (int i = 0; i < count; i++)
                    {
                        // get element
                        // Element el = doc.GetElement(refEls[i].ElementId);
                        Element el = doc.GetElement(ids[i]);
                        if (el == null)
                        {
                            continue;
                        }

                        string elType = el.GetType().ToString();
                        switch (elType)
                        {
                            // wall
                            case WALL_CATEGORY_TYPE:
                                handleWall(ref el);
                                break;
                            // beam
                            case BEAM_CATEGOR_TYPE:
                                handleBeam(ref el);
                                break;
                            default: break;
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

        private void handleWall(ref Element el)
        {
            Wall wall = el as Wall;
            WallUtils.DisallowWallJoinAtEnd(wall, 0);
            WallUtils.DisallowWallJoinAtEnd(wall, 1);
        }

        private void handleBeam(ref Element el)
        {
            FamilyInstance beam = el as FamilyInstance;
            StructuralFramingUtils.DisallowJoinAtEnd(beam, 0);
            StructuralFramingUtils.DisallowJoinAtEnd(beam, 1);
        }
    }
}
