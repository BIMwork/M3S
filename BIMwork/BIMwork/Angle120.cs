using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMwork;

namespace BIMwork
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Angle120 : IExternalCommand
    {
        private const string TAG_CATEGORY_NAME = "Autodesk.Revit.DB.IndependentTag";
        private const string TEXT_NOTE_CATEGOR_NAME = "Autodesk.Revit.DB.TextNote";
        private const string TRANSACTION_NAME = "BOO_TONE_ANGLE_120";


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // get UIDocument
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;

                // get Document
                Document doc = uiDoc.Document;

                // get ref of emplement
                // IList<Reference> refEls = uiDoc.Selection.PickObjects(ObjectType.Element);

                List<ElementId> ids = (List<ElementId>)uiDoc.Selection.GetElementIds();

                // int count = refEls.Count;
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
                            // tag by category
                            case TAG_CATEGORY_NAME:
                                handleTag(ref el);
                                break;
                            // text notes
                            case TEXT_NOTE_CATEGOR_NAME:
                                handleTextNotes(ref el);
                                break;
                            default: break;
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


        private void handleTextNotes(ref Element el)
        {
            TextNote txtNode = el as TextNote;
            XYZ coord = txtNode.Coord;

            string oxyz = AngleUtils.getCoordinateAxisTextNote(ref coord);

            IList<Leader> leaders = txtNode.GetLeaders();
            for (int i = 0; i < leaders.Count; i++)
            {
                Leader leader = leaders[i];
                XYZ anchor = leader.Anchor;
                XYZ elbow = leader.Elbow;
                XYZ end = leader.End;

                TargetPoint targetPoint = null;
                switch (oxyz)
                {
                    case "oxy":
                        targetPoint = Angle120Utils.handleTextNotes120OXY(ref coord, ref anchor, ref elbow, ref end);
                        break;
                    case "oxz":
                        targetPoint = Angle120Utils.handleTextNotes120OXZ(ref coord, ref anchor, ref elbow, ref end);
                        break;
                    case "oyz":
                        targetPoint = Angle120Utils.handleTextNotes120OYZ(ref coord, ref anchor, ref elbow, ref end);
                        break;
                    default:
                        targetPoint = null;
                        break;
                }
                if (targetPoint == null)
                {
                    continue;
                }

                // update leader line
                leader.Elbow = targetPoint.elbow;
                leader.End = targetPoint.end;
            }
        }

        private void handleTag(ref Element el)
        {
            IndependentTag iTag = el as IndependentTag;

            XYZ leaderElbow = iTag.LeaderElbow;
            XYZ leaderEnd = iTag.LeaderEnd;
            string oxyz = AngleUtils.getCoordinateAxisTag(ref leaderElbow, ref leaderEnd);

            IList<Reference> leaders = iTag.GetTaggedReferences();

            for (int i = 0; i < leaders.Count; i++)
            {

                XYZ end = iTag.GetLeaderEnd(leaders[i]);
                XYZ anchor = iTag.TagHeadPosition;
                XYZ elbow = iTag.GetLeaderElbow(leaders[i]);

                TargetPoint targetPoint = null;
                switch (oxyz)
                {
                    case "oxy":
                        targetPoint = Angle120Utils.handleTag120OXY(ref anchor, ref elbow, ref end);
                        break;
                    case "oxz":
                        targetPoint = Angle120Utils.handleTag120OXZ(ref anchor, ref elbow, ref end);
                        break;
                    case "oyz":
                        targetPoint = Angle120Utils.handleTag120OYZ(ref anchor, ref elbow, ref end);
                        break;
                    default:
                        targetPoint = null;
                        break;
                }
                if (targetPoint == null)
                {
                    continue;
                }

                // update leader line
                iTag.SetLeaderElbow(leaders[i], targetPoint.elbow);
                iTag.SetLeaderEnd(leaders[i], targetPoint.end);
            }
        }
    }
}