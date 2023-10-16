using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIMwork
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class BoundingBoxAlignment : IExternalCommand
    {
        private const string BOUNDING_BOX_ALIGNMENT = "BOUNDING_BOX_ALIGNMENT";
        private const string TEXT_NOTE_CATEGOR_NAME = "Autodesk.Revit.DB.TextNote";

        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // get UIDocument
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;

                // get Document
                Document doc = uiDoc.Document;

                // get View
                View view = doc.ActiveView;

                List<ElementId> ids = (List<ElementId>)uiDoc.Selection.GetElementIds();

                int count = ids.Count();
                if (count <= 0)
                {
                    message = "1つ以上を選択してください。";
                    return Result.Failed;
                }

                // update width
                using (Transaction trans = new Transaction(doc, BOUNDING_BOX_ALIGNMENT))
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
                        if (elType != TEXT_NOTE_CATEGOR_NAME)
                        {
                            continue;
                        }
                        // handle agliment
                        agliment(ref doc, ref view, ref el);
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

        private void agliment(ref Document doc, ref View view, ref Element el)
        {
            TextNote txtNote = el as TextNote;
            XYZ coord = txtNote.Coord;
            // type
            TextNote newTxtNote = TextNote.Create(doc, view.Id, txtNote.Coord,
                txtNote.Text, txtNote.GetTypeId());
            newTxtNote.SetFormattedText(txtNote.GetFormattedText());

            // leader
            newTxtNote.LeaderLeftAttachment = txtNote.LeaderLeftAttachment;
            newTxtNote.LeaderRightAttachment = txtNote.LeaderRightAttachment;

            IList<Leader> leaders = txtNote.GetLeaders();
            for (int i = 0; i < leaders.Count; i++)
            {
                Leader leader = leaders[i];
                XYZ anchor = leader.Anchor;
                XYZ elbow = leader.Elbow;
                XYZ end = leader.End;

                // left
                if (end.X <= coord.X)
                {
                    Leader leftLeader = newTxtNote.AddLeader(TextNoteLeaderTypes.TNLT_STRAIGHT_L);
                    leftLeader.Elbow = elbow;
                    leftLeader.End = end;
                }

                // right
                if (end.X > coord.X)
                {
                    Leader rightLeader = newTxtNote.AddLeader(TextNoteLeaderTypes.TNLT_STRAIGHT_R);
                    rightLeader.Elbow = elbow;
                    rightLeader.End = end;
                }
            }

            // align
            newTxtNote.HorizontalAlignment = txtNote.HorizontalAlignment;
            newTxtNote.VerticalAlignment = txtNote.VerticalAlignment;

            // delete 
            doc.Delete(el.Id);
        }


    }
}
