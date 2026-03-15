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





// Remove this class once your pages/features are using your data.
// This is used by the SampleDataService.
// It is the model class we use to display data on pages like Grid, Chart, and List Details.
public sealed class SampleCompany
    {

    public string Address { get; set; }

    public string City { get; set; }
    public string CompanyId { get; set; }

    public string CompanyName { get; set; }

    public string ContactName { get; set; }

    public string ContactTitle { get; set; }

    public string Country { get; set; }

    public string Fax { get; set; }

    public ICollection<SampleOrder> Orders { get; set; }

    public string Phone { get; set; }

    public string PostalCode { get; set; }
    }