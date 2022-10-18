using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Search.Generic.Integration.QP.Permissions.Models
{
    /// <summary>
    /// Связь контентов друг с другом.
    /// Если какой-либо контент оказывает влияние на другой, то связь попадает сюда.
    /// </summary>
    public class ContentToContent
    {
        /// <summary>
        /// Id связи, уникальный (PK)
        /// </summary>
        [Key]
        [Column("link_id")]
        public decimal LinkId { get; set; }
        /// <summary>
        /// Контент который связываем
        /// </summary>
        [Column("l_content_id")]
        public decimal LeftContentId { get; set; }
        /// <summary>
        /// Контент на который накидывается связь
        /// </summary>
        [Column("r_content_id")]
        public decimal RightContentId { get; set; }
    }
}
