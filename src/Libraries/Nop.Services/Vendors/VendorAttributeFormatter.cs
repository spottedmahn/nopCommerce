using System.Net;
using System.Text;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Html;
using Nop.Services.Localization;

namespace Nop.Services.Vendors
{
    /// <summary>
    /// Represents a vendor attribute formatter implementation
    /// </summary>
    public partial class VendorAttributeFormatter : IVendorAttributeFormatter
    {
        #region Fields

        private readonly IVendorAttributeParser _vendorAttributeParser;
        private readonly IVendorAttributeService _vendorAttributeService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="vendorAttributeParser">Vendor attribute parser</param>
        /// <param name="vendorAttributeService">Vendor attribute service</param>
        /// <param name="workContext">Work context</param>
        public VendorAttributeFormatter(IVendorAttributeParser vendorAttributeParser,
            IVendorAttributeService vendorAttributeService,
            IWorkContext workContext)
        {
            this._vendorAttributeParser = vendorAttributeParser;
            this._vendorAttributeService = vendorAttributeService;
            this._workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Format vendor attributes
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="separator">Separator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        /// <returns>Formatted attributes</returns>
        public virtual string FormatAttributes(string attributesXml, string separator = "<br />", bool htmlEncode = true)
        {
            var result = new StringBuilder();

            var attributes = _vendorAttributeParser.ParseVendorAttributes(attributesXml);
            for (var i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];
                var valuesStr = _vendorAttributeParser.ParseValues(attributesXml, attribute.Id);
                for (var j = 0; j < valuesStr.Count; j++)
                {
                    var valueStr = valuesStr[j];
                    var formattedAttribute = "";
                    if (!attribute.ShouldHaveValues())
                    {
                        //no values
                        if (attribute.AttributeControlType == AttributeControlType.MultilineTextbox)
                        {
                            //multiline textbox
                            var attributeName = attribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id);
                            //encode (if required)
                            if (htmlEncode)
                                attributeName = WebUtility.HtmlEncode(attributeName);
                            formattedAttribute = $"{attributeName}: {HtmlHelper.FormatText(valueStr, false, true, false, false, false, false)}";
                            //we never encode multiline textbox input
                        }
                        else if (attribute.AttributeControlType == AttributeControlType.FileUpload)
                        {
                            //file upload
                            //not supported for vendor attributes
                        }
                        else
                        {
                            //other attributes (textbox, datepicker)
                            formattedAttribute = $"{attribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id)}: {valueStr}";
                            //encode (if required)
                            if (htmlEncode)
                                formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);
                        }
                    }
                    else
                    {
                        if (int.TryParse(valueStr, out int attributeValueId))
                        {
                            var attributeValue = _vendorAttributeService.GetVendorAttributeValueById(attributeValueId);
                            if (attributeValue != null)
                            {
                                formattedAttribute = $"{attribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id)}: {attributeValue.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id)}";
                            }
                            //encode (if required)
                            if (htmlEncode)
                                formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);
                        }
                    }

                    if (!string.IsNullOrEmpty(formattedAttribute))
                    {
                        if (i != 0 || j != 0)
                            result.Append(separator);
                        result.Append(formattedAttribute);
                    }
                }
            }

            return result.ToString();
        }

        #endregion
    }
}