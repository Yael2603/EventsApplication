using EventsApp.Domain.Entities;

namespace EventsApp.Application.Abstractions;

public interface ITokenGenerator
{
    string Generate(User user);
}
