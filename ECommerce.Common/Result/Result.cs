using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ECommerce.Common.Result
{
    /// <summary>
    /// Result Pattern - General Response Wrapper (Generic)
    /// </summary>
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        [JsonIgnore]
        public bool IsFailure => !IsSuccess;

        public Result() { }

        // Success Factory Methods
        public static Result<T> Success(T? data, string message = "Operation completed successfully")
        {
            return new Result<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        public static Result<T> Success(string message = "Operation completed successfully")
        {
            return new Result<T>
            {
                IsSuccess = true,
                Message = message
            };
        }

        // Failure Factory Methods
        public static Result<T> Failure(string message, List<string>? errors = null)
        {
            return new Result<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }

    /// <summary>
    /// Result Pattern - General Response Wrapper (Non-generic)
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();

        [JsonIgnore]
        public bool IsFailure => !IsSuccess;

        public Result() { }

        // Success Factory Methods
        public static Result Success(string message = "Operation completed successfully")
        {
            return new Result
            {
                IsSuccess = true,
                Message = message
            };
        }

        // Failure Factory Methods
        public static Result Failure(string message, List<string>? errors = null)
        {
            return new Result
            {
                IsSuccess = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }

    /// <summary>
    /// Paginated Result wrapper for lists
    /// </summary>
    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }
}
