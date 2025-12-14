using RESTAuth.Domain.Entities;

namespace RESTAuth.Application.Graph.Types;

public class UserAddressType : ObjectType<UserAddress>
{
    protected override void Configure(IObjectTypeDescriptor<UserAddress> descriptor)
    {
        descriptor.Field(a => a.Id).Type<NonNullType<IntType>>();
        descriptor.Field(a => a.Country).Type<NonNullType<StringType>>();
        descriptor.Field(a => a.City).Type<NonNullType<StringType>>();
        descriptor.Field(a => a.Street).Type<NonNullType<StringType>>();
        
        descriptor.Field("formattedAddress")
            .Type<StringType>()
            .Resolve(ctx => 
            {
                var address = ctx.Parent<UserAddress>();
                return $"{address.Street}, {address.City}, {address.Country}";
            });
    }
}