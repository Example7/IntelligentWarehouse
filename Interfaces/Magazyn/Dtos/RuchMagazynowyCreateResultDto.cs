namespace Interfaces.Magazyn.Dtos
{
    public class RuchMagazynowyCreateResultDto
    {
        public bool Success { get; set; }
        public IList<RuchMagazynowyCreateErrorDto> Errors { get; set; } = new List<RuchMagazynowyCreateErrorDto>();
    }

    public class RuchMagazynowyCreateErrorDto
    {
        public string Key { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
