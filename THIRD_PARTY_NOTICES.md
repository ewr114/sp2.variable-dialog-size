# Third-party notices

The release archive bundles the official **BepInEx 5.4.23.5** Windows x64 runtime. BepInEx and its
components are third-party software, distributed under their own licenses. This repository does not
claim ownership of them.

| Component | License | Project |
| --- | --- | --- |
| BepInEx (BepInEx.dll, BepInEx.Preloader.dll, BepInEx.Harmony.dll) | LGPL-2.1 | https://github.com/BepInEx/BepInEx |
| Doorstop / winhttp.dll | LGPL-2.1 | https://github.com/NeighTools/UnityDoorstop |
| HarmonyX (0Harmony.dll) | MIT | https://github.com/BepInEx/HarmonyX |
| MonoMod (MonoMod.Utils, MonoMod.RuntimeDetour) | MIT | https://github.com/MonoMod/MonoMod |
| Mono.Cecil | MIT | https://github.com/jbevain/cecil |

The bundled runtime is redistributed unmodified. The SHA-256 of the official BepInEx archive it is
taken from is pinned in `package.ps1` and in the README.

BepInEx's LGPL-2.1 license text can be found in the upstream repository:
https://github.com/BepInEx/BepInEx/blob/master/LICENSE
