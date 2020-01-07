using Newtonsoft.Json;
using RestEaseClientGenerator.Json;
using RestEaseClientGenerator.Types;

namespace RestEaseClientGenerator.VSIX.Options.RestEase
{
    public class RestEaseUserOptions
    {
        [JsonConverter(typeof(DescriptionEnumConverter))]
        public ArrayType? ArrayType { get; set; }

        [JsonConverter(typeof(DescriptionEnumConverter))]
        public MultipartFormDataFileType? MultipartFormDataFileType { get; set; }

        [JsonConverter(typeof(DescriptionEnumConverter))]
        public ApplicationOctetStreamType? ApplicationOctetStreamType { get; set; }

        public bool? FailOnOpenApiErrors { get; set; }

        public bool? AddAuthorizationHeader { get; set; }

        public bool? UseDateTimeOffset { get; set; }

        [JsonConverter(typeof(DescriptionEnumConverter))]
        public MethodReturnType? MethodReturnType { get; set; }

        public bool? AppendAsync { get; set; }

        public bool? GenerateMultipartFormDataExtensionMethods { get; set; }

        public bool? GenerateFormUrlEncodedExtensionMethods { get; set; }

        public bool? GenerateApplicationOctetStreamExtensionMethods { get; set; }

        public string ApiNamespace { get; set; }

        public string ModelsNamespace { get; set; }

        public bool? ReturnObjectFromMethodWhenResponseIsDefinedButNoModelIsSpecified { get; set; }

        [JsonConverter(typeof(DescriptionEnumConverter))]
        public ContentType? PreferredContentType { get; set; }

        public bool? ForceContentTypeToApplicationJson { get; set; }

        public bool? UseOperationIdAsMethodName { get; set; }
        
        // public bool UseUserOptions { get; set; }
    }
}