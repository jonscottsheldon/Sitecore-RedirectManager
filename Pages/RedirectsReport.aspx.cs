// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RedirectsReport.aspx.cs" company="Sitecore A/S">
//   Copyright (C) 2012 by Alexander Davyduk. All rights reserved.
// </copyright>
// <summary>
//   Defines the RedirectsReport type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.SharedSource.RedirectManager.Pages
{
  using System;
  using System.Web.UI.WebControls;
  using Sitecore.Configuration;
  using Sitecore.Data.Items;
  using Sitecore.SharedSource.RedirectManager.Templates;
  using Sitecore.SharedSource.RedirectManager.Utils;

  /// <summary>
  /// The RedirectsReport
  /// </summary>
  public partial class RedirectsReport : System.Web.UI.Page
  {
    /// <summary>
    /// The page_ load.
    /// </summary>
    /// <param name="sender">
    /// The sender.
    /// </param>
    /// <param name="e">
    /// The e.
    /// </param>
    protected void Page_Load(object sender, EventArgs e)
    {
      this.GenerateReport();
    }

    /// <summary>
    /// Builds the name of the item to item node.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>item to item node name</returns>
    private static string BuildItemToItemNodeName(ItemToItem item)
    {
      if (item == null)
      {
        return string.Empty;
      }

      var targetUrl = "Empty";
      if (!string.IsNullOrEmpty(item.TargetItem.Url))
      {
        targetUrl = item.TargetItem.IsInternal ? UrlNormalizer.GetItemUrl(item.TargetItem.TargetItem) : item.TargetItem.Url;
      }

      return string.Format(
           "<div class=\"block-name\"><div class=\"name\">{0}</div><div class=\"title\">Base Url: {2}, Target Url: {3}</div></div><div class=\"description\">Redirect Code: {4}, Last Use: {5}, ID: {1}</div>",
           item.Name,
           item.ID,
           string.IsNullOrEmpty(item.BaseItem.Value) ? "Empty" : UrlNormalizer.CheckPageExtension(UrlNormalizer.Normalize(item.BaseItem.Value)),
           targetUrl,
           item.RedirectCode != 0 ? item.RedirectCode : Configuration.RedirectStatusCode,
           item.LastUse.DateTime.ToString("MM/dd/yy") != "01/01/01" ? item.LastUse.DateTime.ToString("MM/dd/yy") : "Never");
    }

    /// <summary>
    /// Builds the section to item node.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>section to item node</returns>
    private static string BuildSectionToItemNode(SectionToItem item)
    {
      if (item == null)
      {
        return string.Empty;
      }

      var targetUrl = "Empty";
      if (!string.IsNullOrEmpty(item.TargetItem.Url))
      {
        targetUrl = item.TargetItem.IsInternal ? UrlNormalizer.GetItemUrl(item.TargetItem.TargetItem) : item.TargetItem.Url;
      }

      return string.Format(
         "<div class=\"block-name\"><div class=\"name\">{0}</div><div class=\"title\">Base Section Url: {2}, Target Url: {3}</div></div><div class=\"description\">Redirect Code: {4}, Last Use: {5}, ID: {1}</div>",
         item.Name,
         item.ID,
         string.IsNullOrEmpty(item.BaseSection.Value) ? "Empty" : UrlNormalizer.CheckPageExtension(UrlNormalizer.Normalize(item.BaseSection.Value)),
         targetUrl,
         item.RedirectCode != 0 ? item.RedirectCode : Configuration.RedirectStatusCode,
         item.LastUse.DateTime.ToString("MM/dd/yy") != "01/01/01" ? item.LastUse.DateTime.ToString("MM/dd/yy") : "Never");
    }

    /// <summary>
    /// Builds the section to section node.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>section to section node</returns>
    private static string BuildSectionToSectionNode(SectionToSection item)
    {
      if (item == null)
      {
        return string.Empty;
      }

      return string.Format(
         "<div class=\"block-name\"><div class=\"name\">{0}</div><div class=\"title\">Base Section Url: {2}, Target Url: {3}</div></div><div class=\"description\">Redirect Code: {4}, Last Use: {5}, ID: {1}</div>",
         item.Name,
         item.ID,
         string.IsNullOrEmpty(item.BaseSection.Value) ? "Empty" : UrlNormalizer.CheckPageExtension(UrlNormalizer.Normalize(item.BaseSection.Value)),
         item.TargetSection.TargetItem != null ? UrlNormalizer.GetItemUrl(item.TargetSection.TargetItem) : "Empty",
         item.RedirectCode != 0 ? item.RedirectCode : Configuration.RedirectStatusCode,
         item.LastUse.DateTime.ToString("MM/dd/yy") != "01/01/01" ? item.LastUse.DateTime.ToString("MM/dd/yy") : "Never");
    }

    /// <summary>
    /// Builds the name of the regex node.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>regex node</returns>
    private static string BuildRegExNodeName(RegExRedirect item)
    {
      if (item == null)
      {
        return string.Empty;
      }

      return string.Format(
            "<div class=\"block-name\"><div class=\"name\">{0}</div><div class=\"title\">Expression: {2}, Value: {3}</div></div><div class=\"description\">Redirect Code: {4}, Last Use: {5}, ID: {1}</div>",
            item.Name,
            item.ID,
            !string.IsNullOrEmpty(item.Expression.Value) ? item.Expression.Value : "Empty",
            !string.IsNullOrEmpty(item.Value.Value) ? item.Value.Value : "Empty",
            item.RedirectCode != 0 ? item.RedirectCode : Configuration.RedirectStatusCode,
            item.LastUse.DateTime.ToString("MM/dd/yy") != "01/01/01" ? item.LastUse.DateTime.ToString("MM/dd/yy") : "Never");
    }

    /// <summary>
    /// Gets the icon path.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>icon path</returns>
    private static string GetIconPath(Item item)
    {
      return item == null ? string.Empty : string.Format("/~/icon/{0}", item["__icon"]);
    }

    /// <summary>
    /// The build node name.
    /// </summary>
    /// <param name="item">
    /// The item.
    /// </param>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    private static string BuildNodeName(Item item)
    {
      if (item == null)
      {
        return string.Empty;
      }

      var templateId = item.TemplateID.ToString();
      switch (templateId)
      {
        case ItemToItem.TemplateId:
          var itemToItem = new ItemToItem(item);
          return BuildItemToItemNodeName(itemToItem);

        case SectionToItem.TemplateId:
          var sectionToItem = new SectionToItem(item);
          return BuildSectionToItemNode(sectionToItem);

        case SectionToSection.TemplateId:
          var sectionToSection = new SectionToSection(item);
          return BuildSectionToSectionNode(sectionToSection);

        case RegExRedirect.TemplateId:
          var regExRedirect = new RegExRedirect(item);
          return BuildRegExNodeName(regExRedirect);
      }

      return string.Format("<p>{0}</p>", item.Name);
    }

    /// <summary>
    /// Generates the report.
    /// </summary>
    private void GenerateReport()
    {
      if (this.Page.IsPostBack)
      {
        return;
      }

      this.Redirects.Nodes.Clear();
      var rootItem = Factory.GetDatabase(Configuration.Database).GetItem(RedirectManager.Items.ItemIDs.RedirectsFolderItem);

      if (rootItem == null)
      {
        return;
      }

      var rootNode = new TreeNode(BuildNodeName(rootItem), rootItem.ID.ToString(), GetIconPath(rootItem));
      this.AddChildNodes(rootNode, rootItem);
      this.Redirects.Nodes.Add(rootNode);
    }

    /// <summary>
    /// Adds the child nodes.
    /// </summary>
    /// <param name="rootNode">The root node.</param>
    /// <param name="rootItem">The root item.</param>
    private void AddChildNodes(TreeNode rootNode, Item rootItem)
    {
      foreach (var item in rootItem.Children.ToArray())
      {
        var node = new TreeNode(BuildNodeName(item), item.ID.ToString(), GetIconPath(item));
        this.AddChildNodes(node, item);
        rootNode.ChildNodes.Add(node);
      }
    }
  }
}