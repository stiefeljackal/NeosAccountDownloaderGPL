using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountDownloaderLibrary.Interfaces
{
    public interface IAppConfigLoader
    {
        public IAccountDownloadUserConfig LoadAccountDownloadConfig(string userId);

        public void SaveAccountDownloadConfig(string userId, IAccountDownloadUserConfig config);
    }
}
