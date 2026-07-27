using System;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.CustomProperties;

namespace Tipstaff.Helpers
{
    public static class SensitivityLabelValidator
    {
        public static bool HasSensitivityLabel(byte[] fileBytes)
        {
            try
            {
                using (var ms = new MemoryStream(fileBytes))
                using (var template = WordprocessingDocument.Open(ms, false))
                {
                    var customProps = template.CustomFilePropertiesPart;
                    if (customProps == null) return false;

                    return customProps.Properties
                        .OfType<CustomDocumentProperty>()
                        .Any(p => p.Name?.Value?.StartsWith("MSIP_Label_",
                            StringComparison.OrdinalIgnoreCase) == true);
                }
            }
            catch
            {
                return false;
            }
        }

        public static string GetLabelName(byte[] fileBytes)
        {
            try
            {
                using (var ms = new MemoryStream(fileBytes))
                using (var template = WordprocessingDocument.Open(ms, false))
                {
                    var customProps = template.CustomFilePropertiesPart;
                    if (customProps == null) return null;

                    return customProps.Properties
                        .OfType<CustomDocumentProperty>()
                        .FirstOrDefault(p => p.Name?.Value?.EndsWith("_Name",
                            StringComparison.OrdinalIgnoreCase) == true
                            && p.Name.Value.StartsWith("MSIP_Label_",
                            StringComparison.OrdinalIgnoreCase))
                        ?.InnerText?.Trim();
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
