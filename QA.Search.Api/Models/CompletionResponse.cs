using System.ComponentModel.DataAnnotations;
using System.Net;

namespace QA.Search.Api.Models
{
    /// <summary>
    /// Результат дополнения строки поискового ввода
    /// </summary>
    public class CompletionResponse
    {
        /// <summary>
        /// Копия HTTP статус-кода в теле ответа
        /// </summary>
        public int Status { get; set; } = (int)HttpStatusCode.OK;

        /// <summary>
        /// Предлагаемые поисковые фразы
        /// </summary>
        [Required]
        public string[] Phrases { get; set; }
    }
}