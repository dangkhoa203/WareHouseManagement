﻿using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WareHouseManagement.Data;
using WareHouseManagement.Endpoint;
using WareHouseManagement.Model.Entity.Customer_Entity;

namespace WareHouseManagement.Feature.CustomerGroups
{
    public class UpdateCustomerGroup : IEndpoint
    {
        public record Request(string id, string name, string description);
        public record Response(bool success, string errorMessage, ValidationResult? error);
        public sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(r => r.name).NotEmpty().WithMessage("Chưa nhập tên");
            }
            public bool checkSame(Request request, CustomerGroup group) {
               return (request.name==group.Name && request.description==group.Description);
            }
        }
        public static void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/Customer-Groups", Handler).WithTags("CustomerGroups");
        }
        private static async Task<IResult> Handler(
            Request request,
            ApplicationDbContext context,
            ClaimsPrincipal user)
        {
            var validator = new Validator();
            var validatedresult = validator.Validate(request);
            if (!validatedresult.IsValid)
                return Results.BadRequest(new Response(false, "", validatedresult));
            
            var service = context.Users
                .Include(u => u.ServiceRegistered)
                .Where(u => u.UserName == user.Identity.Name)
                .Select(u => u.ServiceRegistered)
                .FirstOrDefault();

            var group = await context.CustomerGroups
                .Where(g => g.ServiceRegisteredFrom.Id == service.Id)
                .Include(g=>g.Customers)
                .FirstOrDefaultAsync(g => g.Id == request.id);
            if (group == null)
                return Results.NotFound(new Response(false, "Lỗi xảy ra khi đang thực hiện!", validatedresult));
            
            if(!validator.checkSame(request, group))
            {
                group.Name = request.name;
                group.Description = request.description;
                if (await context.SaveChangesAsync() < 1)
                {
                    return Results.BadRequest(new Response(false, "Lỗi xảy ra khi đang thực hiện!", validatedresult));
                }
            }
            return Results.Ok(new Response(true, "", validatedresult));
        }
    }
}
