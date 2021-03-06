﻿using System;
using System.Net;
using System.Reflection;
using FluentValidation;
using CoreDocker.Shared.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using CoreDocker.Api.WebApi.Exceptions;
using CoreDocker.Shared.Models.Shared;
using log4net;
using CoreDocker.Utilities.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace CoreDocker.Api.WebApi.Filters
{
    public class CaptureExceptionFilter : ExceptionFilterAttribute
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Overrides of ExceptionFilterAttribute

        public override Task OnExceptionAsync(ExceptionContext context)
        {
            Exception exception = context.Exception.ToFirstExceptionOfException();

            var apiException = exception as ApiException;
            if (apiException != null)
            {
                RespondWithTheExceptionMessage(context, apiException);
            }
            else if (IsSomeSortOfValidationError(exception))
            {
                RespondWithBadRequest(context, exception);
            }
            else if (exception is ValidationException)
            {
                RespondWithValidationRequest(context, exception as ValidationException);
            }
            else
            {
                RespondWithInternalServerException(context, exception);
            }
            return base.OnExceptionAsync(context);
        }

        #endregion
        
        #region Private Methods

        private void RespondWithTheExceptionMessage(ExceptionContext context, ApiException exception)
        {
            var errorMessage = new ErrorMessage(exception.Message);
            context.Result = CreateResponse(exception.HttpStatusCode, errorMessage);
        }

        private void RespondWithBadRequest(ExceptionContext context, Exception exception)
        {
            var errorMessage = new ErrorMessage(exception.Message);
            context.Result = CreateResponse(HttpStatusCode.BadRequest, errorMessage);
        }

        public bool IsSomeSortOfValidationError(Exception exception)
        {
            return exception is System.ComponentModel.DataAnnotations.ValidationException ||
                   exception is ArgumentException;
        }

        private void RespondWithValidationRequest(ExceptionContext context,
            ValidationException validationException)
        {
            var errorMessage =new ErrorMessage(validationException.Errors.Select(x => x.ErrorMessage).FirstOrDefault());
            context.Result = CreateResponse(HttpStatusCode.BadRequest, errorMessage);
        }

        private void RespondWithInternalServerException(ExceptionContext context, Exception exception)
        {
            const HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError;
            var errorMessage =
                new ErrorMessage("An internal system error has occurred. The developers have been notified.");
            _log.Error(exception.Message, exception);
#if DEBUG
            errorMessage.AdditionalDetail = exception.Message;
#endif
            context.Result = CreateResponse(httpStatusCode, errorMessage);
        }

        private IActionResult CreateResponse(HttpStatusCode httpStatusCode, object errorMessage)
        {   
            return new ObjectResult(errorMessage) {StatusCode = (int) httpStatusCode};
        }

        #endregion
    }
}