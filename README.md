# Stiefel's Version of Neos Account Downloader

**This is not the same repository of the Neos Account Downloader devloped by GuVAnj8Gv3RJ. This is a forked repository that includes additional features missing from the original tool that are in high demand by Neos users and features that GuVA is still implementing. If you prefer to download the original Neos Account Downloader, you may visit GuVA's repository at https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader. All FAQs from GuVA's README will be listed here in addtion to ones that I will list.**

A small tool/utility to download your Neos Account contents to your local computer.

# Disclaimer
While every effort is made to download everything from your account, this utility may miss or lose some data. As such we're unable to offer any guarantee or warranty on this application's ability. This is in line with the License but this additional disclaimer is here in the hopes of transparency.

Please refer to the [License](LICENSE.md) file for additional commentary.

# Download

Windows:
[Neos Account Downloader for Windows (v2023.9.1)](https://github.com/stiefeljackal/NeosAccountDownloaderGPL/releases/download/v2023.9.1/AccountDownloaderWindows-v2023.9.1.zip)

Linux:
[Neos Account Downloader for Linux (v2023.9.1)](https://github.com/stiefeljackal/NeosAccountDownloaderGPL/releases/download/v2023.9.1/AccountDownloaderLinux-v2023.9.1.tar.gz)

# Additional Features

Based on review of the original repository, this tool contains changes from [1.10.2](https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/releases/tag/v1.10.2) that was released by GuVA. However, the following major changes have not been added:

- File Extensions - Stiefel's version of implementing file extensions is different from GuVA's. It is also another reason why this repository did not incorporate GuVA's changes from versions 1.8.0 and 1.10.1 until this point due to the different logic to determine file extensions.

All third party libraries are also up-to-date at the time of writing. In addition, the following changes are in place:

- Featured world records are being gathered.
- BMPCUBE file types are accounted for.
- Record information and assets are being downloaded at the same time as the tool continues to search for records.
- Group-only record selections will show the **Inventory & World** and **Assets** progress.
- Additional asset metadata is saved. This includes the asset's MIME type, Neos location, and file size.
- Persistent user selections (will be changing to be folder specific, but keeping it here for now).
  - **Known Issue**: If the download button is still disabled, you may need to check/uncheck an option to enable it. I am looking into it.

# FAQ (from GuVAnj8Gv3RJ)

## How do I download this?
Follow the instructions above.

## Why does this exist?
Backing up Neos content given the current circumstances seemed wise.

## Can I restore this download into my Neos Account?
No.

## What can I do with the downloaded files?
The files are mostly machine readable collections of entities from your account, feel free to poke around.

You could however, write additional tools that do stuff with them.

## Can I import downloaded content into Unity?
This is not a supported use case of this utility. No effort will be made to support this. You could make your own tooling to do that though.

## Should I use a new folder for each user I download?
Ideally no, the local store that this app builds will in some cases handle duplicate assets in a way that will reduce total file size if you use the same folder for multiple accounts.

## Can I run this app for multiple users at the same time?
Yes, but if you do this, you'll need to use two separate folders which we do not recommend. You may also breach some rate limits Neos has in place on its cloud infrastructure.

## Do subsequent downloads, re-download assets?
For assets, we skip downloading them if an existing asset is found. This makes many downloads incremental rather than starting from scratch.

## Why is assets  showing as 0/XYZ?
For assets, we skip downloading them if an existing asset is found. If your progress statistics or report etc. show 0/xyz etc then it means that no new assets were found.

## What's the difference between Assets and Records/Items/Worlds/Avatars?
This diagram might help:
![](docs/AssetsVsNonAssets.png)

- Assets: Anything that makes up an element in Neos that is not the structure of it within the inspector. So Image,Sounds,Videos,Model Files. These are downloaded incrementally
- Records: Records contain a manifest of all assets that are required to represent an item or world. These are downloaded each time.
- Everything Else: JSON Soup. Just JSON Files of various types. Contacts, Messages etc. These are downloaded each time.

# Known Issues

## Localization isn't instant
If you switch languages then the currently active page you're on will not update to the new language. 

Localization defaults to your computer's language, so for most people this hopefully should not be a problem, but for now change your language on the Getting Started screen.

## Progress Metrics aren't 100% Accurate
Neos assets and records are stored in a way that makes it difficult for us to estimate the total number of records required for download. Due to this we sometimes discover more that need to be queued for download as we go. Causing numbers to jump around a little bit.

# Contributing
Please feel free to contribute to this repository if you would like. Please keep in mind that any additions made will be under the GPLv2 license. All work that was and will be performed on GuVAnj8Gv3RJ's repository will be licensed under MIT.

# Contributors from GuVAnj8Gv3RJ's Repository

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tbody>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/TheJebForge"><img src="https://avatars.githubusercontent.com/u/12719947?v=4?s=100" width="100px;" alt="TheJebForge"/><br /><sub><b>TheJebForge</b></sub></a><br /><a href="#translation-TheJebForge" title="Translation">🌍</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/orange3134"><img src="https://avatars.githubusercontent.com/u/56525091?v=4?s=100" width="100px;" alt="orange3134"/><br /><sub><b>orange3134</b></sub></a><br /><a href="#translation-orange3134" title="Translation">🌍</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/stiefeljackal"><img src="https://avatars.githubusercontent.com/u/20023996?v=4?s=100" width="100px;" alt="Stiefel Jackal"/><br /><sub><b>Stiefel Jackal</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/commits?author=stiefeljackal" title="Code">💻</a> <a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues?q=author%3Astiefeljackal" title="Bug reports">🐛</a> <a href="#research-stiefeljackal" title="Research">🔬</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/Sharkmare"><img src="https://avatars.githubusercontent.com/u/34294231?v=4?s=100" width="100px;" alt="Sharkmare"/><br /><sub><b>Sharkmare</b></sub></a><br /><a href="#translation-Sharkmare" title="Translation">🌍</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/GuVAnj8Gv3RJ"><img src="https://avatars.githubusercontent.com/u/132167543?v=4?s=100" width="100px;" alt="GuVAnj8Gv3RJ"/><br /><sub><b>GuVAnj8Gv3RJ</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/commits?author=GuVAnj8Gv3RJ" title="Code">💻</a> <a href="#maintenance-GuVAnj8Gv3RJ" title="Maintenance">🚧</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/RileyGuy"><img src="https://avatars.githubusercontent.com/u/9770110?v=4?s=100" width="100px;" alt="Cyro"/><br /><sub><b>Cyro</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/commits?author=RileyGuy" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/Psychpsyo"><img src="https://avatars.githubusercontent.com/u/60073468?v=4?s=100" width="100px;" alt="Psychpsyo"/><br /><sub><b>Psychpsyo</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/commits?author=Psychpsyo" title="Code">💻</a> <a href="#translation-Psychpsyo" title="Translation">🌍</a></td>
    </tr>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/Xlinka"><img src="https://avatars.githubusercontent.com/u/22996716?v=4?s=100" width="100px;" alt="xLinka"/><br /><sub><b>xLinka</b></sub></a><br /><a href="#research-Xlinka" title="Research">🔬</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/bontebok"><img src="https://avatars.githubusercontent.com/u/23562523?v=4?s=100" width="100px;" alt="Rucio"/><br /><sub><b>Rucio</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues?q=author%3Abontebok" title="Bug reports">🐛</a></td>
      <td align="center" valign="top" width="14.28%"><a href="http://www.xeltalania.com"><img src="https://avatars.githubusercontent.com/u/19335111?v=4?s=100" width="100px;" alt="Samuel-Sann Laurin"/><br /><sub><b>Samuel-Sann Laurin</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues?q=author%3ASectOLT" title="Bug reports">🐛</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/rampa3"><img src="https://avatars.githubusercontent.com/u/68955305?v=4?s=100" width="100px;" alt="rampa3"/><br /><sub><b>rampa3</b></sub></a><br /><a href="#translation-rampa3" title="Translation">🌍</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/ThomFox"><img src="https://avatars.githubusercontent.com/u/137287064?v=4?s=100" width="100px;" alt="ThomFox"/><br /><sub><b>ThomFox</b></sub></a><br /><a href="#research-ThomFox" title="Research">🔬</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/5H4D0W-X"><img src="https://avatars.githubusercontent.com/u/99607717?v=4?s=100" width="100px;" alt="5H4D0W-X"/><br /><sub><b>5H4D0W-X</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues?q=author%3A5H4D0W-X" title="Bug reports">🐛</a></td>
      <td align="center" valign="top" width="14.28%"><a href="http://probableprime.co.uk"><img src="https://avatars.githubusercontent.com/u/8791132?v=4?s=100" width="100px;" alt="ProbablePrime"/><br /><sub><b>ProbablePrime</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues?q=author%3AProbablePrime" title="Bug reports">🐛</a></td>
    </tr>
  </tbody>
</table>

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->

# Contributors from this Repository

Thank you to the following people who contributed towards this repository!

<div style="display:flex">
  <div style="display:flex; flex-direction:column;align-items:center">
    <a style="display:flex; flex-direction:column;align-items:center;" href="https://github.com/Edward205">
      <img src="https://avatars.githubusercontent.com/u/52004020?s=100&v=4" width="100px;" alt="Edward205">
      <strong>Edward205</strong>
    </a>
    Translation (Romanian)
  </div>
</div>