using eAgenda.Aplicacao.Modulos.ModuloContato;
using Microsoft.AspNetCore.Mvc;

namespace eAgenda.WebApi.Features.Contatos;

[ApiController]
[Route("/api/contatos")]
public sealed class ContatosController(ServicoContato servicoContato) : ControllerBase
{

    [HttpGet]
    public ActionResult<List<ListarContatosDto>?> SelecionarTodos()
    {
        var listarContatos = servicoContato.SelecionarTodos();

        return StatusCode(200, listarContatos);
    }


    [HttpPost]
    public ActionResult Cadastrar(CadastrarContatoRequest req)
    {
        var dto = new CadastrarContatoDto(
            req.Nome,
            req.Email,
            req.Telefone,
            req.Cargo,
            req.Empresa
            );

        var result = servicoContato.Cadastrar(dto);

        if (result.IsFailed)
        {
            if (result.HasError(e =>
                e.Message.Equals("Já existe um contato com este telefone.") ||
                e.Message.Equals("Já existe um contato com este email.")
            )
            )
            {
                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    detail: result.Errors.First().Message,
                    title: "Conflito",
                    type: "https://developer.mozilla.org/pt-BR/docs/Web/HTTP/Reference/Status/409"
                );
            }

            ValidationProblemDetails problemDetails = new()
            {
                Status = StatusCodes.Status400BadRequest,
                Title = result.Errors.First().Message
            };

            return ValidationProblem(problemDetails);

            // return BadRequest();
        }


        var res = new CadastrarContatoResponse(result.Value);

        return CreatedAtAction(nameof(SelecionarPorId), new { id = result.Value }, res);
    }


    [HttpGet("{id:guid}")]
    public ActionResult<List<ListarContatosDto>?> SelecionarPorId(Guid id)
    {
        var result = servicoContato.SelecionarPorId(id);

        if (result.IsFailed)
            return NotFound(id);

        var dto = result.Value;

        return Ok(dto);
    }


    [HttpPut("{id:guid}")]
    public ActionResult<DetalhesContatoDto> Editar(Guid id, EditarContatoRequest req)
    {
        var dto = new EditarContatoDto(
            id,
            req.Nome,
            req.Email,
            req.Telefone,
            req.Cargo,
            req.Empresa
        );

        var result = servicoContato.Editar(dto);

        if (result.IsFailed)
            return NotFound(id);

        return CreatedAtAction(nameof(SelecionarPorId), new { id }, req);
    }


    [HttpDelete("{id:guid}")]
    public ActionResult Excluir(Guid id)
    {
        var result = servicoContato.Excluir(id);

        if (result.IsFailed)
            return NotFound(id);

        return NoContent();
    }
}
