# CamBinder

A small Windows utility to quickly bind PDFs together. Select two or more PDF files in Explorer, right-click, and choose **Cambine** to merge them into a single `M-Complete.pdf` in the same folder (auto-numbered as `M-Complete (1).pdf`, etc. if that name is already taken).

## Solution layout
- `src/CamBinder.Core` — merge logic (`PdfMerger`) and output-naming logic (`OutputPathResolver`), built on [PDFsharp](https://www.pdfsharp.net/).
- `src/CamBinder.App` — the WPF app. Reads the selected PDF paths as command-line arguments, shows a minimal always-on-top indicator while merging, and exits automatically when done.
- `installer/CamBinder.iss` — Inno Setup script that packages a self-contained build and registers the "Cambine" Explorer context menu entry for `.pdf` files.

## Building
```
dotnet build
```

## Publishing + building the installer
```
dotnet publish src\CamBinder.App\CamBinder.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish\CamBinder.App
iscc installer\CamBinder.iss
```
This produces `publish\installer\CamBinderSetup.exe`. Running it (as admin) installs CamBinder to Program Files and registers the "Cambine" right-click menu for all users.

## Notes
- The merge indicator's look is a placeholder (a pulsing circle that changes color shortly before the file is written) — intended to be replaced with real branding/animation later.
- The context menu is registered under `HKEY_CLASSES_ROOT\SystemFileAssociations\.pdf\shell\CamBind` using `MultiSelectModel=Player`, so selecting multiple PDFs launches CamBinder once with every selected file passed as an argument.
