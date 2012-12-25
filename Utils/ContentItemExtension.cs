// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContentItemExtension.cs" >
//   Copyright (C) 2012 by Alexander Davyduk. All rights reserved.
// </copyright>
// <summary>
//   Extensions for ContentItem and inheritors
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.SharedSource.RedirectManager.Utils
{
  using System.Linq;
  using System.Web;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Data.Managers;
  using Sitecore.Data.Templates;
  using Sitecore.StringExtensions;

  /// <summary>
  /// Extensions for <c>ContentItem</c> and inheritors
  /// </summary>
  public static class ContentItemExtension
  {
    /// <summary>
    /// Key of the base templates cache.
    /// </summary>
    private const string BaseTemplatesCacheKey = "{0}HasBaseTemplate{1}";

    /// <summary>
    /// Determines whether [is item of type] [the specified item].
    /// </summary>
    /// <param name="item">The item .</param>
    /// <param name="baseTemplateId">The base template id.</param>
    /// <returns>
    /// <c>true</c> if [is item of type] [the specified item]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsItemOfType(this Item item, ID baseTemplateId)
    {
      if (item == null)
      {
        return false;
      }

      var result = HttpRuntime.Cache.Get(BaseTemplatesCacheKey.FormatWith(item.TemplateID, baseTemplateId));
      if (result == null)
      {
        result = HasBaseTemplate(TemplateManager.GetTemplate(item), baseTemplateId);
        HttpRuntime.Cache.Insert(BaseTemplatesCacheKey.FormatWith(item.TemplateID, baseTemplateId), result);
      }

      return (bool)result;
    }

    /// <summary>
    /// Determines whether [is item descendant of] [the specified item].
    /// </summary>
    /// <param name="item">The item .</param>
    /// <param name="parentTemplateId">The parent template id.</param>
    /// <returns>
    /// <c>true</c> if [is item descendant of] [the specified item]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsItemDescendantOf(this Item item, ID parentTemplateId)
    {
      if (item == null)
      {
        return false;
      }

      return !item.Axes.GetAncestors().ToList().TrueForAll(
        i => !i.IsItemOfType(parentTemplateId));
    }

    /// <summary>
    /// Determines whether [has base template] [the specified template].
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="baseTemplateId">The base template id.</param>
    /// <returns>
    ///  true if has base template the specified template otherwise, false.
    /// </returns>
    private static bool HasBaseTemplate(Template template, ID baseTemplateId)
    {
      if (template == null)
      {
        return false;
      }

      if (template.ID == baseTemplateId)
      {
        return true;
      }

      foreach (Template baseTemplate in template.GetBaseTemplates())
      {
        if (baseTemplate.ID == baseTemplateId)
        {
          return true;
        }
      }

      return false;
    }
  }
}