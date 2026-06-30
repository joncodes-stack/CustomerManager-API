using FluentValidation;

namespace CustomerManager.Api.Filters
{
    public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter where T : class
    {
        private readonly IValidator<T> _validator = validator;

        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            if (context.Arguments.FirstOrDefault(arg => arg is T) is not T model)
            {
                return await next(context);
            }

            var result = await _validator.ValidateAsync(model);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }

            return await next(context);
        }
    }
}
