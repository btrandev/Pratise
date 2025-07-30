using AdminService.Domain.Entities;
using AdminService.Features.Tenants.GetById;
using AdminService.Features.Users.GetById;
using AutoMapper;

namespace AdminService.Common.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserResponse>();
        
        // Tenant mappings
        CreateMap<Tenant, TenantResponse>();
    }
}
