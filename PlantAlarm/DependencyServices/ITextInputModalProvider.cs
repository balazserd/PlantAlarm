using System;
using System.Threading.Tasks;

namespace PlantAlarm.DependencyServices
{
    public interface ITextInputModalProvider
    {
        Task<string> ShowTextModalAsync();
    }
}
