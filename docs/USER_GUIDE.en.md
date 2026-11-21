# User guide — English

[ภาษาไทย](USER_GUIDE.th.md) · [Home](../README.md) · [Downloads](https://github.com/gbaba86/usb-capture-viewer/releases/tag/v1.0.0-beta.1)

## 1. Requirements and download

- A Windows x64 PC, a compatible USB HDMI capture card, the console/dock and its power supply, HDMI cable and USB cable.
- Tested combination: Nintendo Switch 2 with AVerMedia Live Gamer MINI GC311. Other combinations are not certified. This app displays common SDR capture formats up to 1080p60 in a 16:9 preview.
- The portable build contains .NET 10 and the native media libraries. You do not need Python, a separate .NET runtime, VLC or OBS.
- Download `USB-Capture-Viewer-v1.0.0-beta.1-win-x64.zip` from Releases. Extract **all** files before opening `SwitchCaptureViewer.exe`. Do not move just the EXE away from the DLLs and subfolders.
- The preview build is unsigned. Verify `SHA256SUMS.txt`; do not disable Windows security settings. If security software blocks it, do not bypass that protection unless you independently trust the file.

To check the download in PowerShell:

```powershell
Get-FileHash .\USB-Capture-Viewer-v1.0.0-beta.1-win-x64.zip -Algorithm SHA256
```

Compare the result with the filename's entry in `SHA256SUMS.txt` from the same Release. A matching checksum checks download integrity; it does not establish that software is safe.

## 2. Connect the hardware

1. Power the console/dock as its manufacturer requires.
2. Connect console/dock HDMI output to **HDMI IN** on the capture card.
3. Connect the capture card's USB output to the PC.
4. HDMI OUT/pass-through is optional according to the card. It is not required for the tested GC311 preview path.
5. Close any other application actively using that capture device, such as an OBS capture source. This app does not close other apps for you.
6. Keep the console awake. A controller-pairing screen is console output: use the console controller to continue. The PC keyboard does not control the game through this viewer.

## 3. Open and choose devices

Open `SwitchCaptureViewer.exe`. It connects to saved inputs automatically. On first use, a detected **Live Gamer MINI** and its matching **Digital Audio Interface (Live Gamer MINI)** are suggested and connected automatically.

For another capture card, use **อุปกรณ์วิดีโอ** (video input) and **อุปกรณ์เสียงเข้า** (audio input) once. Choose the card's HDMI audio, which may have a different name from its video. Choose **ไม่รับเสียง** for video only. The app remembers the selection. Changing an input reconnects the preview.

If it cannot identify a capture device confidently, it waits for a selection. It does not substitute a personal webcam/microphone when the saved card disappears. Devices with identical friendly names remain a limitation; avoid attaching duplicate-named devices.

## 4. Play and control the window

- **F11** or **เต็มจอ**: toggle fullscreen, hiding the menu, header and status bar.
- **Esc**: leave fullscreen. The aspect is 16:9 with letterboxing; it is not stretched to the window shape.
- **ซ่อนเมนู / แสดงเมนู** at the top right: hide/show the side controls. **Tab** does this when focus is outside device lists.
- Volume slider: adjust this app's output. **ปิดเสียง** or **M** mutes/unmutes when focus is outside device lists.
- Audio plays to the Windows default output selected when the audio stream opens. If that output changes, use **เชื่อมต่อใหม่** (Reconnect).
- Closing the app releases both inputs. To update the portable folder, close that copy of the app first.

## 5. Advanced capture settings

Open **ตั้งค่าภาพ / เสียง** (Capture settings). The app temporarily stops capture while the dialog is open and reconnects when it closes. Use **อ่านโหมดจากอุปกรณ์** (Read device modes) to inspect the selected driver's advertised types without running a capture graph for that inspection.

Choose resolution, FPS and pixel format from compatible combinations. The list contains common SDR formats up to 1920×1080 at 60 FPS. It includes advertised default rates and range endpoints, not every possible intermediate rate. A driver reporting a mode does not prove the attached HDMI source supports it.

**กลับไปใช้ Auto** lets the driver negotiate a mode. Auto does not promise 1080p60. **บันทึก** saves; **ยกเลิก** cancels changes. **เลือก 1080p/60 ถ้ามี** selects a reported 1080p60 option if available.

The tested GC311 mode is **1920×1080 / 60 / MJPG**, using **50 ms video cache**. Higher 100/200 ms video-cache choices may help video stability but add video delay. Audio is separate WASAPI shared-mode capture/output with bounded queues. No extra audio delay is added to follow video.

The cache value and requested audio buffers are **not measured total latency**. A capture card, USB transport, drivers, decoding, rendering, Windows audio and the display add their own delay. Do not expect zero latency. CPU/GPU activity alone is not an end-to-end latency measurement.

## 6. GC311 and Switch 2 starting settings

On the console itself:

- System Settings → Display → TV Resolution: **1080p**.
- 120 Hz Output: **Off**. HDR Output: **Disabled / SDR**.
- RGB Range: **Automatic** as a starting point.
- Audio → TV Sound: **Stereo**.
- Inspect the console's Video Output Information if available to confirm the negotiated signal.

These are starting settings for this tested combination. The GC311's specified maximum is 1080p60; it does not support 4K, HDR or high-refresh-rate capture/pass-through. USB capture settings in this app are separate from console HDMI output. The app cannot change the console settings directly.

[AVerMedia GC311](https://www.avermedia.com/product-detail/GC311) · [GC311 specifications](https://storage.avermedia.com/web_release_www/GC311/EN_GC331%20DS_20190215-min.pdf) · [Nintendo display settings](https://www.nintendo.com/en-gb/Support/Troubleshooting/How-to-Adjust-the-Display-Settings-on-Nintendo-Switch-2-2889732.html)

## 7. Troubleshooting

**No device:** check USB/power and choose the correct inputs. The app waits for its saved device; **เชื่อมต่อใหม่** retries. It does not install drivers or change Windows privacy settings.

**Black/No Signal:** check HDMI IN versus OUT, dock power, console sleep and compatible SDR output. Stop competing capture software. Try Auto, then a lower reported mode such as 720p60 MJPG. A black game scene is not proof of HDMI loss.

**No or broken audio:** choose the card's HDMI audio input, unmute, raise the app volume and check the Windows output/volume mixer. Use Stereo on the console. Enter a game with actual sound; pairing screens can be silent. Reconnect after changing the Windows output. The live audio path is independent of the video cache, so raising video cache is not an audio fix.

**Undock/redock:** allow HDMI negotiation to settle. Frame stalls/device errors trigger reconnect attempts with backoff from 3 to 60 seconds. The tester repeatedly undocked/redocked the Switch 2 successfully. A card that continues sending black/No Signal frames may require the Reconnect button; the app does not restart just because a game scene is dark. USB hotplug and every hardware combination are not certified.

**Noticeable delay:** keep MJPG and the tested 50 ms video cache first. Avoid large queues, overloaded USB links and competing capture apps. Hardware pass-through to a separate display can have a different latency from USB preview. Some hardware delay remains; no precise input-to-photon result has been measured for this app.

## 8. Local data and reports

Settings and bounded diagnostics live under `%LOCALAPPDATA%\SwitchCaptureViewer` (`settings.json`, `viewer.log`, `capture-status.json`; optional inspection/self-test files). Nothing is uploaded automatically by this app.

These files can contain device names/identifiers, local paths and error details. Review and redact them before sharing. Do not post captured private video/audio/screenshots or credentials in issues. Public releases are built from a clean staging area and do not include the tester's local data, desktop shortcut or debug symbols. See [privacy](PRIVACY.md), [testing](TESTING.md), and [licensing](../THIRD_PARTY_NOTICES.md).


Hardware feedback refers to the earlier local baseline; the public .NET 10 runtime upgrade was rechecked with synthetic video/audio, without a new hardware capture session.
