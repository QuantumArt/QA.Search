using QA.Search.Generic.DAL.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Search.Generic.Integration.QP.Permissions.Models
{
    /// <summary>
    /// Кастомные поля для контента пользовательских ролей
    /// </summary>
    public class UserRole : GenericItem
    {
        /// <summary>
        /// Человеческое наименование роли (может быть на русском)
        /// </summary>
        [Column("title")]
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Программистское наименование
        /// </summary>
        [Column("alias")]
        public string Alias { get; set; } = string.Empty;
    }
}
