using Microsoft.EntityFrameworkCore;
using QA.Search.Generic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace QA.Search.Generic.DAL.Services.Extensions
{
    public static class EntityBuilderExtension
    {
        public static ModelBuilder AddM2MRelationship<TEntity, TLeftRelatedEntity>(
            this ModelBuilder modelBuilder,
            string m2mRelanshipTableName,
            Expression<Func<TEntity, IEnumerable<TLeftRelatedEntity>>> leftNavigationExpression,
            Expression<Func<TLeftRelatedEntity, IEnumerable<TEntity>>> rightNavigationExpression)
            where TEntity : GenericItem
            where TLeftRelatedEntity : GenericItem
        {
            modelBuilder
                .Entity<TEntity>()
                .HasMany(leftNavigationExpression)
                .WithMany(rightNavigationExpression)
                .UsingEntity(
                m2mRelanshipTableName,
                l => l.HasOne(typeof(TLeftRelatedEntity))
                      .WithMany()
                      .HasForeignKey("linked_id")
                      .HasPrincipalKey("ContentItemID"),
                r => r.HasOne(typeof(TEntity))
                      .WithMany()
                      .HasForeignKey("id")
                      .HasPrincipalKey("ContentItemID"),
                j => j.HasKey("id", "linked_id"));

            return modelBuilder;
        }
    }
}
