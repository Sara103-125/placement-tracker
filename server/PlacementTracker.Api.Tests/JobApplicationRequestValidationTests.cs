using System.ComponentModel.DataAnnotations;
using PlacementTracker.Api.Dtos;

namespace PlacementTracker.Api.Tests;

/// <summary>
/// Checks the validation attributes on the request DTO ([Required], [Url], [MaxLength]).
/// In the running API, [ApiController] turns these failures into a 400 Bad Request.
/// </summary>
public class JobApplicationRequestValidationTests
{
    private static List<string> Validate(JobApplicationRequest request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
        return results.SelectMany(r => r.MemberNames).ToList();
    }

    [Fact]
    public void ValidRequest_HasNoErrors()
    {
        var request = new JobApplicationRequest
        {
            Company = "Google",
            Role = "SWE Intern",
            JobLink = "https://careers.google.com",
        };

        Assert.Empty(Validate(request));
    }

    [Fact]
    public void MissingCompanyAndBadLink_AreRejected()
    {
        var request = new JobApplicationRequest
        {
            Company = "",
            Role = "SWE Intern",
            JobLink = "not-a-link",
        };

        var invalidFields = Validate(request);

        Assert.Contains("Company", invalidFields);
        Assert.Contains("JobLink", invalidFields);
    }
}
