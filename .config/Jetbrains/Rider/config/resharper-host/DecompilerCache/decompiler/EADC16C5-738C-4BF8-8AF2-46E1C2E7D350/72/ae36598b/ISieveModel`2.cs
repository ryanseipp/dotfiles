// Decompiled with JetBrains decompiler
// Type: Sieve.Models.ISieveModel`2
// Assembly: Sieve, Version=2.3.1.0, Culture=neutral, PublicKeyToken=null
// MVID: EADC16C5-738C-4BF8-8AF2-46E1C2E7D350
// Assembly location: /home/zorbik/.nuget/packages/sieve/2.3.1/lib/netstandard2.0/Sieve.dll

using System.Collections.Generic;

namespace Sieve.Models
{
  public interface ISieveModel<TFilterTerm, TSortTerm>
    where TFilterTerm : IFilterTerm
    where TSortTerm : ISortTerm
  {
    string Filters { get; set; }

    string Sorts { get; set; }

    int? Page { get; set; }

    int? PageSize { get; set; }

    List<TFilterTerm> GetFiltersParsed();

    List<TSortTerm> GetSortsParsed();
  }
}
