# Elden Banner (Portable C# Version)

**GitHub:** https://github.com/Gargantubrain/EldenBanner

A standalone C# application that replicates the "Elden Ring" message banner style. It creates a transparent, click-through overlay on your screen, plays a sound, and animates text before automatically closing.

## Prerequisites

- **Windows OS** with .NET Framework 4.8 (standard on Windows 10/11)
- **.NET SDK** for building (download from https://dotnet.microsoft.com/download if needed)
- **Assets**: Place `Mantinia.otf` and `elden_ring_sound.mp3` in the same folder as the built executable

**Asset Credits:**
- Font: [Mantinia](https://font.download/font/mantinia) - Free for commercial use
- Sound: From [MettiFire/elden_mail_banner](https://github.com/MettiFire/elden_mail_banner), downsampled with `lame --preset medium`

## How to Compile

1. Open **PowerShell** or **Command Prompt**
2. Navigate to the folder containing `EldenBanner.cs`
3. Run:

   ```powershell
   dotnet build -c Release
   ```

4. The executable will be in `bin\Release\net48\EldenBanner.exe`
5. Copy `Mantinia.otf` and `elden_ring_sound.mp3` to that folder

## How to Use

1. Ensure `EldenBanner.exe`, `Mantinia.otf`, and `elden_ring_sound.mp3` are in the same folder.
2. Double-click `EldenBanner.exe` to see the default help message banner.

### Command Line Customization

You can invoke the app from a terminal, shortcut, or batch script with custom text.

**Valid Usage:**

```
.\EldenBanner.exe "DEPLOYMENT COMPLETE"
.\EldenBanner.exe "TIME TO STRETCH" --nosound
```

Help & Usage Info:

Run with --help or /? to see the usage dialog:

```
.\EldenBanner.exe --help
```

Common Errors:

If you see the usage dialog unexpectedly, ensure you are using quotes for multi-word messages:

- ❌ Wrong: `.\EldenBanner.exe HELLO WORLD`
- ✅ Right: `.\EldenBanner.exe "HELLO WORLD"`