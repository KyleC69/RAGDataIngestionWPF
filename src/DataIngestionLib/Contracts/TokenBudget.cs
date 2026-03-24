// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//



namespace DataIngestionLib.Contracts;





public class TokenBudget
{
    public int BudgetTotal { get; set; }
    public int MaximumContext { get; set; }
    public int MetaBudget { get; set; }
    public int RAGBudget { get; set; }
    public int SessionBudget { get; set; }
    public int SystemBudget { get; set; }
    public int ToolBudget { get; set; }
}