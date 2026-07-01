using CustomerManager.Api.Filters;
using CustomerManager.Application.Commands;
using CustomerManager.Application.Handlers;
using CustomerManager.Application.Interfaces;
using CustomerManager.Application.Queries;
using CustomerManager.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManager.Api.Endpoints
{
    public static class CustomerManagerEndpoints
    {
        public static void MapCustomerManagerEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/clientes").WithTags("Clientes");

            group.MapPost("/", CreateCustomer)
                .AddEndpointFilter<ValidationFilter<CreateCustomerCommand>>()
                .WithSummary("Cria um novo cliente")
                .WithDescription("Apenas administradores podem cadastrar clientes.")
                .Produces<CreateCustomerResponse>(StatusCodes.Status201Created)
                .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);

            group.MapGet("/", GetAllCustomers)
                .AddEndpointFilter<ValidationFilter<GetAllCustomerQuery>>()
                .WithSummary("Lista todos os clientes ativos com paginação")
                .Produces<PagedResponse<GetAllCustomerResponse>>(StatusCodes.Status200OK)
                .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);

            group.MapGet("/{id:Guid}", GetCustomerById)
                .WithSummary("Busca cliente por ID")
                .Produces<GetCustomerByIdResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            group.MapGet("/cpf/{cpf}", GetCustomerByCpf)
                .WithSummary("Busca cliente por CPF")
                .Produces<GetCustomerByCpfResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            group.MapPut("/{id:Guid}", UpdateCustomer)
                .AddEndpointFilter<ValidationFilter<UpdateCustomerCommand>>()
                .WithSummary("Atualiza um cliente")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            group.MapDelete("/{id:Guid}", DeleteCustomer)
                .WithSummary("Inativa um cliente")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);
        }


        private static async Task<IResult> CreateCustomer(
        CreateCustomerCommand command, [FromServices] ICreateCustomerHandler handler)
        {
            var result = await handler.Handle(command);
            return Results.Created($"/clientes/{result.Id}", result);
        }

        private static async Task<IResult> GetAllCustomers(
            [AsParameters] GetAllCustomerQuery query, [FromServices] IGetAllCustomerHandler handler)
        {
            var result = await handler.Handle(query);
            return Results.Ok(result);
        }

        private static async Task<IResult> GetCustomerById(
            Guid id, [FromServices] IGetCustomerByIdHandler handler)
        {
            var result = await handler.Handle(new GetCustomerByIdQuery(id));
            return result is null ? Results.NotFound() : Results.Ok(result);
        }

        private static async Task<IResult> GetCustomerByCpf(
            string cpf, [FromServices] IGetCustomerByCPFHandler handler)
        {
            var result = await handler.Handle(new GetCustomerByCpfQuery(cpf));
            return Results.Ok(result);
        }

        private static async Task<IResult> UpdateCustomer(
            Guid id, UpdateCustomerCommand command, [FromServices] IUpdateCustomerHandler handler)
        {
            await handler.Handle(command with { Id = id });
            return Results.NoContent();
        }

        private static async Task<IResult> DeleteCustomer(
            Guid id, [FromServices] IDeleteCustomerHandler handler)
        {
            await handler.Handle(new DeleteCustomerCommand(id));
            return Results.NoContent();
        }
    }
}
