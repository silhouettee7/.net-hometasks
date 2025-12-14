using Microsoft.EntityFrameworkCore;
using RESTAuth.Domain.Entities;
using RESTAuth.Persistence.DataBase;

namespace RESTAuth.Application.Graph.Types;

public class UserType : ObjectType<User>
{
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor.Field(u => u.Id).Type<NonNullType<UuidType>>();
        descriptor.Field(u => u.Name).Type<NonNullType<StringType>>();
        descriptor.Field(u => u.Email).Type<NonNullType<StringType>>();
        descriptor.Field(u => u.Role).Type<StringType>();
        descriptor.Ignore(u => u.Password);
   
        descriptor.Field("profile")
            .Resolve(async ctx => 
            {
                var user = ctx.Parent<User>();
                var db = ctx.Service<AppDbContext>();
                return await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            })
            .Type<UserProfileType>();

        descriptor.Field("addresses")
            .Resolve(async ctx =>
            {
                var user = ctx.Parent<User>();
                var db = ctx.Service<AppDbContext>();
                return await db.UserAddresses
                    .Where(ua => ua.UserId == user.Id)
                    .ToListAsync();
            })
            .Type<ListType<UserAddressType>>();
    }
}