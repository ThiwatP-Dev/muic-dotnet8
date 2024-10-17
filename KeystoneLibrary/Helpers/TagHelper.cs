using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace KeystoneLibrary.Helpers
{
    /// <summary>
    /// This tag helper used for displaying name of property that specified in [Display(Name = "xxx")] annotation
    /// If no display name specified, it will use inner content instead. 
    /// </summary>
    [HtmlTargetElement(Attributes = ForAttributeName)]
    public abstract class BaseDisplayNameTagHelper : TagHelper
    {
        protected const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public virtual ModelExpression For { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Get inner content that specified in yyy of <th asp-for="xxx">yyy</th>
            var existingContent = (await output.GetChildContentAsync()).GetContent();
            output.Content.SetContent(For.Metadata.DisplayName ?? existingContent);

            await base.ProcessAsync(context, output);
        }
    }

    /// <summary>
    /// This tag helper used for displaying name of property that specified in [Display(Name = "xxx")] annotation
    /// If no display name specified, it will use inner content instead. 
    /// </summary>
    [HtmlTargetElement("span", Attributes = ForAttributeName)]
    public class SpanTagHelper : BaseDisplayNameTagHelper { }

    /// <summary>
    /// This tag helper used for displaying name of property that specified in [Display(Name = "xxx")] annotation
    /// If no display name specified, it will use inner content instead. 
    /// </summary>
    [HtmlTargetElement("th", Attributes = ForAttributeName)]
    public class ThTagHelper : BaseDisplayNameTagHelper { }
}