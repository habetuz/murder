using Murder.DomainIdentity;

namespace Murder.ApplicationIdentity;

public class IdentityService(IIdentityRepository identityRepository)
{
    private readonly IIdentityRepository _identityRepository = identityRepository;

    public Identity GetIdentity(IdentityId identityId)
    {
        return _identityRepository.IdentityById(identityId);
    }

    public bool IsUser(IdentityId identity)
    {
        var identityData = _identityRepository.IdentityById(identity);
        return identityData is User;
    }

    public bool IsGuest(IdentityId identity)
    {
        var identityData = _identityRepository.IdentityById(identity);
        return identityData is Guest;
    }

    public IdentityId CreateUser(string name)
    {
        try
        {
            _identityRepository.IdentityOfName(name);
            throw new DuplicateUsernameException(name);
        }
        catch (KeyNotFoundException)
        {
            // No existing user with this name — proceed
        }

        var user = _identityRepository.IdentityFactory.CreateUser(name);
        _identityRepository.Store(user);
        return user.Id;
    }

    public IdentityId CreateGuest()
    {
        var guest = _identityRepository.IdentityFactory.CreateGuest();
        _identityRepository.Store(guest);
        return guest.Id;
    }

    public void DeactivateUser(IdentityId identity)
    {
        var user =
            (User)_identityRepository.IdentityById(identity)
            ?? throw new UserGuestMismatchException(true);
        user.Deactivate();
        _identityRepository.Update(user);
    }
}
