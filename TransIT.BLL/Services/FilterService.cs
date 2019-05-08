using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData;
using TransIT.BLL.Helpers;
using TransIT.DAL.Models.Entities.Abstractions;
using TransIT.DAL.Models.ViewModels;
using TransIT.DAL.Repositories;

namespace TransIT.BLL.Services
{
    public class FilterService<TEntity> : IODCrudService<TEntity>
        where TEntity : class, IEntity, new()
    {        
        protected readonly IQueryRepository<TEntity> _queryRepository;
        protected readonly ICrudService<TEntity> _crudService;

        public ulong TotalRecordsAmount => (ulong)_queryRepository.GetQueryable().LongCount();
        
        public FilterService(
            IQueryRepository<TEntity> queryRepository,
            ICrudService<TEntity> crudService)
        {
            _queryRepository = queryRepository;
            _crudService = crudService;
        }

        public virtual Task<IEnumerable<TEntity>> GetQueriedAsync() =>
            Task.FromResult<IEnumerable<TEntity>>(
                _queryRepository.GetQueryable()
                );

        public virtual async Task<IEnumerable<TEntity>> GetQueriedAsync(DataTableRequestViewModel dataFilter)
        {
            if (!dataFilter.Columns.Any())
                throw new ArgumentException(
                    $"{nameof(DataTableRequestViewModel)}.{nameof(dataFilter.Columns)} is empty.");
            if (!dataFilter.Order.Any())
                throw new ArgumentException(
                    $"{nameof(DataTableRequestViewModel)}.{nameof(dataFilter.Order)} is empty.");
            
             return ProcessQuery(dataFilter, await DetermineDataSource(dataFilter));
        }
        
        public virtual Task<IEnumerable<TEntity>> GetQueriedAsync(ODataQueryOptions<TEntity> options)
        {
            try
            {
                return Task.FromResult<IEnumerable<TEntity>>(
                    options
                        .ApplyTo(_queryRepository.GetQueryable())
                        .Cast<TEntity>()
                );
            }
            catch (ODataException)
            {
                return Task.FromResult<IEnumerable<TEntity>>(null);
            }
        }

        private async Task<IQueryable<TEntity>> DetermineDataSource(DataTableRequestViewModel dataFilter) =>
            string.IsNullOrEmpty(dataFilter.Search.Value)
                ? _queryRepository.GetQueryable()
                : (await _crudService.SearchAsync(dataFilter.Search.Value)).AsQueryable();

        private IQueryable<TEntity> ProcessQuery(DataTableRequestViewModel dataFilter, IQueryable<TEntity> data) =>
            data.OrderBy(
                    dataFilter.Columns[dataFilter.Order[0].Column].Data,
                    dataFilter.Order[0].Dir == DataTableRequestViewModel.DataTableDescending
                )
                .Cast<TEntity>()
                .Skip(dataFilter.Start)
                .Take(dataFilter.Length);
    }
}