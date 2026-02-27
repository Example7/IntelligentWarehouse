namespace Data.Auditing
{
    public interface IAuditActorProvider
    {
        int? GetCurrentUserId();
    }
}
