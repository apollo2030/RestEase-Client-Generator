﻿using Microsoft.OpenApi.Readers;
using System.Collections.Generic;
using System.IO;
using RestEaseClientGenerator.Models;
using RestEaseClientGenerator.Settings;

namespace RestEaseClientGenerator
{
    public interface IGenerator
    {
        ICollection<GeneratedFile> FromStream(Stream stream, GeneratorSettings settings, out OpenApiDiagnostic diagnostic);

        ICollection<GeneratedFile> FromStream(Stream stream, string clientNamespace, string apiName, bool singleFile, out OpenApiDiagnostic diagnostic);
    }
}