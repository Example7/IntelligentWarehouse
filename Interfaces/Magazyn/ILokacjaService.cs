using Data.Data.Magazyn;
using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces.Magazyn
{
    public interface ILokacjaService
    {
        Task<IList<Lokacja>> GetLokacje();
    }
}
