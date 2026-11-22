# Third-party notices and corresponding sources

Application source is **GPL-2.0-or-later**; see [LICENSE](LICENSE). This project is independent of its dependency authors and the console/capture-card manufacturers. The app icon is original project artwork, supplied as editable SVG and a multi-resolution ICO in `src/assets/`.

## Components in the Windows package

| Component | Version / source | License materials |
| --- | --- | --- |
| Microsoft .NET / Windows Desktop | 10.0.15, SDK 10.0.404 | `licenses/DOTNET-*` and `licenses/WINDOWS-DESKTOP-LICENSE.txt`; upstream runtime/WinForms notices |
| LibVLC / VLC native libraries, plugins, Lua and HRTF data | Official VLC 3.0.24 Windows x64 ZIP; native changeset `6de05adcba` | GPL-2.0-or-later / LGPL-2.1-or-later plus individual bundled-library licenses; original `COPYING`, `AUTHORS`, `THANKS` in `libvlc/win-x64/`; additional originals in `licenses/upstream/` |
| LibVLCSharp | 3.10.2, commit `3082c09ca955131c705fc4767fde03e9c4a14999` | LGPL-2.1-or-later; `licenses/LGPL-2.1.txt` and original source notices |
| DirectShowLib.Standard | 2.1.0, source at commit `51c8732d0322d43c142d05d69fa76ae5176a200e` | LGPL-2.1; original license and source notices |
| NAudio.Wasapi / NAudio.Core | 2.2.1, commit `b5d5ff83fd378f046398891fe5cd99426ce44732` | MIT; `licenses/NAUDIO-MIT.txt` and original source notices |

The native plugins contain or link additional open-source libraries; do not interpret the top-level VLC license as replacing their individual copyright notices. The package retains upstream license/copyright/notice files, and the accompanying source bundle retains originals and build patches. In particular, the HRTF data carries the 2017 3D Sound Labs LGPL-2.1-or-later notice; it is included unchanged with its corresponding source data in VLC.

## Sources accompanying this Release

Download these from the [same Release](https://github.com/gbaba86/usb-capture-viewer/releases/tag/v1.0.0-beta.2) as the binary:

- `USB-Capture-Viewer-v1.0.0-beta.2-source.zip`: this application's editable source, resources, build script, documentation and dependency lockfile.
- [Unchanged upstream corresponding-source ZIP from beta.1](https://github.com/gbaba86/usb-capture-viewer/releases/download/v1.0.0-beta.1/USB-Capture-Viewer-v1.0.0-beta.1-corresponding-sources.zip): VLC 3.0.24 release source and the source snapshot matching the native build's changeset, its contrib build recipes/patches and associated library source archives, plus LibVLCSharp, DirectShowLib and NAudio source snapshots. Beta.2 uses identical upstream binaries. SHA256: `a101d6e6bd33b36a0700481bffad7e00e9194ec52be832faa0a375aee4668de7`.
- `SHA256SUMS.txt`: release-asset checksums using filenames only.

The contrib collection includes library archives referenced by VLC's matching build recipes. Compiler/operating-system interface headers and non-Windows X11 packages are not separately mirrored as individual header files; a MinGW source archive and VLC build recipes are supplied. Installer tools not included in this application are not part of the shipped runtime. No private local build tree, credentials or tester data is included.

Keep the corresponding source and license materials available with redistributed GPL/LGPL binaries. Libraries are dynamically linked and remain replaceable by compatible modified builds. The app does not enforce signatures/DRM against replacement. No patent-clearance or universal jurisdictional-compatibility claim is made here.

## Upstream locations

- [VideoLAN legal and redistribution guidance](https://www.videolan.org/legal.html)
- [Official native ZIP](https://download.videolan.org/pub/videolan/vlc/3.0.24/win64/vlc-3.0.24-win64.zip), SHA-256 `fcf30850371ad10c9373cc4f0f4501e7dee49e3e9ae9f20c72fb2661a1ca6323`
- [VLC release source](https://download.videolan.org/pub/videolan/vlc/3.0.24/vlc-3.0.24.tar.xz), [matching native source](https://github.com/videolan/vlc/tree/6de05adcba), [contrib source cache](https://download.videolan.org/pub/contrib/)
- [LibVLCSharp](https://github.com/videolan/libvlcsharp/tree/3082c09ca955131c705fc4767fde03e9c4a14999)
- [DirectShowLib](https://github.com/luthfiampas/DirectShowLib/tree/51c8732d0322d43c142d05d69fa76ae5176a200e)
- [NAudio](https://github.com/naudio/NAudio/tree/b5d5ff83fd378f046398891fe5cd99426ce44732)
- [.NET runtime](https://github.com/dotnet/runtime), [Windows Forms](https://github.com/dotnet/winforms)

## ภาษาไทย

Source แอปใช้ GPL-2.0-or-later ส่วนไลบรารีและข้อมูลที่รวมมามีสิทธิ์ของตนเองตามตาราง เก็บ license, copyright และ notice ต้นฉบับไว้ในชุดแจกจ่าย ไม่ใช่สรุปว่าทุกส่วนใช้ license เดียวกับ VLC

Release นี้มี source แอปที่แก้ไขได้และ SHA-256 ส่วน corresponding-source bundle ของ VLC/ไลบรารีและ build patches ใช้ไฟล์ beta.1 ที่ลิงก์ด้านบน เนื่องจากไลบรารีเดิมไม่เปลี่ยน เมื่อแจกต่อส่วน GPL/LGPL ให้คง source และข้อมูลสิทธิ์ที่เกี่ยวข้อง สามารถแทนไลบรารี dynamic ด้วยรุ่นดัดแปลงที่เข้ากันได้ ไม่มี DRM ขัดขวาง เอกสารนี้ไม่ได้อ้างว่าผ่านการตรวจสิทธิบัตรหรือกฎหมายทุกประเทศ
