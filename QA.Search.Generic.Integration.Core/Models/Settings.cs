using QA.Search.Common.Interfaces;
using QA.Search.Generic.Integration.Core.Services;

namespace QA.Search.Generic.Integration.Core.Models
{
    /// <summary>
    /// Settings for single integration (indexing) process
    /// </summary>
    /// <typeparam name="TMarker"></typeparam>
    public class Settings<TMarker>
        where TMarker : IServiceMarker
    {
        /// <summary>
        /// CronTab string for schedule indexing. See https://crontab.guru
        /// </summary>
        public string CronSchedule { get; set; }               
    }
}