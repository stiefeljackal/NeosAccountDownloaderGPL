using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountDownloaderLibrary.Interfaces
{
    public interface IAppConfigLoader
    {
        public IAccountDownloadUserConfigProfile LoadAccountDownloadConfigProfile(string userId);

        public void SaveAccountDownloadConfigProfile(string userId, IAccountDownloadUserConfigProfile config);
    }
}
