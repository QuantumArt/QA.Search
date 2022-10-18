using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Search.Generic.Integration.QP.Permissions.Models
{
    /// <summary>
    /// Связь многие к многим для содержимого контентов.
    /// </summary>
    public class ItemToItem
    {
        /// <summary>
        /// Уникальный идентификатор связи контент-контент.
        /// </summary>
        [Column("link_id")]
        public decimal LinkId { get; set; }
        /// <summary>
        /// Строк из связующего контента
        /// </summary>
        [Column("l_item_id")]
        public decimal LeftItemId { get; set; }
        /// <summary>
        /// Строка из контента на который идёт связь
        /// </summary>
        [Column("r_item_id")]
        public decimal RightItemId { get; set; }
        /// <summary>
        /// Флаг реверсивности.
        /// Все связи делаются в обе стороны, поэтому стоит выбирать что-то одно.
        /// </summary>
        [Column("is_rev")]
        public bool Reversed { get; set; }
    }
}
