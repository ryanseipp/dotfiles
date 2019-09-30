// Decompiled with JetBrains decompiler
// Type: Sieve.Models.SieveModel`2
// Assembly: Sieve, Version=2.3.1.0, Culture=neutral, PublicKeyToken=null
// MVID: EADC16C5-738C-4BF8-8AF2-46E1C2E7D350
// Assembly location: /home/zorbik/.nuget/packages/sieve/2.3.1/lib/netstandard2.0/Sieve.dll

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Sieve.Models
{
  [DataContract]
  public class SieveModel<TFilterTerm, TSortTerm> : ISieveModel<TFilterTerm, TSortTerm>
    where TFilterTerm : IFilterTerm, new()
    where TSortTerm : ISortTerm, new()
  {
    private const string EscapedCommaPattern = "(?<!($|[^\\\\])(\\\\\\\\)*?\\\\),";

    [DataMember]
    public string Filters { get; set; }

    [DataMember]
    public string Sorts { get; set; }

    [DataMember]
    [Range(1, 2147483647)]
    public int? Page { get; set; }

    [DataMember]
    [Range(1, 2147483647)]
    public int? PageSize { get; set; }

    public List<TFilterTerm> GetFiltersParsed()
    {
      if (this.Filters == null)
        return (List<TFilterTerm>) null;
      List<TFilterTerm> source = new List<TFilterTerm>();
      TFilterTerm filterTerm1;
      foreach (string str1 in Regex.Split(this.Filters, "(?<!($|[^\\\\])(\\\\\\\\)*?\\\\),"))
      {
        if (!string.IsNullOrWhiteSpace(str1))
        {
          if (str1.StartsWith("("))
          {
            string oldValue = str1.Substring(str1.LastIndexOf(")") + 1);
            string str2 = str1.Replace(oldValue, "").Replace("(", "").Replace(")", "");
            filterTerm1 = new TFilterTerm();
            filterTerm1.Filter = str2 + oldValue;
            TFilterTerm filterTerm = filterTerm1;
            if (!source.Any<TFilterTerm>((Func<TFilterTerm, bool>) (f => ((IEnumerable<string>) f.Names).Any<string>((Func<string, bool>) (n => ((IEnumerable<string>) filterTerm.Names).Any<string>((Func<string, bool>) (n2 => n2 == n)))))))
              source.Add(filterTerm);
          }
          else
          {
            filterTerm1 = new TFilterTerm();
            filterTerm1.Filter = str1;
            TFilterTerm filterTerm2 = filterTerm1;
            source.Add(filterTerm2);
          }
        }
      }
      return source;
    }

    public List<TSortTerm> GetSortsParsed()
    {
      if (this.Sorts == null)
        return (List<TSortTerm>) null;
      List<TSortTerm> source = new List<TSortTerm>();
      foreach (string str in Regex.Split(this.Sorts, "(?<!($|[^\\\\])(\\\\\\\\)*?\\\\),"))
      {
        if (!string.IsNullOrWhiteSpace(str))
        {
          TSortTerm sortTerm = new TSortTerm()
          {
            Sort = str
          };
          if (!source.Any<TSortTerm>((Func<TSortTerm, bool>) (s => s.Name == sortTerm.Name)))
            source.Add(sortTerm);
        }
      }
      return source;
    }
  }
}
