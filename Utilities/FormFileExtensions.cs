using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

public static class FormFileExtensions
{
    public static List<IFormFile> GetFiles(this IFormFileCollection formFiles, string fieldName)
    {
        return formFiles?
            .Where(f => f.Name.Equals(fieldName))
            .ToList() ?? new List<IFormFile>();
    }
}