﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WareHouseManagement.Data;
using WareHouseManagement.Endpoint;

namespace WareHouseManagement.Feature.CustomerGroups
{
    public class RemoveCustomerGroup : IEndpoint
    {
        public record Request(string id);
        public record Response(bool success, string errorMessage);
        public static void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/Customer-Groups/", Handler).WithTags("CustomerGroups");
        }
        private static async Task<IResult> Handler([FromBody]Request request, ApplicationDbContext context, ClaimsPrincipal user)
        {
            var service = context.Users
                .Include(u => u.ServiceRegistered)
                .Where(u => u.UserName == user.Identity.Name)
                .Select(u => u.ServiceRegistered)
                .FirstOrDefault();
            var group = await context.CustomerGroups
                .Where(g=>g.ServiceRegisteredFrom.Id==service.Id)
                .FirstOrDefaultAsync(g => g.Id == request.id);
            if (group != null)
            {
                context.CustomerGroups.Remove(group);
                var result = await context.SaveChangesAsync();
                if (result > 0)
                    return Results.Ok(new Response(true, ""));
                return Results.BadRequest(new Response(false, "Lỗi đã xảy ra!"));
            }
            return Results.NotFound(new Response(false, "Không tìm thấy nhóm!"));
        }
    }
}
