using Interfaces.Magazyn.Dtos;

namespace IntranetWeb.Models.Klient;

public class KlientDetailsPageViewModel
{
    public KlientDetailsDto Details { get; set; } = null!;
    public IList<string> RoleKontaMobilnego { get; set; } = new List<string>();
    public bool CzyKontoMobilneMaRoleKlienta { get; set; }
    public bool CzyKontoMobilneMaRolePracownicze { get; set; }
}
