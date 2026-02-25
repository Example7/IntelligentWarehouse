namespace Interfaces.CMS.Dtos
{
    public class SzablonWydrukuIndexDto
    {
        public string SearchTerm { get; set; } = string.Empty;

        public IList<SzablonWydrukuIndexItemDto> Items { get; set; } = new List<SzablonWydrukuIndexItemDto>();
    }
}
