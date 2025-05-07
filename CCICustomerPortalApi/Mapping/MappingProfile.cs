using AutoMapper;
using CCICustomerPortalApi.Models;
using CCICustomerPortalApi.Models.DTOs;
using Microsoft.AspNetCore.Identity;

namespace CCICustomerPortalApi.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore());

        CreateMap<Workspace, WorkspaceDto>();

        CreateMap<Shop, ShopDto>()
            .ForMember(dest => dest.ProgramNames, opt => opt
                .MapFrom(src => src.ShopPrograms
                    .Select(sp => sp.Program.Name)
                    .ToList()));

        CreateMap<Report, ReportDto>()
            .ForMember(dest => dest.CategoryName, opt => opt
                .MapFrom(src => src.Category.Name));

        CreateMap<ShopKpi, ShopKpiDto>();

        // DTO to Entity mappings for create/update operations
        CreateMap<WorkspaceDto, Workspace>()
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.UserWorkspaces, opt => opt.Ignore())
            .ForMember(dest => dest.Reports, opt => opt.Ignore());

        CreateMap<ShopDto, Shop>()
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.ShopPrograms, opt => opt.Ignore())
            .ForMember(dest => dest.ShopUsers, opt => opt.Ignore())
            .ForMember(dest => dest.ShopKpis, opt => opt.Ignore());

        CreateMap<ReportDto, Report>()
            .ForMember(dest => dest.Category, opt => opt.Ignore());
    }
}