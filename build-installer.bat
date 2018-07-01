:: Before running build
:: --------------------
:: 1. Update version number in InstallerDefinition.iss
:: 2. Ensure path to devenv.com is correct (changes with different versions of Visual Studio, e.g. 2017 Community)
:: 2. Ensure `npm install` has been executed in WcfPad.UI/public/src

gulp all --cwd WcfPad.UI/public/src && echo Starting devenv rebuild... && "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.com" WcfPad.sln /Rebuild Release && "C:\Program Files (x86)\Inno Setup 5\ISCC.exe" InstallerDefinition.iss