using System.Text;
using RESTAuth.Domain.Abstractions.Services;
using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Application.Services;

public class FileReportService: IFileReportService
{
    public FileReport CreateFileReport(Guid reportId, List<UserReport> users)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"report_{reportId}_{timestamp}.csv";
        string contentType = "text/csv";

        var sb = new StringBuilder();

        sb.AppendLine("Name;Email;Salary;Department;CreatedDate;UpdatedDate");
        foreach (var user in users)
        {
            sb.AppendLine($"{user.Name};{user.Email};{user.Salary};{user.Department};{user.CreatedDate};{user.UpdatedDate}");
        }
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));

        return new FileReport
        {
            FileName = fileName,
            Content = stream,
            ContentType = contentType
        };
    }
}