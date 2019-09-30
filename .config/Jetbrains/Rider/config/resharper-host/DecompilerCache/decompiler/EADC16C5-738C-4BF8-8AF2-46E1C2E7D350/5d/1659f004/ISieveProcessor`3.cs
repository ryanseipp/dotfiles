// Decompiled with JetBrains decompiler
// Type: Sieve.Services.ISieveProcessor`3
// Assembly: Sieve, Version=2.3.1.0, Culture=neutral, PublicKeyToken=null
// MVID: EADC16C5-738C-4BF8-8AF2-46E1C2E7D350
// Assembly location: /home/zorbik/.nuget/packages/sieve/2.3.1/lib/netstandard2.0/Sieve.dll

using Sieve.Models;
using System.Linq;

namespace Sieve.Services
{
  public interface ISieveProcessor<TSieveModel, TFilterTerm, TSortTerm>
    where TSieveModel : class, ISieveModel<TFilterTerm, TSortTerm>
    where TFilterTerm : IFilterTerm, new()
    where TSortTerm : ISortTerm, new()
  {
    IQueryable<TEntity> Apply<TEntity>(
      TSieveModel model,
      IQueryable<TEntity> source,
      object[] dataForCustomMethods = null,
      bool applyFiltering = true,
      bool applySorting = true,
      bool applyPagination = true);
  }
}
