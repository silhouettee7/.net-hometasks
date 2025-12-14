using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Domain.Abstractions.Services;

public interface IFileReportService
{
    FileReport CreateFileReport(Guid reportId, List<UserReport> users);
}