using QA.Search.Generic.DAL.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace QA.Search.Generic.Integration.QP.Extensions
{
    /// <summary>
    /// Методы-расширения DSL для описания индексации контентов документов из БД QP:
    /// выбор полей, JOIN таблиц, условия фильтрации.
    /// </summary>
    public static class QueryableExtensions
    {
        #region JoinOne

        /// <summary>
        /// Загрузить связь ManyToOne
        /// </summary>
        /// <example>
        /// Db.NewsRubrics.JoinOne(rubric => rubric.Group)
        /// </example>
        public static IQueryable<TSource> JoinOne<TSource, TProperty>(
            this IQueryable<TSource> source, Expression<Func<TSource, TProperty>> selector)
            where TSource : IGenericItem
            where TProperty : IGenericItem
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            return source.Provider.CreateQuery<TSource>(Expression.Call(
                null, GetMethodInfo(JoinOne, source, selector),
                new[] { source.Expression, Expression.Quote(selector) }
            ));
        }

        /// <example>
        /// Db.News.JoinMany(
        ///     news => news.Regions,
        ///     link => link.MarketingRegionLinkedItem
        ///         .JoinOne(region => region.Parent))
        /// </example>
        public static ICollection<TSource> JoinOne<TSource, TProperty>(
           this ICollection<TSource> source, Func<TSource, TProperty> selector)
           where TSource : IGenericItem
           where TProperty : IGenericItem
        {
            throw new NotSupportedException();
        }

        /// <example>
        /// Db.NewsRubrics.JoinOne(rubric => rubric.Group
        ///     .JoinOne(group => group.MainRubric))
        /// </example>
        public static TSource JoinOne<TSource, TProperty>(
            this TSource source, Func<TSource, TProperty> selector)
            where TSource : IGenericItem
            where TProperty : IGenericItem
        {
            throw new NotSupportedException();
        }

        #endregion

        #region JoinMany

        /// <summary>
        /// Загрузить связь OneToMany
        /// </summary>
        /// <example>
        /// Db.Regions.JoinMany(region => region.Children)
        /// </example>
        public static IQueryable<TSource> JoinMany<TSource, TElement>(
            this IQueryable<TSource> source, Expression<Func<TSource, ICollection<TElement>>> selector)
            where TSource : IGenericItem
            where TElement : IGenericItem
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            return source.Provider.CreateQuery<TSource>(Expression.Call(
                null, GetMethodInfo(JoinMany, source, selector),
                new[] { source.Expression, Expression.Quote(selector) }
            ));
        }

        /// <example>
        /// Db.Regions.JoinMany(region => region.Children.JoinMany(child => child.Children))
        /// </example>
        public static ICollection<TSource> JoinMany<TSource, TElement>(
            this ICollection<TSource> source, Func<TSource, ICollection<TElement>> selector)
            where TSource : IGenericItem
            where TElement : IGenericItem
        {
            throw new NotSupportedException();
        }

        /// <example>
        /// Db.Regions.JoinOne(region => region.Parent.JoinMany(parent => parent.Children))
        /// </example>
        public static TSource JoinMany<TSource, TElement>(
            this TSource source, Func<TSource, ICollection<TElement>> selector)
            where TSource : IGenericItem
            where TElement : IGenericItem
        {
            throw new NotSupportedException();
        }

        #endregion

        #region JoinManyToMany

        /// <summary>
        /// Загрузить связь ManyToMany
        /// </summary>
        /// <example>
        /// Db.News.JoinMany(news => news.Regions, link => link.MarketingRegionLinkedItem)
        /// </example>
        public static IQueryable<TSource> JoinMany<TSource, TLink, TElement>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, ICollection<TLink>>> collectionSelector,
            Expression<Func<TLink, TElement>> elementSelector)
            where TSource : IGenericItem
            where TLink : IGenericLink
            where TElement : IGenericItem
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (collectionSelector == null) throw new ArgumentNullException(nameof(collectionSelector));
            if (elementSelector == null) throw new ArgumentNullException(nameof(elementSelector));

            return source.Provider.CreateQuery<TSource>(Expression.Call(
                null, GetMethodInfo(JoinMany, source, collectionSelector, elementSelector),
                new[] { source.Expression, Expression.Quote(collectionSelector), Expression.Quote(elementSelector) }
            ));
        }

        public static ICollection<TSource> JoinMany<TSource, TLink, TElement>(
            this ICollection<TSource> source,
            Func<TSource, ICollection<TLink>> collectionSelector,
            Func<TLink, TElement> elementSelector)
            where TSource : IGenericItem
            where TLink : IGenericLink
            where TElement : IGenericItem
        {
            throw new NotSupportedException();
        }

        public static TSource JoinMany<TSource, TLink, TElement>(
            this TSource source,
            Func<TSource, ICollection<TLink>> collectionSelector,
            Func<TLink, TElement> elementSelector)
            where TSource : IGenericItem
            where TLink : IGenericLink
            where TElement : IGenericItem
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Pick

        /// <summary>
        /// Выбрать поля для индексации
        /// </summary>
        /// <example>
        /// Db.News.Pick(news => news.Id).Pick(news => news.Title)
        /// Db.News.Pick(news => new { news.Id, news.Title, news.Text })
        /// </example>
        public static IQueryable<TSource> Pick<TSource, TProperty>(
            this IQueryable<TSource> source, Expression<Func<TSource, TProperty>> selector)
            where TSource : IGenericItem
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            return source.Provider.CreateQuery<TSource>(Expression.Call(
                null, GetMethodInfo(Pick, source, selector),
                new[] { source.Expression, selector }
            ));
        }

        /// <example>
        /// Db.News.JoinMany(
        ///     news => news.Regions,
        ///     link => link.MarketingRegionLinkedItem
        ///         .Pick(region => new { region.Alias, region.Title }))
        /// </example>
        public static ICollection<TSource> Pick<TSource, TProperty>(
            this ICollection<TSource> source, Func<TSource, TProperty> selector)
            where TSource : IGenericItem
        {
            throw new NotSupportedException();
        }


        /// <example>
        /// Db.NewsRubrics.JoinOne(
        ///     rubric => rubric.Group.Pick(group => group.Id))
        /// </example>
        public static TSource Pick<TSource, TProperty>(
            this TSource source, Func<TSource, TProperty> selector)
            where TSource : IGenericItem
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Omit

        /// <summary>
        /// Удалить поля из индексации
        /// </summary>
        /// <example>
        /// Db.News.Omit(news => news.PublishDate).Omit(news => news.Anounce)
        /// Db.News.Omit(news => new { news.PublishDate, news.Anounce })
        /// </example>
        public static IQueryable<TSource> Omit<TSource, TProperty>(
            this IQueryable<TSource> source, Expression<Func<TSource, TProperty>> selector)
            where TSource : IGenericItem
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            return source.Provider.CreateQuery<TSource>(Expression.Call(
                null, GetMethodInfo(Omit, source, selector),
                new[] { source.Expression, selector }
            ));
        }

        /// <example>
        /// Db.News.JoinMany(
        ///     news => news.Regions,
        ///     link => link.MarketingRegionLinkedItem.Omit(region => region.Text))
        /// </example>
        public static ICollection<TSource> Omit<TSource, TProperty>(
            this ICollection<TSource> source, Func<TSource, TProperty> selector)
            where TSource : IGenericItem
        {
            throw new NotSupportedException();
        }

        /// <example>
        /// Db.NewsRubrics.JoinOne(
        ///     rubric => rubric.Group.Omit(group => group.Title))
        /// </example>
        public static TSource Omit<TSource, TProperty>(
            this TSource source, Func<TSource, TProperty> selector)
            where TSource : IGenericItem
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Filter

        /// <summary>
        /// Индексировать только статьи, подходящие под фильтр.
        /// Допустима фильтрация только по простым полям (без Navigation Properties)
        /// </summary>
        /// <example>
        /// Db.News.Filter(news => news.PublishDate != null)
        /// </example>
        public static IQueryable<TSource> Filter<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
            where TSource : IGenericItem
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return source.Provider.CreateQuery<TSource>(Expression.Call(
                null, GetMethodInfo(Filter, source, predicate),
                new[] { source.Expression, predicate }
            ));
        }

        public static IQueryable<TSource> Filter<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool?>> predicate)
            where TSource : IGenericItem
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return source.Provider.CreateQuery<TSource>(Expression.Call(
                null, GetMethodInfo(Filter, source, predicate),
                new[] { source.Expression, predicate }
            ));
        }

        /// <example>
        /// Db.News.JoinMany(
        ///     news => news.Regions,
        ///     link => link.MarketingRegionLinkedItem
        ///         .Filter(region => new[] { "moskva", "spb" }.Contains(region.Alias)))
        /// </example>
        public static ICollection<TSource> Filter<TSource>(
            this ICollection<TSource> source, Func<TSource, bool> predicate)
            where TSource : IGenericItem
        {
            throw new NotSupportedException();
        }

        public static ICollection<TSource> Filter<TSource>(
            this ICollection<TSource> source, Func<TSource, bool?> predicate)
            where TSource : IGenericItem
        {
            throw new NotSupportedException();
        }

        /// <example>
        /// Db.NewsRubrics.JoinOne(
        ///     rubric => rubric.Group
        ///         .Filter(group => group.Modified > Date.Parse("2010-01-01")))
        /// </example>
        public static TSource Filter<TSource>(
            this TSource source, Func<TSource, BooleanTag> predicate)
            where TSource : IGenericItem
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Utils

        public struct BooleanTag
        {
            public static implicit operator BooleanTag(bool value) => throw new NotSupportedException();

            public static implicit operator BooleanTag(bool? value) => throw new NotSupportedException();
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> func, T1 a1, T2 a2)
        {
            return func.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4>(Func<T1, T2, T3, T4> func, T1 a1, T2 a2, T3 a3)
        {
            return func.Method;
        }

        #endregion
    }
}