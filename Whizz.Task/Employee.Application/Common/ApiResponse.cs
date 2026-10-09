using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Application.Common
{
    public class ApiResponse<T>
    {
        public T? Data { get; set; }
        public PaginationMetadata? Pagination { get; set; }
        public string? Error { get; set; }
        public ApiResponse() { }
        public ApiResponse(T? data, PaginationMetadata? pagination = null, string? error = null)
        {
            Data = data;
            Pagination = pagination;
            Error = error;
        }
        public static ApiResponse<T> Success(T data, PaginationMetadata? pagination = null)
        {
            return new ApiResponse<T>(data, pagination, null);
        }
        public static ApiResponse<T> Failure(string errorMessage)
        {
            return new ApiResponse<T>(default, null, errorMessage);
        }
    }
}
