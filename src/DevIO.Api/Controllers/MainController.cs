using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DevIO.Api.Controllers;

[ApiController]
public abstract class MainController : ControllerBase
{
    private readonly INotificador _notificador;

    protected MainController(INotificador notificador)
    {
        _notificador = notificador;
    }

    protected ActionResult CustomResponse(ModelStateDictionary modelState)
    {
        if (!modelState.IsValid) NotificarErroModelInvalida(modelState);

        return CustomResponse();
    }

    protected bool OperacaoValida()
    {

        return !_notificador.TemNotificacao();
    }

    protected ActionResult CustomResponse(object result = null)
    {
        if (OperacaoValida())
        {
            return Ok(new
            {
                success = true,
                data = result
            });
        }

        return BadRequest(new
        {
            success = false,
            errors = _notificador.ObterNotificacoes().Select(x => x.Mensagem)
        });
    }

    protected void NotificarErroModelInvalida(ModelStateDictionary modelState)
    {
        var erros = modelState.Values.SelectMany(x => x.Errors);

        foreach (var erro in erros)
        {
            var errorMsg = erro.Exception == null ? erro.ErrorMessage : erro.Exception.Message;
            NotificarErro(errorMsg);
        }
    }

    protected void NotificarErro(string errorMsg)
    {

        _notificador.Handle(new Business.Notificacoes.Notificacao(errorMsg));
    }
}