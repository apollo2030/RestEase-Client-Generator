using System.ComponentModel;
using AutoMapper;
using Microsoft.VisualStudio.Shell;
using RestEaseClientGenerator.Types;

namespace RestEaseClientGenerator.VSIX.Options.RestEase
{
    public class RestEaseOptionsPage : DialogPage, IRestEaseOptions
    {
        private const string General = "General";
        private const string Interface = "Interface";
        private const string Models = "Models";

        #region General
        [Category(General)]
        [DisplayName("Array Type")]
        [Description("The Array type to use. The default is Array 'T[]'.")]
        public ArrayType ArrayType { get; set; }

        [Category(General)]
        [DisplayName("Fail on OpenApi Errors")]
        [Description("Don't generate the file if errors are detected when parsing the specification file. The default value is 'False'.")]
        public bool FailOnOpenApiErrors { get; set; }

        [Category(General)]
        [DisplayName("Use DateTimeOffset")]
        [Description("Use DateTimeOffset instead of DateTime. The default value is 'False'.")]
        public bool UseDateTimeOffset { get; set; }

        [Category(General)]
        [DisplayName("Namespace for the Api")]
        [Description("Append this namespace for the Api. The default value is 'Api'.")]
        public string ApiNamespace { get; set; } = "Api";

        [Category(General)]
        [DisplayName("Namespace for the Models")]
        [Description("Append this namespace for the Models. The default value is 'Models'.")]
        public string ModelsNamespace { get; set; } = "Models";

        [Category(General)]
        [DisplayName("Use .RestEaseOptions file")]
        [Description("Read and write .RestEaseOptions file which can be used to overwrite some Visual Studio global RestEase options. The default value is 'True'.")]
        public bool UseUserOptions { get; set; } = true;
        #endregion

        #region Interface
        [Category(Interface)]
        [DisplayName("Append Async")]
        [Description("Append Async postfix to all methods. The default value is 'True'.")]
        public bool AppendAsync { get; set; } = true;

        [Category(Interface)]
        [DisplayName("Method ReturnType")]
        [Description("The ReturnType to use for the methods. The default value is 'Type'. For more details see https://github.com/canton7/RestEase#return-types.")]
        public MethodReturnType MethodReturnType { get; set; }

        [Category(Interface)]
        [DisplayName("Type for multipart/form-data")]
        [Description("The MultipartFormData FileType to use. The default value is 'byte[]'.")]
        public MultipartFormDataFileType MultipartFormDataFileType { get; set; }

        [Category(Interface)]
        [DisplayName("Type for application/octet-stream")]
        [Description("The ApplicationOctetStream Type to use. The default value is 'byte[]'.")]
        public ApplicationOctetStreamType ApplicationOctetStreamType { get; set; }

        [Category(Interface)]
        [DisplayName("Generate MultipartFormData Extension methods")]
        [Description("Generate Extension methods for MultipartFormData methods. The default value is 'True'.")]
        public bool GenerateMultipartFormDataExtensionMethods { get; set; } = true;

        [Category(Interface)]
        [DisplayName("Generate FormUrlEncoded Extension methods")]
        [Description("Generate Extension methods for FormUrlEncoded methods. The default value is 'True'.")]
        public bool GenerateFormUrlEncodedExtensionMethods { get; set; } = true;

        [Category(Interface)]
        [DisplayName("Generate ApplicationOctetStream Extension methods")]
        [Description("Generate Extension methods for ApplicationOctetStream methods. The default value is 'True'.")]
        public bool GenerateApplicationOctetStreamExtensionMethods { get; set; } = true;

        [Category(Interface)]
        [DisplayName("Return Object from Method")]
        [Description("Return Object from Method when Response is defined but no Model is specified. The default value is 'False'.")]
        public bool ReturnObjectFromMethodWhenResponseIsDefinedButNoModelIsSpecified { get; set; }

        [Category(Interface)]
        [DisplayName("Preferred Content-Type")]
        [Description("Preferred Content-Type to use when both 'application/json' and 'application/xml' are defined. The default value is 'application/json'.")]
        public ContentType PreferredContentType { get; set; }

        [Category(Interface)]
        [DisplayName("Force Content-Type to 'application/json'")]
        [Description("Always use Content-Type 'application/json', also when multiple Content-Types are are defined. The default value is 'False'.")]
        public bool ForceContentTypeToApplicationJson { get; set; }

        [Category(Interface)]
        [DisplayName("Use OperationId as method name")]
        [Description("Use the OperationId as method name, if valid. The default value is 'True'.")]
        public bool UseOperationIdAsMethodName { get; set; } = true;

        [Category(Interface)]
        [DisplayName("Preferred SecurityDefinition")]
        [Description("Preferred SecurityDefinition type to add to the interface. The default value is 'Automatic'.")]
        public SecurityDefinitionType PreferredSecurityDefinitionType { get; set; } = SecurityDefinitionType.Automatic;

        [Category(Interface)]
        [DisplayName("Make NonRequired parameters optional")]
        [Description("Append '= null' to opional parameters in the interface methods. The default value is 'True'.")]
        public bool MakeNonRequiredParametersOptional { get; set; } = true;
        #endregion

        #region Models
        [Category(Models)]
        [DisplayName("Generate properties as nullable for OpenApi 2.0")]
        [Description("Generate all (primitive) properties as nullable for OpenApi 2.0, the default value is 'False'.")]
        public bool GeneratePrimitivePropertiesAsNullableForOpenApi20 { get; set; } = false;

        [Category(Models)]
        [DisplayName("Support 'x-nullable'")]
        [Description("Support vendor extension 'x-nullable' to indicate a property as nullable for OpenApi 2.0, the default value is 'False'.")]
        public bool SupportExtensionXNullable { get; set; }
        #endregion

        #region MergeWith
        public void MergeWith(RestEaseUserOptions options)
        {
            bool useUserOptions = UseUserOptions;
            AutoMapperUtils.Instance.Mapper.Map(options, this);
            UseUserOptions = useUserOptions;
        }
        #endregion
    }
}