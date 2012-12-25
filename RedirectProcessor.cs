// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Redirects.cs">
//   Copyright (C) 2012 by Alexander Davyduk. All rights reserved.
// </copyright>
// <summary>
//   Redirects class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.SharedSource.RedirectManager
{
  using System.Collections;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Text.RegularExpressions;
  using System.Threading;
  using System.Web;

  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Data.Fields;
  using Sitecore.Data.Items;
  using Sitecore.SharedSource.RedirectManager.Items;
  using Sitecore.SharedSource.RedirectManager.Templates;
  using Sitecore.SharedSource.RedirectManager.Utils;

  /// <summary>
  /// Redirects class 
  /// </summary>
  public static class RedirectProcessor
  {
    /// <summary>
    ///   Cache key for items redirects
    /// </summary>
    private const string ItemsCacheKey = "Sitecore.RedirectManager:Items " + ItemIDs.RedirectsFolderItem;

    /// <summary>
    ///   Cache key for sections redirects
    /// </summary>
    private const string SectionsCacheKey = "Sitecore.RedirectManager:Sections " + ItemIDs.RedirectsFolderItem;

    /// <summary>
    ///   Cache key for RegEx redirects
    /// </summary>
    private const string RegExCacheKey = "Sitecore.RedirectManager:RegEx " + ItemIDs.RedirectsFolderItem;

    /// <summary>
    ///  local variable for synchronization
    /// </summary>
    private static readonly object SyncObject = new object();

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public static void Initialize()
    {
      UrlNormalizer.Initialize();
    }

    /// <summary>
    /// Updates the last use in thread.
    /// </summary>
    /// <param name="id">The id.</param>
    public static void UpdateLastUseInThread(string id)
    {
      var newThread = new Thread(UpdateLastUse);
      newThread.Start(id);
    }

    /// <summary>
    /// Finds the redirect.
    /// </summary>
    /// <param name="url">The requested URL.</param>
    /// <param name="redirectCode">The redirect code.</param>
    /// <param name="redirectId">The redirect id.</param>
    /// <returns>
    /// target url
    /// </returns>
    public static string FindRedirect(string url, out int redirectCode, out string redirectId)
    {
      redirectCode = Configuration.RedirectStatusCode;
      redirectId = string.Empty;

      var targetRedirect = FindItemToItemRedirect(url);
      if (targetRedirect != null)
      {
        redirectId = targetRedirect.ItemId.ToString();
        redirectCode = targetRedirect.RedirectCode;
        return PrepareRedirectUrl(targetRedirect);
      }

      targetRedirect = FindSectionToItemRedirect(url);
      if (targetRedirect != null)
      {
        redirectId = targetRedirect.ItemId.ToString();
        redirectCode = targetRedirect.RedirectCode;
        return PrepareRedirectUrl(targetRedirect);
      }

      var encodedUrl = string.Format("{0}{1}", UrlNormalizer.GetVirtualVolder(), UrlNormalizer.RemovePageExtension(UrlNormalizer.EncodeUrl(url)));
      var targetRegExRedirect = FindRegExRedirect(encodedUrl);
      if (targetRegExRedirect != null)
      {
        redirectId = targetRegExRedirect.ItemId.ToString();
        redirectCode = targetRegExRedirect.RedirectCode;
        return PrepareRedirectUrl(targetRegExRedirect, encodedUrl);
      }

      return string.Empty;
    }

    /// <summary>
    /// Gets the redirects.
    /// </summary>
    /// <param name="cacheKey">The cache key.</param>
    /// <returns>List of redirects</returns>
    public static List<RedirectItem> GetRedirects(string cacheKey)
    {
      if (HttpContext.Current == null)
      {
        return null;
      }

      var redirectsList = HttpContext.Current.Cache.Get(cacheKey) as List<RedirectItem>;

      return redirectsList;
    }

    /// <summary>
    /// Gets the regex redirects.
    /// </summary>
    /// <param name="cacheKey">The cache key.</param>
    /// <returns>
    /// The regex redirects.
    /// </returns>
    public static List<RegExItem> GetRegExRedirects(string cacheKey)
    {
      if (HttpContext.Current == null)
      {
        return null;
      }

      var redirectsList = HttpContext.Current.Cache.Get(cacheKey) as List<RegExItem>;

      return redirectsList;
    }

    /// <summary>
    /// Removes the redirect.
    /// </summary>
    /// <param name="item">The item.</param>
    public static void RemoveRedirect(Item item)
    {
      lock (SyncObject)
      {
        var sw = new Stopwatch();
        sw.Start();
        var itemsList = GetRedirects(ItemsCacheKey);
        var sectionsList = GetRedirects(SectionsCacheKey);
        var regExList = GetRegExRedirects(RegExCacheKey);

        CheckItemTypeAndRemove(item, itemsList, sectionsList, regExList, out itemsList, out sectionsList, out regExList);

        SaveRedirects(ItemsCacheKey, itemsList);
        SaveRedirects(SectionsCacheKey, sectionsList);
        SaveRedirects(RegExCacheKey, regExList);
        sw.Stop();
        LogManager.WriteInfo(
          string.Format(
            "Redirect for the {0} item was removed: elapsed time - {1} milliseconds, {2} ticks",
            item.ID,
            sw.ElapsedMilliseconds,
            sw.ElapsedTicks));
      }
    }

    /// <summary>
    /// Updates the redirect.
    /// </summary>
    /// <param name="item">The item.</param>
    public static void UpdateRedirect(Item item)
    {
      lock (SyncObject)
      {
        var sw = new Stopwatch();
        sw.Start();
        var itemsList = GetRedirects(ItemsCacheKey);
        var sectionsList = GetRedirects(SectionsCacheKey);
        var regExList = GetRegExRedirects(RegExCacheKey);

        CheckItemTypeAndRemove(item, itemsList, sectionsList, regExList, out itemsList, out sectionsList, out regExList);

        List<RedirectItem> bufItemsList;
        List<RedirectItem> bufSectionsList;
        List<RegExItem> bufRegExList;

        CheckRedirectType(item, out bufItemsList, out bufSectionsList, out bufRegExList);

        if (bufItemsList != null)
        {
          if (itemsList == null)
          {
           itemsList = new List<RedirectItem>();
          }

          itemsList.AddRange(bufItemsList);
          SaveRedirects(ItemsCacheKey, itemsList);
        }

        if (bufSectionsList != null)
        {
          if (sectionsList == null)
          {
            sectionsList = new List<RedirectItem>();
          }

          sectionsList.AddRange(bufSectionsList);
          SaveRedirects(SectionsCacheKey, sectionsList);
        }

        if (bufRegExList != null)
        {
          if (regExList == null)
          {
            regExList = new List<RegExItem>();
          }

          regExList.AddRange(bufRegExList);
          SaveRedirects(RegExCacheKey, regExList);
        }

        sw.Stop();
        LogManager.WriteInfo(
          string.Format(
            "Redirect for the {0} item was updated: elapsed time - {1} milliseconds, {2} ticks",
            item.ID,
            sw.ElapsedMilliseconds,
            sw.ElapsedTicks));
      }
    }

    /// <summary>
    /// Creates the list of redirects.
    /// </summary>
    public static void CreateListOfRedirects()
    {
      lock (SyncObject)
      {
        var sw = new Stopwatch();
        sw.Start();
        var redirectForlder = Factory.GetDatabase(Configuration.Database).GetItem(ItemIDs.RedirectsFolderItem);
        if (redirectForlder == null)
        {
          LogManager.WriteError(string.Format("Forlder with redirects \"{0}\" not found", ItemIDs.RedirectsFolderItem));
          return;
        }

        var itemsRedirectsList = new List<RedirectItem>();
        var sectionsRedirectList = new List<RedirectItem>();
        var regExRedirectList = new List<RegExItem>();
        foreach (var item in redirectForlder.Axes.GetDescendants())
        {
          List<RedirectItem> bufItemsRedirectsList;
          List<RedirectItem> bufSectionsRedirectList;
          List<RegExItem> bufRegExRedirectList;

          CheckRedirectType(item, out bufItemsRedirectsList, out bufSectionsRedirectList, out bufRegExRedirectList);
          if (bufItemsRedirectsList != null)
          {
            itemsRedirectsList.AddRange(bufItemsRedirectsList);
          }

          if (bufSectionsRedirectList != null)
          {
            sectionsRedirectList.AddRange(bufSectionsRedirectList);
          }

          if (bufRegExRedirectList != null)
          {
            regExRedirectList.AddRange(bufRegExRedirectList);
          }
        }

        SaveRedirects(ItemsCacheKey, itemsRedirectsList);
        SaveRedirects(SectionsCacheKey, sectionsRedirectList);
        SaveRedirects(RegExCacheKey, regExRedirectList);
        sw.Stop();
        LogManager.WriteInfo(
          string.Format(
            "Lists with redirects were created: total number of redirects - {0}, elapsed time - {1} milliseconds, {2} ticks",
            itemsRedirectsList.Count + sectionsRedirectList.Count + regExRedirectList.Count,
            sw.ElapsedMilliseconds,
            sw.ElapsedTicks));
      }
    }

    /// <summary>
    /// The check presentation.
    /// </summary>
    /// <param name="item">
    /// The item.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public static bool CheckPresentation(Item item)
    {
      if (!Configuration.CheckPresentation)
      {
        return true;
      }

      if (item == null)
      {
        return false;
      }

      LayoutField field = item.Fields[FieldIDs.LayoutField];
      return field != null && !string.IsNullOrEmpty(field.Value);
    }

    /// <summary>
    /// Updates the last use.
    /// </summary>
    /// <param name="id">The id.</param>
    private static void UpdateLastUse(object id)
    {
      var itemId = id.ToString();

      var item = Factory.GetDatabase(Configuration.Database).GetItem(itemId);
      if (item == null || !item.IsItemOfType(Templates.Settings.TemplateId))
      {
        return;
      }

      var settingItem = new Templates.Settings(item);
      settingItem.UpdateLastUseWithCurrentDate();
    }

    /// <summary>
    /// Prepares the redirect URL.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>
    /// The redirect URL.
    /// </returns>
    private static string PrepareRedirectUrl(RedirectItem item)
    {
      if (item == null)
      {
        return string.Empty;
      }

      var queryString = item.TargetQueryString;
      var targetUrl = item.Target;

      if (!string.IsNullOrEmpty(queryString))
      {
        targetUrl = !string.IsNullOrEmpty(Context.Request.QueryString.ToString()) ? 
          string.Format("{0}?{1}&{2}", targetUrl, queryString, Context.Request.QueryString) : 
          string.Format("{0}?{1}", targetUrl, queryString);
      }
      else
      {
        if (!string.IsNullOrEmpty(Context.Request.QueryString.ToString()))
        {
          targetUrl = string.Format("{0}?{1}", targetUrl, Context.Request.QueryString);
        }
      }

      if (!item.External)
      {
        targetUrl = string.Format("{0}{1}", UrlNormalizer.GetVirtualVolder(), targetUrl);
      }

      return UrlNormalizer.EncodeUrl(targetUrl);
    }

    /// <summary>
    /// Prepares the redirect URL.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="url">The URL.</param>
    /// <returns>
    /// The redirect URL.
    /// </returns>
    private static string PrepareRedirectUrl(RegExItem item, string url)
    {
      if (item == null)
      {
        return string.Empty;
      }

      var foundKey = item.Expression.ToString();
      var targetUrl = Regex.Replace(url, foundKey, string.Concat(item.Value, "$2"), RegexOptions.IgnoreCase);

      return UrlNormalizer.EncodeUrl(targetUrl);
    }

    /// <summary>
    /// Checks the item type and remove it.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="itemsList">The items list.</param>
    /// <param name="sectionsList">The sections list.</param>
    /// <param name="regExList">The regex list.</param>
    /// <param name="outputItemsList">The output items list.</param>
    /// <param name="outputSectionsList">The output sections list.</param>
    /// <param name="outputRegExList">The output regex list.</param>
    private static void CheckItemTypeAndRemove(
      Item item,
      List<RedirectItem> itemsList,
      List<RedirectItem> sectionsList,
      List<RegExItem> regExList,
      out List<RedirectItem> outputItemsList,
      out List<RedirectItem> outputSectionsList,
      out List<RegExItem> outputRegExList)
    {
      var templateId = item.TemplateID.ToString();
      switch (templateId)
      {
        case ItemToItem.TemplateId:
          itemsList = RemoveItemToItemRedirect(item, itemsList);
          break;

        case SectionToItem.TemplateId:
          sectionsList = RemoveSectionToItemRedirect(item, sectionsList);
          break;

        case SectionToSection.TemplateId:
          RemoveSectionToSectionRedirect(item, itemsList, sectionsList, out itemsList, out sectionsList);
          break;
        case RegExRedirect.TemplateId:
          regExList = RemoveRegExRedirect(item, regExList);
          break;
      }

      outputItemsList = itemsList;
      outputSectionsList = sectionsList;
      outputRegExList = regExList;
    }

    /// <summary>
    /// Removes the item to item redirect.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="list">The list with items redirects.</param>
    /// <returns>List with removed Item redirect</returns>
    private static List<RedirectItem> RemoveItemToItemRedirect(Item item, List<RedirectItem> list)
    {
      if (list == null)
      {
        return null;
      }

      list.RemoveAll(x => x.ItemId == item.ID);
      return list;
    }

    /// <summary>
    /// Removes the regex redirect.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="list">The list.</param>
    /// <returns>
    /// The regex redirect.
    /// </returns>
    private static List<RegExItem> RemoveRegExRedirect(Item item, List<RegExItem> list)
    {
      if (list == null)
      {
        return null;
      }

      list.RemoveAll(x => x.ItemId == item.ID);
      return list;
    }

    /// <summary>
    /// Removes the section to item redirect.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="list">The list with sections redirects.</param>
    /// <returns>List with removed Section redirect</returns>
    private static List<RedirectItem> RemoveSectionToItemRedirect(Item item, List<RedirectItem> list)
    {
      if (list == null)
      {
        return null;
      }

      list.RemoveAll(x => x.ItemId == item.ID);
      return list;
    }

    /// <summary>
    /// Removes the section to section redirect.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="itemsList">The items list.</param>
    /// <param name="sectionsList">The sections list.</param>
    /// <param name="outItemsList">The out items list.</param>
    /// <param name="outSectionsList">The out sections list.</param>
    private static void RemoveSectionToSectionRedirect(
      Item item,
      List<RedirectItem> itemsList,
      List<RedirectItem> sectionsList,
      out List<RedirectItem> outItemsList,
      out List<RedirectItem> outSectionsList)
    {
      sectionsList = RemoveSectionToItemRedirect(item, sectionsList);
      itemsList = RemoveItemToItemRedirect(item, itemsList);

      outSectionsList = sectionsList;
      outItemsList = itemsList;
    }

    /// <summary>
    /// Checks the redirect type.
    /// </summary>
    /// <param name="item">
    /// The item.
    /// </param>
    /// <param name="itemsRedirectsList">
    /// The items redirects list.
    /// </param>
    /// <param name="sectionsRedirectList">
    /// The sections redirect list.
    /// </param>
    /// <param name="regExList">
    /// The regex redirect list.
    /// </param>
    /// ///
    private static void CheckRedirectType(
      Item item, out List<RedirectItem> itemsRedirectsList, out List<RedirectItem> sectionsRedirectList, out List<RegExItem> regExList)
    {
      itemsRedirectsList = new List<RedirectItem>();
      sectionsRedirectList = new List<RedirectItem>();
      regExList = new List<RegExItem>();

      switch (item.TemplateID.ToString())
      {
        case ItemToItem.TemplateId:
          itemsRedirectsList = AddItemToList(AddItemToItemRedirect(item), itemsRedirectsList);
          break;

        case SectionToItem.TemplateId:
          sectionsRedirectList = AddItemToList(AddSectionToItemRedirect(item), sectionsRedirectList);
          break;

        case SectionToSection.TemplateId:
          itemsRedirectsList = AddItemsToList(AddSectionToSectionDescendants(item), itemsRedirectsList);
          sectionsRedirectList = AddItemToList(AddSectionToSectionRedirect(item), sectionsRedirectList);
          break;

        case RegExRedirect.TemplateId:
          regExList = AddItemToList(AddRegExRedirect(item), regExList);
          break;

        default:
          return;
      }
    }

    /// <summary>
    /// Adds the item to list.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="list">The list.</param>
    /// <returns>
    /// The list with added item.
    /// </returns>
    private static List<RedirectItem> AddItemToList(RedirectItem item, List<RedirectItem> list)
    {
      if (item != null)
      {
        if (!CheckDuplicates(item.Base, list))
        {
          list.Add(item);
        }
      }

      return list;
    }

    /// <summary>
    /// Adds the item to list.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="list">The list.</param>
    /// <returns>
    /// The item to list.
    /// </returns>
    private static List<RegExItem> AddItemToList(RegExItem item, List<RegExItem> list)
    {
      if (item != null)
      {
        if (!CheckDuplicates(item.Expression, list))
        {
          list.Add(item);
        }
      }

      return list;
    }

    /// <summary>
    /// Adds the items to list.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <param name="list">The list.</param>
    /// <returns>
    /// The list with added items.
    /// </returns>
    private static List<RedirectItem> AddItemsToList(IEnumerable<RedirectItem> items, List<RedirectItem> list)
    {
      if (items != null)
      {
        if (Configuration.CheckDuplicates)
        {
          foreach (var sectionItem in items)
          {
            if (!CheckDuplicates(sectionItem.Base, list))
            {
              list.Add(sectionItem);
            }
          }
        }
        else
        {
          list.AddRange(items);
        }
      }

      return list;
    }

    /// <summary>
    /// Adds the item to item redirect.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>Redirect item</returns>
    private static RedirectItem AddItemToItemRedirect(Item item)
    {
      if (item == null)
      {
        return null;
      }
      
      var itemToItem = new ItemToItem(item);
      if (string.IsNullOrEmpty(itemToItem.TargetItem.Url) || string.IsNullOrEmpty(itemToItem.BaseItem.Value))
      {
        return null;
      }

      if (!CheckPresentation(itemToItem.TargetItem.TargetItem))
      {
        return null;
      }

      var external = true;
      var targetUrl = itemToItem.TargetItem.Url;
      if (itemToItem.TargetItem.IsInternal)
      {
        targetUrl = UrlNormalizer.GetItemUrl(itemToItem.TargetItem.TargetItem);
        external = false;
      }

      var redirectItem = new RedirectItem
        {
          ItemId = itemToItem.ID,
          Target = targetUrl,
          TargetQueryString = itemToItem.TargetItem.QueryString,
          Base = UrlNormalizer.CheckPageExtension(UrlNormalizer.Normalize(itemToItem.BaseItem.Value)),
          External = external,
          RedirectCode = itemToItem.RedirectCode
        };

      return redirectItem;
    }

    /// <summary>
    /// Adds the section to item redirect.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>Redirect item</returns>
    private static RedirectItem AddSectionToItemRedirect(Item item)
    {
      if (item == null)
      {
        return null;
      }

      var sectionToItem = new SectionToItem(item);
      if (string.IsNullOrEmpty(sectionToItem.BaseSection.Value) || string.IsNullOrEmpty(sectionToItem.TargetItem.Url))
      {
        return null;
      }

      if (!CheckPresentation(sectionToItem.TargetItem.TargetItem))
      {
        return null;
      }

      var external = true;
      var targetUrl = sectionToItem.TargetItem.Url;
      if (sectionToItem.TargetItem.IsInternal)
      {
        targetUrl = UrlNormalizer.GetItemUrl(sectionToItem.TargetItem.TargetItem);
        external = false;
      }

      var redirectItem = new RedirectItem
        {
          ItemId = sectionToItem.ID,
          Target = targetUrl,
          TargetQueryString = sectionToItem.TargetItem.QueryString,
          Base = UrlNormalizer.RemovePageExtension(UrlNormalizer.Normalize(sectionToItem.BaseSection.Value)),
          External = external,
          RedirectCode = sectionToItem.RedirectCode
        };

      return redirectItem;
    }

    /// <summary>
    /// Adds the section to section redirect.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>Redirect item</returns>
    private static RedirectItem AddSectionToSectionRedirect(Item item)
    {
      if (item == null)
      {
        return null;
      }

      var sectionToSection = new SectionToSection(item);
      if (string.IsNullOrEmpty(sectionToSection.BaseSection.Value) || sectionToSection.TargetSection.TargetItem == null)
      {
        return null;
      }

      if (!CheckPresentation(sectionToSection.TargetSection.TargetItem))
      {
        return null;
      }

      var redirectItem = new RedirectItem
        {
          ItemId = sectionToSection.ID,
          Target = UrlNormalizer.GetItemUrl(sectionToSection.TargetSection.TargetItem),
          Base = UrlNormalizer.Normalize(sectionToSection.BaseSection.Value),
          RedirectCode = sectionToSection.RedirectCode
        };

      return redirectItem;
    }

    /// <summary>
    /// Adds the section to section items.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>List with items redirects</returns>
    private static IEnumerable<RedirectItem> AddSectionToSectionDescendants(Item item)
    {
      if (item == null)
      {
        return null;
      }

      var sectionToSection = new SectionToSection(item);
      if (string.IsNullOrEmpty(sectionToSection.BaseSection.Value) || sectionToSection.TargetSection.TargetItem == null)
      {
        return null;
      }

      var list = AddSectionDescendants(
        sectionToSection.TargetSection.TargetItem.ID.ToString(),
        sectionToSection.BaseSection.Value,
        UrlNormalizer.GetItemUrl(sectionToSection.TargetSection.TargetItem),
        sectionToSection.ID, 
        sectionToSection.RedirectCode);

      return list;
    }

    /// <summary>
    /// Adds the regex redirect.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>redirect item</returns>
    private static RegExItem AddRegExRedirect(Item item)
    {
      if (item == null)
      {
        return null;
      }

      var regExRedirect = new RegExRedirect(item);
      if (string.IsNullOrEmpty(regExRedirect.Expression.Value) || string.IsNullOrEmpty(regExRedirect.Value.Value))
      {
        return null;
      }

      var redirectItem = new RegExItem
      {
        ItemId = regExRedirect.ID,
        Value = new Regex(regExRedirect.Value.Value, RegexOptions.IgnoreCase),
        Expression = new Regex(regExRedirect.Expression.Value, RegexOptions.IgnoreCase),
        RedirectCode = regExRedirect.RedirectCode
      };

      return redirectItem;
    }

    /// <summary>
    /// Finds the item to item redirect.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>Item target url</returns>
    private static RedirectItem FindItemToItemRedirect(string url)
    {
      var list = GetRedirects(ItemsCacheKey);

      if (list == null)
      {
        return null;
      }

      var found = list.AsParallel().Where(x => x.Base.GetHashCode() == url.GetHashCode());
      return found.FirstOrDefault(x => x.Base == url);
    }

    /// <summary>
    /// Finds the section to item.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>section target url</returns>
    private static RedirectItem FindSectionToItemRedirect(string url)
    {
      var list = GetRedirects(SectionsCacheKey);

      if (list == null)
      {
        return null;
      }

      return list.AsParallel().FirstOrDefault(x => url.Contains(x.Base));
    }

    /// <summary>
    /// Finds the regex redirect.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>
    /// The regex redirect.
    /// </returns>
    private static RegExItem FindRegExRedirect(string url)
    {
      var list = GetRegExRedirects(RegExCacheKey);

      if (list == null)
      {
        return null;
      }

      return list.AsParallel().FirstOrDefault(x => x.Expression.IsMatch(url));
    }

    /// <summary>
    /// Adds the section descendants.
    /// </summary>
    /// <param name="targetItemId">The target item id.</param>
    /// <param name="baseSectionUrl">The base section URL.</param>
    /// <param name="targetSectionUrl">The target section URL.</param>
    /// <param name="itemId">The item id.</param>
    /// <param name="redirectCode">The redirect code.</param>
    /// <returns>
    /// list of items redirects
    /// </returns>
    private static IEnumerable<RedirectItem> AddSectionDescendants(
      string targetItemId, string baseSectionUrl, string targetSectionUrl, ID itemId, int redirectCode)
    {
      var sectionItem = Factory.GetDatabase(Configuration.Database).GetItem(targetItemId);

      if (sectionItem == null)
      {
        return null;
      }

      var list = new List<RedirectItem>();

      if (CheckPresentation(sectionItem))
      {
        var sectionRedirectItem = new RedirectItem
        {
          ItemId = itemId,
          Target = targetSectionUrl,
          Base = UrlNormalizer.CheckPageExtension(baseSectionUrl),
          RedirectCode = redirectCode
        };

        list.Add(sectionRedirectItem);
      }

      baseSectionUrl = UrlNormalizer.Normalize(baseSectionUrl);
      targetSectionUrl = UrlNormalizer.RemovePageExtension(targetSectionUrl);
      

      if (targetSectionUrl == "/")
      {
        list.AddRange(
          from item in sectionItem.Axes.GetDescendants().Where(CheckPresentation)
          select UrlNormalizer.GetItemUrl(item)
          into targetUrl
            let baseUrl = string.Format("{0}{1}", baseSectionUrl, targetUrl)
          select new RedirectItem
          {
            ItemId = itemId,
            Target = targetUrl,
            Base = baseUrl,
            External = false,
            RedirectCode = redirectCode
          });
      }
      else
      {
        list.AddRange(
         from item in sectionItem.Axes.GetDescendants().Where(CheckPresentation)
         select UrlNormalizer.GetItemUrl(item)
           into targetUrl
           let baseUrl = targetUrl.Replace(targetSectionUrl, baseSectionUrl)
           select new RedirectItem
           {
             ItemId = itemId,
             Target = targetUrl,
             Base = baseUrl,
             External = false,
             RedirectCode = redirectCode
           });
      }

      return list;
    }

    /// <summary>
    /// Saves the redirects.
    /// </summary>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="redirectsList">The redirects list.</param>
    private static void SaveRedirects(string cacheKey, ICollection redirectsList)
    {
      if (redirectsList != null && redirectsList.Count > 0 && HttpContext.Current != null)
      {
        HttpContext.Current.Cache.Insert(cacheKey, redirectsList);
      }
    }

    /// <summary>
    /// Checks the duplicates.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="redirectsList">The redirects list.</param>
    /// <returns>true if there are a duplicates, false otherwise</returns>
    private static bool CheckDuplicates(string url, IEnumerable<RedirectItem> redirectsList)
    {
      if (Configuration.CheckDuplicates)
      {
        if (redirectsList != null)
        {
          var found = redirectsList.AsParallel().Where(x => x.Base == url);
          var firstOrDefault = found.FirstOrDefault();
          if (firstOrDefault != null)
          {
            return true;
          }
        }
      }

      return false;
    }

    /// <summary>
    /// Checks the duplicates.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <param name="redirectsList">The redirects list.</param>
    /// <returns>
    /// The duplicates.
    /// </returns>
    private static bool CheckDuplicates(Regex expression, IEnumerable<RegExItem> redirectsList)
    {
      if (Configuration.CheckDuplicates)
      {
        if (redirectsList != null)
        {
          var found = redirectsList.AsParallel().Where(x => x.Expression == expression);
          var firstOrDefault = found.FirstOrDefault();
          if (firstOrDefault != null)
          {
            return true;
          }
        }
      }

      return false;
    }
  }
}