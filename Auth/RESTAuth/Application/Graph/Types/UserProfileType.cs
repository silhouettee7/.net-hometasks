using RESTAuth.Domain.Entities;

namespace RESTAuth.Application.Graph.Types;

public class UserProfileType : ObjectType<UserProfile>
{
    protected override void Configure(IObjectTypeDescriptor<UserProfile> descriptor)
    {
        descriptor.Field(p => p.Id).Type<NonNullType<IntType>>();
        descriptor.Field(p => p.FirstName).Type<NonNullType<StringType>>();
        descriptor.Field(p => p.LastName).Type<NonNullType<StringType>>();
        descriptor.Field(p => p.BirthDate).Type<DateTimeType>();
        descriptor.Field(p => p.Phone).Type<StringType>();
        
        descriptor.Field("fullName")
            .Type<StringType>()
            .Resolve(ctx => 
            {
                var profile = ctx.Parent<UserProfile>();
                return $"{profile.FirstName} {profile.LastName}";
            });
            
        descriptor.Field("age")
            .Type<IntType>()
            .Resolve(ctx => 
            {
                var profile = ctx.Parent<UserProfile>();
                var today = DateTime.Today;
                var age = today.Year - profile.BirthDate.Year;
                if (profile.BirthDate.Date > today.AddYears(-age)) age--;
                return age;
            });
    }
}