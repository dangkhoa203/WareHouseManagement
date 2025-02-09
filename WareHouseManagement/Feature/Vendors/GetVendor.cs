﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WareHouseManagement.Data;
using WareHouseManagement.Endpoint;

namespace WareHouseManagement.Feature.Vendors {
    public class GetVendor : IEndpoint {
        public record groupDTO(string id, string name, string description);
        public record vendorDTO(string id, string name, string email, string address, string phoneNumber, groupDTO? group, DateTime createDate);
        public record Response(bool success, vendorDTO data, string errorMessage);

        public static void MapEndpoint(IEndpointRouteBuilder app) {
            app.MapGet("/api/Vendors/{id}", Handler).WithTags("Vendors");
        }
        private static async Task<IResult> Handler([FromRoute] string id, ApplicationDbContext context, ClaimsPrincipal user) {
            try {
                var service = context.Users
                    .Include(u => u.ServiceRegistered)
                    .Where(u => u.UserName == user.Identity.Name)
                    .Select(u => u.ServiceRegistered)
                    .FirstOrDefault();
                var vendor = await context.Vendors
                    .Include(c => c.VendorGroup)
                    .Where(c => c.ServiceRegisteredFrom.Id == service.Id)
                    .Select(c => new vendorDTO(

                        c.Id,
                        c.Name,
                        c.Email,
                        c.Address,
                        c.PhoneNumber,
                        c.VendorGroup != null ?
                        new groupDTO(c.VendorGroup.Id, c.VendorGroup.Name, c.VendorGroup.Description) : null,
                        c.CreatedDate

                     ))
                    .FirstOrDefaultAsync(c => c.id == id);
                if (vendor != null)
                    return Results.Ok(new Response(true, vendor, ""));
                return Results.NotFound(new Response(false, null, "Không tìm thấy dữ liệu!"));
            } catch (Exception ex) {
                return Results.BadRequest(new Response(false, null, "Lỗi đã xảy ra!"));
            }
        }
    }
}
