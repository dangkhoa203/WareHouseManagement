﻿using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WareHouseManagement.Data;
using WareHouseManagement.Endpoint;
using WareHouseManagement.Model.Entity.Customer_Entity;
using WareHouseManagement.Model.Entity.Vendor_Entity;

namespace WareHouseManagement.Feature.VendorGroups
{
    public class AddVendorGroup:IEndpoint
    {
        public record Request(string name, string description);
        public record Response(bool success, string errorMessage, ValidationResult? error);
        public sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(r => r.name).NotEmpty().WithMessage("Chưa nhập tên");
            }
        }
        public static void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Vendor-Groups", Handler).WithTags("VendorGroups");
        }
        private static async Task<IResult> Handler(Request request, ApplicationDbContext context, ClaimsPrincipal user)
        {
            var service = context.Users.Include(u => u.ServiceRegistered).Where(u => u.UserName == user.Identity.Name).Select(u => u.ServiceRegistered).FirstOrDefault();
            var validator = new Validator();
            var validatedresult = validator.Validate(request);
            if (!validatedresult.IsValid)
            {
                return Results.BadRequest(new Response(false, "", validatedresult));
            }
            VendorGroup Group = new()
            {
                Name = request.name,
                Description = request.description,
                ServiceRegisteredFrom = service,
            };
            await context.VendorGroups.AddAsync(Group);
            if (await context.SaveChangesAsync() > 0)
            {
                return Results.Ok(new Response(true, "", validatedresult));
            }
            return Results.BadRequest(new Response(false, "Lỗi xảy ra khi đang thực hiện!", validatedresult));
        }
    }
}
