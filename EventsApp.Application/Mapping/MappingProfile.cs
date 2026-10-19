using AutoMapper;
using EventsApp.Application.DTOs.Auth;
using EventsApp.Domain.Entities;

namespace EventsApp.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RegisterRequestDto, User>()
            .ForMember(u => u.PasswordHash, o => o.Ignore())
            .ForMember(u => u.Role, o => o.Ignore());

        CreateMap<RegisterProviderRequestDto, User>()
            .ForMember(u => u.PasswordHash, o => o.Ignore())
            .ForMember(u => u.Role, o => o.Ignore());

        CreateMap<RegisterProviderRequestDto, Provider>()
            .ForMember(p => p.Description, o => o.MapFrom(d => d.Description ?? string.Empty))
            .ForMember(p => p.Status, o => o.Ignore())
            .ForMember(p => p.AcceptedTermsAt, o => o.Ignore());

        CreateMap<User, AuthResponseDto>()
            .ForMember(d => d.UserId, o => o.MapFrom(u => u.Id))
            .ForMember(d => d.Token, o => o.Ignore());
    }
}
