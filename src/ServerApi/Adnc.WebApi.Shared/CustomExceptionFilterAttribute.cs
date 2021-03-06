﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Adnc.Infr.Common;
using Adnc.Application.Shared;

namespace Microsoft.AspNetCore.Mvc.Filters
{
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<CustomExceptionFilterAttribute> _logger;

        public CustomExceptionFilterAttribute(ILogger<CustomExceptionFilterAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            Exception exception = context.Exception;
            JsonResult result = null;
            if (exception is BusinessException)
            {
                result = new JsonResult(exception.Message)
                {
                    StatusCode = exception.HResult
                };
            }
            else
            {
                result = new JsonResult(new ErrorModel(ErrorCode.InternalServerError, "服务器异常"))
                {
                    StatusCode = 500
                };

                var userContext = context.HttpContext.RequestServices.GetService<UserContext>();

                var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
                string className = descriptor.ControllerName;
                string method = descriptor.ActionName;
                string requestUrl = context.HttpContext.Request.Path;
                long userId = userContext.ID;
                //var parms = ex.Data?.ToDictionary().Select(k => k.Key + "=" + k.Value).Join() ?? "";

                _logger.LogError(exception, exception.Message);
                //Agent.Tracer.CurrentTransaction.CaptureException(exception);
            }

            context.Result = result;
            context.ExceptionHandled = true;
        }

        public override Task OnExceptionAsync(ExceptionContext context)
        {
            OnException(context);
            return Task.CompletedTask;
        }
    }
}