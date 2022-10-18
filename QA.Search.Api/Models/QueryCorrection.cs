using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace QA.Search.Api.Models
{
    /// <summary>
    /// Результат исправления поисковой строки пользователя
    /// </summary>
    public class QueryCorrection
    {
        /// <summary>
        /// Мера релевантности исправления в рамках одного индекса
        /// </summary>
        [JsonIgnore]
        public float Score { get; set; }

        /// <summary>
        /// Исправленная поисковая строка в текстовом виде
        /// </summary>
        [Required]
        public string Text { get; set; }

        /// <summary>
        /// Исправленная поисковая строка с HTML-выделением исправленных фраз
        /// </summary>
        [Required]
        public string Snippet { get; set; }

        /// <summary>
        /// Было ли применено исправление при поиске результатов
        /// </summary>
        public bool ResultsAreCorrected { get; set; }
    }
}