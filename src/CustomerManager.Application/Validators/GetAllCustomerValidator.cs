using CustomerManager.Application.Queries;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerManager.Application.Validators
{
    public class GetAllCustomerValidator : AbstractValidator<GetAllCustomerQuery>
    {
        public GetAllCustomerValidator()
        {
            RuleFor(x => x.Pagina)
            .GreaterThanOrEqualTo(1)
            .WithMessage("A página deve ser maior ou igual a 1.");

            RuleFor(x => x.TamanhoPagina)
                .GreaterThan(0)
                .WithMessage("O tamanho da página deve ser maior que 0.")
                .LessThanOrEqualTo(100)
                .WithMessage("O tamanho máximo da página permitido é 100.");
        }
    }
}
