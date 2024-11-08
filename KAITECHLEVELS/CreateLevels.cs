using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace KAITECHLEVELS
{
    [Transaction(TransactionMode.Manual)]
    public class CreateLevels : IExternalCommand
    {
        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //uidocument
			UIDocument uidoc=commandData.Application.ActiveUIDocument;
			//document
			Document doc=uidoc.Document;
			#region parametars

			
			//start level
			double startlevel =0;
				//space
				double space=3;
				//no of levels
				int noOflevels=100;
				//conversation factor
				double tofeet=3.28084;
			#endregion
			#region Check Dublicate levels
			// store dublicate levels
			StringBuilder sb =new StringBuilder();
			int count=0;
			//check duplication first
			for (int i = 0; i < noOflevels; i++) 
			{
				string levelName = "KAITECH-LEVEL" + " " + (i + 1);
				Level existing=GetExistingLevels(doc, levelName);

                if (existing != null) 
				{
					sb.AppendLine(levelName);
					count++;
				}
			}
			if (count>0) 
			{
			string massage="Duplicate Levels Found!!";
			massage+="\n\tNumbers:\t"+count;
			massage+="\n"+sb;
			TaskDialog.Show("Failed",massage);
			return Result.Cancelled;
			}
            #endregion
            #region create Levels and Structural floor plan
            else
            {
                using (Transaction tns = new Transaction(doc, "Create level"))
                {

                    tns.Start();

                    for (int i = 0; i < noOflevels; i++)
                    {
                        double levelElevation = (startlevel + (space * i)) * tofeet;
                        string levelName = "KAITECH-LEVEL" + " " + (i + 1);

                        Level newLevel = Level.Create(doc, levelElevation);
                        newLevel.Name = levelName;
                        var viewfamilytypeidSt = new FilteredElementCollector(doc)
                            .OfClass(typeof(ViewFamilyType))

                            .Cast<ViewFamilyType>()
                            .FirstOrDefault(v => v.ViewFamily == ViewFamily.StructuralPlan).Id;

                        ViewPlan newplan = ViewPlan.Create(doc, viewfamilytypeidSt, newLevel.Id);

                    }
                    tns.Commit();

                    TaskDialog.Show("Level Creation", "All levels created and Structural floor plan");

                }
                return Result.Succeeded;
            }
            #endregion

        }

        #region Method to check Existing levels
        private Level GetExistingLevels(Document doc,string levelname)
		{
			var leveles=new FilteredElementCollector(doc)
				.OfClass(typeof(Level)).Cast<Level>();
			foreach (var level in leveles) 
			{
				if(level.Name==levelname)
					return level;
			}
			return null;
		}
        #endregion

    }
}
