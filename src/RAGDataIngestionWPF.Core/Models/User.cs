// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



namespace RAGDataIngestionWPF.Core.Models;





// This class contains user members to download user information from Microsoft Graph
// https://docs.microsoft.com/graph/api/resources/user?view=graph-rest-1.0
public class User
    {

    public List<string> BusinessPhones { get; set; }

    public string DisplayName { get; set; }

    public string GivenName { get; set; }
    public string Id { get; set; }

    public object JobTitle { get; set; }

    public string Mail { get; set; }

    public string MobilePhone { get; set; }

    public object OfficeLocation { get; set; }

    public string Photo { get; set; }

    public string PreferredLanguage { get; set; }

    public string Surname { get; set; }

    public string UserPrincipalName { get; set; }
    }