using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BooTone
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class DeleteDefaultType : IExternalCommand
    {
        private const string TRANSACTION_NAME = "DELETE_DEFAULT_TYPE";
        private const string PROFILES_CATEGORY = "Profiles";
        private const string DETAIL_ITEMS_CATEGORY = "Detail Items";
        private const string ANNOTATION_SYMBOLS_CATEGORY = "Annotation";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // get UIDocument
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;

                // get Document
                Document doc = uiDoc.Document;

                StringBuilder sb = new StringBuilder();

                // start transaction
                using (Transaction trans = new Transaction(doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    IList<ElementId> elIds = new List<ElementId>();

                    // family
                    findFamily(ref doc, ref elIds, ref sb);

                    // delete family type
                    foreach (ElementId elementId in elIds)
                    {
                        try
                        {
                            doc.Delete(elementId);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    // TaskDialog.Show("reavit", sb.ToString());
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

        // find family
        private void findFamily(ref Document doc, ref IList<ElementId> elIds, ref StringBuilder sb)
        {
            FilteredElementIterator familyList = new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .GetElementIterator();
          
            while (familyList.MoveNext())
            {
                Family family = familyList.Current as Family;
                if (family != null)
                {
                    string familyName = family.Name;
                    
                    foreach (ElementId elementId in family.GetFamilySymbolIds())
                    {
                        object symbol = doc.GetElement(elementId);
                        FamilySymbol familyType = symbol as FamilySymbol;
                        if (familyType == null || familyType.Category == null)
                        {
                            continue;
                        }

                        // add list delete family type
                        // string categoryName = familyType.Category.Name;
                        string categoryFamilyName = family.FamilyCategory.Name;
                        string categoryTypeFamily = family.FamilyCategory.CategoryType.ToString();
                        string familyTypeName = familyType.Name;
                        bool isValidCategory = categoryFamilyName != PROFILES_CATEGORY 
                            && categoryFamilyName != DETAIL_ITEMS_CATEGORY 
                            && categoryTypeFamily != ANNOTATION_SYMBOLS_CATEGORY;
                        string log = categoryFamilyName + "---:---" + categoryTypeFamily + "---:---" + familyName + "---:---" + familyTypeName;
                        if (isValidCategory && familyName == familyTypeName)
                        {
                            // sb.AppendLine(log);
                            elIds.Add(familyType.Id);
                        }
                    }
                }
            }
        }
    }
}
