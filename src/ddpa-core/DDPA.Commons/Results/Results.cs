using DDPA.Commons.Enums;
using System.Collections.Generic;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.Commons.Results
{
    public class Result
    {
        public string Id { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool IsRedirect { get; set; }
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Default value is ErrorCode.DEFAULT
        /// </summary>
        public ErrorCode ErrorCode { get; set; }

        public Result()
        {
            Success = false;
            Message = "";
            ErrorCode = ErrorCode.DEFAULT;
            Id = "";
            IsRedirect = false;
            RedirectUrl = "";
        }
    }

    public class OneResult<TEntity> : Result where TEntity : class, new()
    {
        public TEntity Entity { get; set; }
    }

    public class ManyResult<TEntity> : Result where TEntity : class, new()
    {
        public IEnumerable<TEntity> Entities { get; set; }
        public long TotalFilteredEntities { get; set; }
        public long TotalEntities { get; set; }
    }
}