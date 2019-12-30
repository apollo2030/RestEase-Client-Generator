﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using RestEaseClientGenerator.Constants;
using RestEaseClientGenerator.Extensions;
using RestEaseClientGenerator.Models;
using RestEaseClientGenerator.Settings;
using RestEaseClientGenerator.Types;
using RestEaseClientGenerator.Utils;

namespace RestEaseClientGenerator.Mappers
{
    internal class InterfaceMapper : BaseMapper
    {
        public InterfaceMapper(GeneratorSettings settings) : base(settings)
        {
        }

        public RestEaseInterface Map(OpenApiPaths paths)
        {
            string name = CSharpUtils.CreateValidIdentifier(Settings.ApiName, CasingType.Pascal);
            string interfaceName = $"I{name}Api";

            var @interface = new RestEaseInterface
            {
                Name = interfaceName,
                Namespace = Settings.Namespace,
            };

            foreach (var path in paths)
            {
                MapPath(@interface, path.Key, path.Value);
            }
            //var methods = paths.Select(p => MapPath(interfaceName, p.Key, p.Value)).SelectMany(x => x).ToList();

            //var counts = methods
            //    .GroupBy(method => method.RestEaseMethod.Name + method.RestEaseMethod.Parameters)
            //    .Where(grouping => grouping.Count() > 1)
            //    .ToDictionary(grouping => grouping.Key, p => p.Count());

            //// modify the list, going backwards so we can take advantage of our counts.
            //for (int i = methods.Count - 1; i >= 0; i--)
            //{
            //    string key = methods[i].RestEaseMethod.Name + methods[i].RestEaseMethod.Parameters;
            //    if (counts.ContainsKey(key))
            //    {
            //        // add the suffix and decrement the number of duplicates left to tag.
            //        methods[i].RestEaseMethod.Name += $"{counts[key]--}";
            //    }
            //}

            return @interface;
        }

        private void MapPath(RestEaseInterface @interface, string path, OpenApiPathItem pathItem)
        {
            foreach (var restEaseInterfaceMethodDetails in pathItem.Operations.Select(o => MapOperationToMappingModel(@interface, path, o.Key.ToString(), o.Value)))
            {
                @interface.Methods.Add(restEaseInterfaceMethodDetails);
            }
        }

        private RestEaseInterfaceMethodDetails MapOperationToMappingModel(RestEaseInterface @interface, string path, string httpMethod, OpenApiOperation operation)
        {
            string methodRestEaseForAnnotation = httpMethod.ToPascalCase();
            string methodRestEaseMethod = !string.IsNullOrEmpty(operation.OperationId) ?
                operation.OperationId.ToPascalCase() :
                GenerateNameForMethod(path, methodRestEaseForAnnotation);

            var pathParameterList = operation.Parameters
                .Where(p => p.In == ParameterLocation.Path && p.Schema.GetSchemaType() != SchemaType.Object)
                .Select(p => BuildValidParameter(p.Name, p.Schema, p.Required, p.Description, "Path"))
                .ToList();

            var queryParameterList = operation.Parameters
                .Where(p => p.In == ParameterLocation.Query && p.Schema.GetSchemaType() != SchemaType.Object)
                .Select(p => BuildValidParameter(p.Name, p.Schema, p.Required, p.Description, "Query"))
                .OrderByDescending(p => p.Required)
                .ToList();

            var extensionMethodParameterList = new List<RestEaseParameter>();
            var bodyParameterList = new List<RestEaseParameter>();
            if (operation.RequestBody != null)
            {
                if (TryGetOpenApiMediaType(operation.RequestBody.Content, SupportedContentTypes.ApplicationJson, out OpenApiMediaType requestMediaTypeJson))
                {
                    string bodyParameter;
                    switch (requestMediaTypeJson.Schema?.GetSchemaType())
                    {
                        case SchemaType.Array:
                            string arrayType = requestMediaTypeJson.Schema.Items.Reference != null ? requestMediaTypeJson.Schema.Items.Reference.Id : MapSchema(requestMediaTypeJson.Schema.Items, "", requestMediaTypeJson.Schema.Nullable).ToString();
                            bodyParameter = MapArrayType(arrayType);
                            break;

                        case SchemaType.Object:
                            bodyParameter = requestMediaTypeJson.Schema.Reference.Id;
                            break;

                        default:
                            bodyParameter = "";
                            break;
                    }

                    if (!string.IsNullOrEmpty(bodyParameter))
                    {
                        string bodyParameterIdentifierName = bodyParameter.ToCamelCase();
                        bodyParameterList.Add(new RestEaseParameter
                        {
                            Identifier = bodyParameterIdentifierName,
                            IdentifierWithType = $"{bodyParameter} {bodyParameterIdentifierName}",
                            IdentifierWithRestEase = $"[Body] {bodyParameter} {bodyParameterIdentifierName}",
                            Summary = requestMediaTypeJson.Schema?.Description
                        });
                    }
                }
                else if (TryGetOpenApiMediaType(operation.RequestBody.Content, SupportedContentTypes.MultipartFormData, out OpenApiMediaType requestMultipartFormData))
                {
                    string httpContentDescription;
                    if (!Settings.GenerateMultipartFormDataExtensionMethods)
                    {
                        httpContentDescription = "Manually add an extension method to support the exact parameters. See https://github.com/canton7/RestEase#wrapping-other-methods for more info.";
                    }
                    else
                    {
                        httpContentDescription = "An extension method is generated to support the exact parameters.";
                        extensionMethodParameterList.AddRange(requestMultipartFormData.Schema.Properties
                            .Select(p => BuildValidParameter(p.Key, p.Value, p.Value.Nullable, p.Value.Description, "")));
                    }

                    bodyParameterList.Add(new RestEaseParameter
                    {
                        Identifier = "content",
                        IdentifierWithType = "HttpContent content",
                        IdentifierWithRestEase = "HttpContent content",
                        Summary = httpContentDescription,
                        IsSpecial = true
                    });
                }
                else if (TryGetOpenApiMediaType(operation.RequestBody.Content, SupportedContentTypes.ApplicationFormUrlEncoded, out OpenApiMediaType requestFormUrlencoded))
                {
                    bodyParameterList.Add(new RestEaseParameter
                    {
                        Identifier = "formData",
                        IdentifierWithType = "IDictionary<string, object> formData",
                        IdentifierWithRestEase = "[Body(BodySerializationMethod.UrlEncoded)] IDictionary<string, object> formData",
                        Summary = requestFormUrlencoded.Schema?.Description
                    });
                }
            }

            var methodParameterList = pathParameterList
                .Union(bodyParameterList)
                .Union(queryParameterList)
                .ToList();

            var response = operation.Responses.First();

            object returnType = null;
            if (response.Value != null && TryGetOpenApiMediaType(response.Value.Content, SupportedContentTypes.ApplicationJson, out OpenApiMediaType responseMediaType))
            {
                switch (responseMediaType.Schema?.GetSchemaType())
                {
                    case SchemaType.Array:
                        string arrayType = responseMediaType.Schema.Items.Reference != null ?
                            responseMediaType.Schema.Items.Reference.Id :
                            MapSchema(responseMediaType.Schema.Items, "", responseMediaType.Schema.Nullable).ToString();

                        returnType = MapArrayType(arrayType);
                        break;

                    case SchemaType.Object:
                        if (responseMediaType.Schema.Reference != null)
                        {
                            // Existing defined object
                            returnType = responseMediaType.Schema.Reference.Id;
                        }
                        else if (responseMediaType.Schema.AdditionalProperties != null)
                        {
                            // Use AdditionalProperties
                            returnType = MapSchema(responseMediaType.Schema.AdditionalProperties, null, responseMediaType.Schema.AdditionalProperties.Nullable, false);
                        }
                        else
                        {
                            // Object is defined `inline`, create a new Model and use that one.
                            string className = !string.IsNullOrEmpty(responseMediaType.Schema.Title)
                                ? CSharpUtils.CreateValidIdentifier(responseMediaType.Schema.Title, CasingType.Pascal)
                                : $"{methodRestEaseMethod.ToPascalCase()}Result";

                            var existingModel = @interface.InlineModels.FirstOrDefault(m => m.ClassName == className);
                            if (existingModel == null)
                            {
                                var newModel = new RestEaseModel
                                {
                                    Namespace = Settings.Namespace,
                                    ClassName = className,
                                    Properties = MapSchema(responseMediaType.Schema, null, false) as ICollection<string>
                                };
                                @interface.InlineModels.Add(newModel);
                            }

                            returnType = className;
                        }
                        break;
                }
            }

            var method = new RestEaseInterfaceMethodDetails
            {
                Summary = operation.Summary ?? $"{methodRestEaseMethod} ({path})",
                SummaryParameters = methodParameterList.Select(mp => $"<param name=\"{mp.Identifier}\">{mp.Summary}</param>").ToList(),
                RestEaseAttribute = $"[{methodRestEaseForAnnotation}(\"{path}\")]",
                RestEaseMethod = new RestEaseInterfaceMethod
                {
                    ReturnType = MapReturnType(returnType),
                    Name = methodRestEaseMethod,
                    ParametersAsString = string.Join(", ", methodParameterList.Select(mp => mp.IdentifierWithRestEase)),
                    Parameters = methodParameterList
                }
            };

            if (extensionMethodParameterList.Any())
            {
                var combinedMethodParameterList = new List<RestEaseParameter>
                {
                    new RestEaseParameter
                    {
                        Identifier = "api",
                        IdentifierWithType = $"this {@interface.Name} api",
                        IdentifierWithRestEase = $"this {@interface.Name} api",
                        Summary = "The Api"
                    }
                };
                combinedMethodParameterList.AddRange(methodParameterList.Where(p => !p.IsSpecial));
                combinedMethodParameterList.AddRange(extensionMethodParameterList);

                method.MultipartFormDataMethodDetails = new RestEaseInterfaceMethodDetails
                {
                    Summary = operation.Summary ?? $"{methodRestEaseMethod} ({path})",
                    SummaryParameters = combinedMethodParameterList.Select(mp => $"<param name=\"{mp.Identifier}\">{mp.Summary}</param>").ToList(),
                    RestEaseMethod = new RestEaseInterfaceMethod
                    {
                        ReturnType = method.RestEaseMethod.ReturnType,
                        Name = method.RestEaseMethod.Name,
                        ParametersAsString = string.Join(", ", combinedMethodParameterList.Select(mp => mp.IdentifierWithType)),
                        Parameters = combinedMethodParameterList
                    }
                };
                method.MultipartFormDataMethodParameters = extensionMethodParameterList;
            }

            return method;
        }

        private string MapReturnType(object returnType)
        {
            string returnTypeAsString = returnType as string;
            if (returnTypeAsString == null)
            {
                return "Task";
            }

            if (returnTypeAsString == "Models")
            {
                returnTypeAsString = "Models.Models";
            }

            switch (Settings.MethodReturnType)
            {
                case MethodReturnType.String:
                    return "Task<string>";

                case MethodReturnType.HttpResponseMessage:
                    return "Task<HttpResponseMessage>";

                case MethodReturnType.Response:
                    return $"Task<Response<{returnTypeAsString}>>";

                case MethodReturnType.Stream:
                    return "Task<Stream>";

                default:
                    return $"Task<{returnTypeAsString}>";
            }
        }

        private string GenerateNameForMethod(string path, string httpMethodPascalCased)
        {
            var list = new List<string> { httpMethodPascalCased };

            bool byFound = false;
            foreach (string part in path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (part.StartsWith("{"))
                {
                    var text = part.Split(new[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries)[0].ToPascalCase();

                    list.Add(byFound ? $"By{text}" : $"And{text}");

                    byFound = true;
                }
                else
                {
                    list.Add(part.ToPascalCase());
                }
            }

            return string.Join("", list);
        }

        private RestEaseParameter BuildValidParameter(string identifier, OpenApiSchema schema, bool required, string description, string parameterType, params string[] extraAttributes)
        {
            var attributes = new List<string>();
            string validIdentifier = CSharpUtils.CreateValidIdentifier(identifier);

            object paramWithType;
            if (identifier != validIdentifier)
            {
                attributes.Add($"Name = \"{identifier}\"");
                attributes.AddRange(extraAttributes);

                paramWithType = MapSchema(schema, validIdentifier, !required, false);

                return new RestEaseParameter
                {
                    Required = required,
                    Identifier = validIdentifier,
                    SchemaType = schema.GetSchemaType(),
                    SchemaFormat = schema.GetSchemaFormat(),
                    IdentifierWithType = $"{paramWithType}",
                    IdentifierWithRestEase = $"[{parameterType}({string.Join(", ", attributes)})] {paramWithType}",
                    Summary = description
                };
            }

            string extraAttributesBetweenParentheses = extraAttributes.Length == 0 ? string.Empty : $"({string.Join(", ", extraAttributes)})";
            paramWithType = MapSchema(schema, identifier, !required, false);

            return new RestEaseParameter
            {
                Required = required,
                Identifier = identifier,
                SchemaType = schema.GetSchemaType(),
                SchemaFormat = schema.GetSchemaFormat(),
                IdentifierWithType = $"{paramWithType}",
                IdentifierWithRestEase = $"[{parameterType}{extraAttributesBetweenParentheses}] {paramWithType}",
                Summary = description
            };
        }

        private bool TryGetOpenApiMediaType(IDictionary<string, OpenApiMediaType> contentTypes, string contentType, out OpenApiMediaType mediaType)
        {
            var contentTypesIgnoreCase = new Dictionary<string, OpenApiMediaType>(contentTypes, StringComparer.InvariantCultureIgnoreCase);

            if (contentTypesIgnoreCase.TryGetValue(contentType, out mediaType))
            {
                return true;
            }

            var key = contentTypesIgnoreCase.Keys.FirstOrDefault(ct => ct.Contains(contentType));
            if (key != null)
            {
                mediaType = contentTypesIgnoreCase[key];
                return true;
            }

            //if (contentTypesIgnoreCase.Count > 0)
            //{
            //    mediaType = contentTypesIgnoreCase.First().Value;
            //    return true;
            //}

            return false;
        }
    }
}