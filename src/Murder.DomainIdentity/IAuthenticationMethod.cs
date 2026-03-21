namespace Murder.DomainIdentity;

internal interface IAuthenticationMethod
{
    Type MethodKeyType { get; }
    IdentityId? Authenticate(object credential);
    object Enroll(object enrollmentData);
}

public interface IAuthenticationMethod<in TCredential, in TEnrollmentData, TMethod>
    where TCredential : IIncomingCredential<TMethod>
    where TEnrollmentData : IEnrollmentData<TMethod>
    where TMethod : IAuthenticationMethodKey
{
    IdentityId? Authenticate(TCredential credential);
    EnrollmentResult<TMethod> Enroll(TEnrollmentData enrollmentData);
}
