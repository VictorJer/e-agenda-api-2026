using eAgenda.Aplicacao.Compartilhado;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace eAgenda.WebApi.Compartilhado;

public static class ResultExtensions
{
    public static ActionResult ValidationProblem(this ControllerBase controller, ResultBase result)
    {

        var tipoErro = (TipoErro)result.Errors.First().Metadata[nameof(TipoErro)];

        if (tipoErro == TipoErro.NaoEncontrado)
        {
            return controller.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: result.Errors.First().Message,
                    title: "Recurso não encontrado",
                    type: "https://developer.mozilla.org/pt-BR/docs/Web/HTTP/Reference/Status/404"
            );
        }

        if (tipoErro == TipoErro.Conflito)
        {
            if (result.HasError(e =>
                e.Message.Equals("Já existe um contato com este telefone.") ||
                e.Message.Equals("Já existe um contato com este email.")
            )
            )
            {
                return controller.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    detail: result.Errors.First().Message,
                    title: "Conflito",
                    type: "https://developer.mozilla.org/pt-BR/docs/Web/HTTP/Reference/Status/409"
                );
            }
        }

        if (tipoErro.Equals(TipoErro.Validacao))
        {
            var modelSatate = new ModelStateDictionary();

            foreach (var erro in result.Errors)
            {
                var campo = erro.Metadata["Campo"].ToString()!;

                modelSatate.AddModelError(campo, erro.Message);
            }

            ValidationProblemDetails problemDetail = new(modelSatate)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Requisição Invalida"
            };

            return controller.StatusCode(StatusCodes.Status400BadRequest, problemDetail);
        }

        // if (tipoErro.Equals(TipoErro.Validacao))
        // {
        //     var modelState = new ModelStateDictionary();

        //     foreach (var erro in result.Errors)
        //     {
        //         var campo = erro.Metadata["Campo"].ToString()!;

        //         modelState.AddModelError(campo, erro.Message);
        //     }

        //     ValidationProblemDetails problemDetails = new(modelState)
        //     {
        //         Status = StatusCodes.Status400BadRequest,
        //         Title = "Requisição Inválida",
        //         Type = ProblemDetailsTypes.BadRequest
        //     };

        //     AdicionarTraceId(problemDetails, controller);

        //     return controller.ValidationProblem(problemDetails);
        // }


        // Erros de cadastro
        var modelState = new ModelStateDictionary();

        foreach (var erro in result.Errors)
        {
            var Campo = erro.Metadata["campo"];

            modelState.AddModelError(Campo.ToString()!, erro.Message);
        }

        ValidationProblemDetails problemDetails = new(modelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = result.Errors.First().Message
        };


        return controller.StatusCode(StatusCodes.Status400BadRequest, problemDetails);
    }
}
