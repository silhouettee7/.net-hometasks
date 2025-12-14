namespace RESTAuth.Domain.Models;

public class FileReport
{
    public Stream Content { get; set; }
    public string ContentType { get; set; }
    public string FileName { get; set; }
}