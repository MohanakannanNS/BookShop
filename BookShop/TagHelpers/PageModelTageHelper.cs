using BookShop.Models;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.TagHelpers
{
    [HtmlTargetElement("div", Attributes = "page-model")]
    public class PageModelTageHelper : TagHelper
    {
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }
        public PageInfo PagingModel { get; set; }
        public string PageAction { get; set; }
        public bool PageClassesEnabled { get; set; }
        public string PageClass { get; set; }
        public string PageClassNormal { get; set; }
        public string PageClassSelected { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            TagBuilder builder = new TagBuilder("div");
            for (int i = 1; i <= PagingModel.TotalPage; i++)
            {
                TagBuilder tag = new TagBuilder("a");
                string url = PagingModel.URLParam.Replace(":", i.ToString());
                tag.Attributes["href"] = url;
                if (PageClassesEnabled)
                {
                    tag.AddCssClass(PageClass);
                    tag.AddCssClass(i == PagingModel.CurrentPage ? PageClassSelected : PageClassNormal);
                }
                tag.InnerHtml.Append(i.ToString());
                builder.InnerHtml.AppendHtml(tag);
            }
            output.Content.AppendHtml(builder.InnerHtml);

        }
    }
}
