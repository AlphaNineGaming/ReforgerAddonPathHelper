# Reforger Addon Folder Helper

A small Windows utility for moving Arma Reforger Workshop downloads to a folder on another drive.

## Download

Open the [`release`](release) folder and download all four files to the same folder. Run `ReforgerAddonPathHelper.exe`.

The prebuilt app targets Windows x64 and requires the **.NET 8 Desktop Runtime**. It does not install .NET automatically.

## Use

1. Choose a folder on a drive that is available whenever you play, such as `F:\addons`.
2. Click **Check folder** to confirm that it exists and is writable.
3. Click **Save to Steam** to write the launch options, or **Copy command** to set them yourself.

The app uses:

```text
-addonDownloadDir "F:\addons" -addonsDir "F:\addons"
```

Steam must be fully exited before using **Save to Steam**. The helper backs up Steam's `localconfig.vdf` before updating Arma Reforger's launch options. It preserves unrelated launch options and replaces previous `-addonDownloadDir`/`-addonsDir` values. Restart Steam before launching the game.

The folder parameter is the download root; Reforger places downloaded content in its `addons` subfolder. Existing mods are not moved automatically.

## Build from source

Install the .NET 8 SDK and run from the repository root:

```powershell
dotnet publish .\src\ReforgerAddonPathHelper\ReforgerAddonPathHelper.csproj -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=false -o .\release
```

## License

MIT. See [LICENSE](LICENSE).
