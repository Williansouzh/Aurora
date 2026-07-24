using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Aurora.API.Versioning;

/// <summary>
/// Exposes every "api/..." controller additionally under "api/v1/...", establishing a versioned
/// URL namespace without touching controllers or breaking existing "api/..." clients. A future
/// breaking revision can ship as dedicated "api/v2/..." controllers alongside these.
/// </summary>
public class ApiVersionRouteConvention : IApplicationModelConvention
{
    private const string Prefix = "api/";
    private const string VersionedPrefix = "api/v1/";

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var selector in controller.Selectors.ToList())
            {
                var template = selector.AttributeRouteModel?.Template;
                if (template is null || !template.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var versioned = VersionedPrefix + template[Prefix.Length..];
                controller.Selectors.Add(new SelectorModel(selector)
                {
                    AttributeRouteModel = new AttributeRouteModel { Template = versioned },
                });
            }
        }
    }
}
