﻿using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace BooTone
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Angle120 : IExternalCommand
    {
        private const string TAG_CATEGORY_NAME = "Autodesk.Revit.DB.IndependentTag";
        private const string TEXT_NOTE_CATEGOR_NAME = "Autodesk.Revit.DB.TextNote";
        private const string TRANSACTION_NAME = "BOO_TONE_ANGLE_120";
        private const double ANGLE_TARGET = 120;


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
            IList<Leader> leaders = txtNode.GetLeaders();
            for (int i = 0; i < leaders.Count; i++)
            {
                Leader leader = leaders[i];
                XYZ anchor = leader.Anchor;
                XYZ elbow = leader.Elbow;
                XYZ end = leader.End;

                // update
                // 3 điểm thẳng hàng chéo
                XYZ angle90Elbow = new XYZ(end.X, anchor.Y, 0);
                XYZ angle90End = new XYZ(end.X, end.Y, 0);

                // anchor.Y == end.Y
                if (Math.Abs(anchor.Y - end.Y) <= 0.0001)
                {
                    double middle = (anchor.X - end.X) / 2;
                    double elbowY = coord.Y > elbow.Y ? anchor.Y - Math.Abs(middle) : anchor.Y + Math.Abs(middle);

                    angle90Elbow = new XYZ(anchor.X - middle, elbowY, 0);
                }

                // anchor.X == end.X
                if (Math.Abs(anchor.X - end.X) <= 0.0001)
                {
                    double middle = (anchor.Y - end.Y) / 2;
                    double elbowX = coord.X > anchor.X ? anchor.X - Math.Abs(middle) : anchor.X + Math.Abs(middle);

                    angle90Elbow = new XYZ(elbowX, anchor.Y - middle, 0);
                }

                calcNewElbowTextNotes(ref angle90End, ref angle90Elbow, ref coord);

                leader.Elbow = angle90Elbow;
                leader.End = angle90End;
            }
        }

        private void calcNewElbowTextNotes(ref XYZ end, ref XYZ elbow, ref XYZ coord)
        {
            /* double absDy = Math.Abs(elbow.Y - end.Y);
             double lenX = absDy / Math.Tan(Convert.ToDouble(ANGLE_TARGET) * Math.PI / 180.0);
             lenX = Math.Abs(lenX);
             double newEndX = end.X + lenX;
             if (end.X < coord.X)
             {
                 newEndX = end.X - lenX;

             }
             XYZ newEnd = new XYZ(newEndX, end.Y, end.X);
             end = newEnd;*/


            double absDy = Math.Abs(end.Y - elbow.Y);
            double lenX = absDy / Math.Tan(Convert.ToDouble(ANGLE_TARGET) * Math.PI / 180.0);
            double newElbowX = elbow.X - lenX;
            if (coord.X > elbow.X)
            {
                newElbowX = elbow.X + lenX;
            }
            XYZ newElbow = new XYZ(newElbowX, elbow.Y, elbow.X);
            elbow = newElbow;
        }

        private void handleTag(ref Element el)
        {
            IndependentTag iTag = el as IndependentTag;

            IList<Reference> leaders = iTag.GetTaggedReferences();

            for (int i = 0; i < leaders.Count; i++)
            {

                XYZ end = iTag.GetLeaderEnd(leaders[i]);
                XYZ anchor = iTag.TagHeadPosition;
                XYZ elbow = new XYZ(end.X, anchor.Y, end.Z);

                XYZ angle90Elbow = new XYZ(end.X, anchor.Y, 0);
                XYZ angle90End = new XYZ(end.X, end.Y, 0);

                // anchor.Y == end.Y
                if (Math.Abs(anchor.Y - end.Y) <= 0.0001)
                {
                    double middle = (anchor.X - end.X) / 2;
                    double elbowY = anchor.Y > elbow.Y ? anchor.Y - Math.Abs(middle) : anchor.Y + Math.Abs(middle);

                    angle90Elbow = new XYZ(anchor.X - middle, elbowY, 0);
                }

                // anchor.X == end.X
                if (Math.Abs(anchor.X - end.X) <= 0.0001)
                {
                    double middle = (anchor.Y - end.Y) / 2;
                    double elbowX = anchor.X > anchor.X ? anchor.X - Math.Abs(middle) : anchor.X + Math.Abs(middle);

                    angle90Elbow = new XYZ(elbowX, anchor.Y - middle, 0);
                }

                calcNewElbowTag(ref anchor, ref angle90End, ref angle90Elbow);

                iTag.SetLeaderElbow(leaders[i], angle90Elbow);
                iTag.SetLeaderEnd(leaders[i], angle90End);
            }
        }

        private void calcNewElbowTag(ref XYZ tagHeadPosition, ref XYZ end, ref XYZ elbow)
        {
            /* double absDy = Math.Abs(elbow.Y - end.Y);
             double lenX = absDy / Math.Tan(Convert.ToDouble(ANGLE_TARGET) * Math.PI / 180.0);
             lenX = Math.Abs(lenX);
             double newEndX = end.X + lenX;
             if (tagHeadPosition.X > elbow.X)
             {
                 newEndX = end.X - lenX;
             }
             XYZ newEnd = new XYZ(newEndX, end.Y, end.X);
             end = newEnd;*/

            double absDy = Math.Abs(elbow.Y - end.Y);
            double lenX = absDy / Math.Tan(Convert.ToDouble(ANGLE_TARGET) * Math.PI / 180.0);
            double newElbowX = elbow.X - lenX;
            if (tagHeadPosition.X > elbow.X)
            {
                newElbowX = elbow.X + lenX;
            }
            XYZ newElbow = new XYZ(newElbowX, elbow.Y, elbow.X);
            elbow = newElbow;

        }
    }
}