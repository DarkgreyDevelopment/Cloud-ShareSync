# Local Documentation Development
[docs.cloud-sharesync.com](https://docs.cloud-sharesync.com) is hosted on github pages and is generated using [docfx](https://dotnet.github.io/docfx/) from markdown pages housed in the [github repository](https://main.cloud-sharesync.com).  
Docfx also generates the API documentation based on the XML documentation in the code itself.  

You can test how documentation changes look locally prior to pushing any commits to github.  
To host the docs page locally you must
1. Download a copy of [DocFX](https://github.com/dotnet/docfx/releases/latest/download/docfx.zip) and extract it.
2. Clone the repo `git clone https://git.cloud-sharesync.com`
3. Generate API metadata: `docfx.exe metadata Cloud-ShareSync\docs\docfx.json`
4. Serve the website `docfx.exe Cloud-ShareSync\docs\docfx.json -t templates/CloudShareSync --serve`