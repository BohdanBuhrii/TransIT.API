﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TransIT.DAL.Models;
using TransIT.DAL.Models.Entities;
using TransIT.DAL.Repositories.InterfacesRepositories;

namespace TransIT.DAL.Repositories.ImplementedRepositories
{
    public class TransitionRepository:BaseRepository<Transition>, ITransitionRepository
    {
        public TransitionRepository(TransITDBContext context)
               : base(context)
        {
        }

        public override Task<IQueryable<Transition>> SearchExpressionAsync(IEnumerable<string> strs) =>
            Task.FromResult(
                GetQueryable().Where(transition =>
                    strs.Any(str => transition.FromState.TransName.ToUpperInvariant().Contains(str)
                                    || transition.ToState.TransName.ToUpperInvariant().Contains(str)
                                    || transition.ActionType.Name.ToUpperInvariant().Contains(str)))
                );
        
        protected override IQueryable<Transition> ComplexEntities => Entities.
           Include(u => u.ActionType).
           Include(u => u.FromState).
           Include(u => u.ToState).
           Include(t => t.Create).
           Include(w => w.Mod).OrderByDescending(u => u.UpdatedDate).ThenByDescending(x => x.CreatedDate);
    }
}
